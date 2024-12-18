#if __IOS__

using System;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using Foundation;
using Microsoft.Extensions.Logging;
using SecureHttpClient.CertificatePinning;
using System.Security.Cryptography.X509Certificates;

namespace SecureHttpClient
{
    /// <summary>
    /// Implementation of ISecureHttpClientHandler (iOS implementation)
    /// </summary>
    public class SecureHttpClientHandler : HttpClientHandler, Abstractions.ISecureHttpClientHandler
    {
        internal readonly Dictionary<NSUrlSessionTask, InflightOperation> InflightRequests;
        internal NSUrlCredential ClientCertificate { get; private set; }
        private X509Certificate2Collection _trustedRoots = null;
        private readonly Lazy<CertificatePinner> _certificatePinner;
        private NSUrlSession _session;

        /// <summary>
        /// SecureHttpClientHandler constructor (iOS implementation)
        /// </summary>
        /// <param name="logger">Logger</param>
        public SecureHttpClientHandler(ILogger<Abstractions.ISecureHttpClientHandler> logger)
        {
            InflightRequests = new Dictionary<NSUrlSessionTask, InflightOperation>();
            _certificatePinner = new Lazy<CertificatePinner>(() => new CertificatePinner(logger));
        }

        /// <summary>
        /// Add certificate pins for a given hostname (iOS implementation)
        /// </summary>
        /// <param name="hostname">The hostname</param>
        /// <param name="pins">The array of certifiate pins (example of pin string: "sha256/fiKY8VhjQRb2voRmVXsqI0xPIREcwOVhpexrplrlqQY=")</param>
        public virtual void AddCertificatePinner(string hostname, string[] pins)
        {
            _certificatePinner.Value.AddPins(hostname, pins);
        }

        /// <summary>
        /// Set the client certificate provider (iOS implementation)
        /// </summary>
        /// <param name="provider">The provider for client certificates on this platform</param>
        public virtual void SetClientCertificates(Abstractions.IClientCertificateProvider provider)
        {
            ClientCertificate = (provider as IClientCertificateProvider)?.Credential;
        }

        /// <summary>
        /// Set certificates for the trusted Root Certificate Authorities (iOS implementation)
        /// </summary>
        /// <param name="certificates">Certificates for the CAs to trust</param>
        public virtual void SetTrustedRoots(params byte[][] certificates)
        {
            if (certificates.Length == 0)
            {
                _trustedRoots = null;
                return;
            }
            _trustedRoots = new X509Certificate2Collection();
            foreach (var cert in certificates)
            {
                _trustedRoots.Add(X509CertificateLoader.LoadCertificate(cert));
            }
        }

        private void InitSession()
        {
            using var configuration = NSUrlSessionConfiguration.DefaultSessionConfiguration;
            if (!UseProxy)
            {
                configuration.ConnectionProxyDictionary = new NSDictionary();
            }
#pragma warning disable CA1416
            else if (Proxy is WebProxy webProxy)
#pragma warning restore CA1416
            {
                configuration.ConnectionProxyDictionary = GetProxyDictionary(webProxy);
            }
            if (!UseCookies)
            {
                configuration.HttpCookieStorage = null;
            }
            var nsUrlSessionDelegate = (INSUrlSessionDelegate)new DataTaskDelegate(this, _certificatePinner.IsValueCreated ? _certificatePinner.Value : null, _trustedRoots);
            _session = NSUrlSession.FromConfiguration(configuration, nsUrlSessionDelegate, null);
        }

        private static NSDictionary GetProxyDictionary(WebProxy webProxy)
        {
            var host = webProxy.Address.Host;
            var port = webProxy.Address.Port;
            var values = new []
            {
                NSObject.FromObject(host),
                NSNumber.FromInt32(port),
                NSNumber.FromInt32(1),
                NSObject.FromObject(host),
                NSNumber.FromInt32(port),
                NSNumber.FromInt32(1)
            };
            var keys = new []
            {
                NSObject.FromObject("HTTPProxy"),
                NSObject.FromObject("HTTPPort"),
                NSObject.FromObject("HTTPEnable"),
                NSObject.FromObject("HTTPSProxy"),
                NSObject.FromObject("HTTPSPort"),
                NSObject.FromObject("HTTPSEnable")
            };
            var proxyDictionary = NSDictionary.FromObjectsAndKeys(values, keys);
            return proxyDictionary;
        }

        /// <inheritdoc />
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (_session == null)
            {
                InitSession();
            }

            var headers = request.Headers as IEnumerable<KeyValuePair<string, IEnumerable<string>>>;

            var body = default(NSData);
            if (request.Content != null)
            {
                var bytes = await request.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
                if (bytes.Length > 0 || request.Method != HttpMethod.Get)
                {
                    body = NSData.FromArray(bytes);
                    headers = headers.Union(request.Content.Headers).ToArray();
                }
            }

            var rq = new NSMutableUrlRequest
            {
                AllowsCellularAccess = true,
                Body = body,
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