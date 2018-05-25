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
    public class SecureHttpClientHandler : HttpClientHandler, Abstractions.ISecureHttpClientHandler
    {
        internal readonly Dictionary<NSUrlSessionTask, InflightOperation> InflightRequests;
        internal NSUrlCredential ClientCertificate { get; private set; }
        private readonly Lazy<CertificatePinner> _certificatePinner;
        private NSUrlSession _session;
        
        public SecureHttpClientHandler(ILogger logger = null)
        {
            InflightRequests = new Dictionary<NSUrlSessionTask, InflightOperation>();
            _certificatePinner = new Lazy<CertificatePinner>(() => new CertificatePinner(logger));
        }

        public void AddCertificatePinner(string hostname, string[] pins)
        {
            _certificatePinner.Value.AddPins(hostname, pins);
        }

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
