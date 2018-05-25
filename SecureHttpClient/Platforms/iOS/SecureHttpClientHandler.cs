#if __IOS__

using System;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using Foundation;
using Microsoft.Extensions.Logging;
using SecureHttpClient.CertificatePinning;
using Security;

namespace SecureHttpClient
{
    /// <summary>
    /// Implementation of ISecureHttpClientHandler (iOS implementation)
    /// </summary>
    public class SecureHttpClientHandler : HttpClientHandler, Abstractions.ISecureHttpClientHandler
    {
        internal readonly Dictionary<NSUrlSessionTask, InflightOperation> InflightRequests;
        internal NSUrlCredential ClientCertificate { get; private set; }
        private readonly Lazy<CertificatePinner> _certificatePinner;
        private NSUrlSession _session;

        /// <summary>
        /// SecureHttpClientHandler constructor (iOS implementation)
        /// </summary>
        /// <param name="logger">Optional logger</param>
        public SecureHttpClientHandler(ILogger logger = null)
        {
            InflightRequests = new Dictionary<NSUrlSessionTask, InflightOperation>();
            _certificatePinner = new Lazy<CertificatePinner>(() => new CertificatePinner(logger));
        }

        /// <summary>
        /// Add certificate pins for a given hostname (iOS implementation)
        /// </summary>
        /// <param name="hostname">The hostname</param>
        /// <param name="pins">The array of certifiate pins (example of pin string: "sha256/fiKY8VhjQRb2voRmVXsqI0xPIREcwOVhpexrplrlqQY=")</param>
        public void AddCertificatePinner(string hostname, string[] pins)
        {
            _certificatePinner.Value.AddPins(hostname, pins);
        }

        /// <summary>
        /// Set a client certificate (iOS implementation)
        /// </summary>
        /// <param name="certificate">The client certificate raw data</param>
        /// <param name="passphrase">The client certificate pass phrase</param>
        public void SetClientCertificate(byte[] certificate, string passphrase)
        {
            NSDictionary opt;
            if (string.IsNullOrEmpty(passphrase))
            {
                opt = new NSDictionary();
            }
            else
            {
                opt = NSDictionary.FromObjectAndKey(new NSString(passphrase), new NSString("passphrase"));
            }

            NSDictionary[] array;
            var status = SecImportExport.ImportPkcs12(certificate, opt, out array);

            if (status == SecStatusCode.Success)
            {
                var identity = new SecIdentity(array[0]["identity"].Handle);
                SecCertificate[] certs = { identity.Certificate };
                ClientCertificate = new NSUrlCredential(identity, certs, NSUrlCredentialPersistence.ForSession);
            }
        }

        private void InitSession()
        {
            using (var configuration = NSUrlSessionConfiguration.DefaultSessionConfiguration)
            {
                var nsUrlSessionDelegate = (INSUrlSessionDelegate) new DataTaskDelegate(this, _certificatePinner.IsValueCreated ? _certificatePinner.Value : null);
                _session = NSUrlSession.FromConfiguration(configuration, nsUrlSessionDelegate, null);
            }
        }

        /// <inheritdoc />
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (_session == null)
            {
                InitSession();
            }

            var headers = request.Headers as IEnumerable<KeyValuePair<string, IEnumerable<string>>>;
            var ms = new MemoryStream();

            if (request.Content != null)
            {
                await request.Content.CopyToAsync(ms).ConfigureAwait(false);
                headers = headers.Union(request.Content.Headers).ToArray();
            }

            var rq = new NSMutableUrlRequest
            {
                AllowsCellularAccess = true,
                Body = NSData.FromArray(ms.ToArray()),
                CachePolicy = NSUrlRequestCachePolicy.UseProtocolCachePolicy,
                Headers = headers.Aggregate(new NSMutableDictionary(), (acc, x) => {
                    acc.Add(new NSString(x.Key), new NSString(string.Join(x.Key == "User-Agent" ? " " : ",", x.Value)));
                    return acc;
                }),
                HttpMethod = request.Method.ToString().ToUpperInvariant(),
                Url = NSUrl.FromString(request.RequestUri.AbsoluteUri),
            };

            var op = _session.CreateDataTask(rq);

            cancellationToken.ThrowIfCancellationRequested();

            var ret = new TaskCompletionSource<HttpResponseMessage>();
            cancellationToken.Register(() => ret.TrySetCanceled());

            lock (InflightRequests)
            {
                InflightRequests[op] = new InflightOperation
                {
                    FutureResponse = ret,
                    Request = request,
                    ResponseBody = new ByteArrayListStream(),
                    CancellationToken = cancellationToken,
                };
            }

            op.Resume();
            return await ret.Task.ConfigureAwait(false);
        }
    }
}

#endif
