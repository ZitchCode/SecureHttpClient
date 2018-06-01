#if __ANDROID__

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Android.OS;
using Android.Runtime;
using Java.IO;
using Java.Security;
using Java.Security.Cert;
using Java.Util.Concurrent;
using Javax.Net.Ssl;
using Square.OkHttp3;
using Microsoft.Extensions.Logging;

namespace SecureHttpClient
{
    /// <summary>
    /// Implementation of ISecureHttpClientHandler (Android implementation)
    /// </summary>
    public class SecureHttpClientHandler : HttpClientHandler, Abstractions.ISecureHttpClientHandler
    {
        private readonly Lazy<OkHttpClient> _client;
        private readonly Lazy<CertificatePinner.Builder> _certificatePinnerBuilder;
        private readonly ILogger _logger;
        private KeyManagerFactory _keyMgrFactory;
        private TrustManagerFactory _trustMgrFactory;
        private IX509TrustManager _x509TrustManager;
        private IKeyManager[] _keyManagers => _keyMgrFactory?.GetKeyManagers();
        private ITrustManager[] _trustManagers => _trustMgrFactory?.GetTrustManagers();

        /// <summary>
        /// SecureHttpClientHandler constructor (Android implementation)
        /// </summary>
        /// <param name="logger">Optional logger</param>
        public SecureHttpClientHandler(ILogger logger = null)
        {
            _logger = logger;
            _client = new Lazy<OkHttpClient>(CreateOkHttpClientInstance);
            _certificatePinnerBuilder = new Lazy<CertificatePinner.Builder>();
        }

        /// <summary>
        /// Add certificate pins for a given hostname (Android implementation)
        /// </summary>
        /// <param name="hostname">The hostname</param>
        /// <param name="pins">The array of certifiate pins (example of pin string: "sha256/fiKY8VhjQRb2voRmVXsqI0xPIREcwOVhpexrplrlqQY=")</param>
        public void AddCertificatePinner(string hostname, string[] pins)
        {
            _logger?.LogDebug($"Add CertificatePinner: hostname:{hostname}, pins:{string.Join("|", pins)}");
            _certificatePinnerBuilder.Value.Add(hostname, pins);
        }

        /// <summary>
        /// Set a client certificate (Android implementation)
        /// </summary>
        /// <param name="certificate">The client certificate raw data</param>
        /// <param name="passphrase">The client certificate pass phrase</param>
        public void SetClientCertificate(byte[] certificate, string passphrase)
        {
            KeyStore keyStore = KeyStore.GetInstance("pkcs12");
            keyStore.Load(new System.IO.MemoryStream(certificate), passphrase.ToCharArray());
            _keyMgrFactory = KeyManagerFactory.GetInstance("X509");
            _keyMgrFactory.Init(keyStore, passphrase.ToCharArray());
        }

        /// <summary>
        /// Set certificates for the trusted Root Certificate Authorities
        /// </summary>
        /// <param name="certificates">Certificates for the CAs to trust</param>
        public void SetTrustedRoots(params byte[][] certificates)
        {
            if (certificates == null)
            {
                _trustMgrFactory = null;
                _x509TrustManager = null;
                return;
            }
            KeyStore keyStore = KeyStore.GetInstance(KeyStore.DefaultType);
            keyStore.Load(null);
            var certFactory = CertificateFactory.GetInstance("X.509");
            foreach (var certificate in certificates)
            {
                var cert = (X509Certificate)certFactory.GenerateCertificate(new System.IO.MemoryStream(certificate));
                keyStore.SetCertificateEntry(cert.SubjectDN.Name, cert);
            }

            _trustMgrFactory = TrustManagerFactory.GetInstance(TrustManagerFactory.DefaultAlgorithm);
            _trustMgrFactory.Init(keyStore);
            foreach (var trustManager in _trustManagers)
            {
                _x509TrustManager = trustManager.JavaCast<IX509TrustManager>();
                if (_x509TrustManager != null)
                {
                    break;
                }
            }
        }

        private OkHttpClient CreateOkHttpClientInstance()
        {
            var builder = new OkHttpClient.Builder()
                .ConnectTimeout(100, TimeUnit.Seconds)
                .WriteTimeout(100, TimeUnit.Seconds)
                .ReadTimeout(100, TimeUnit.Seconds)
                .CookieJar(new NativeCookieJar());

            if (_certificatePinnerBuilder.IsValueCreated)
            {
                builder.CertificatePinner(_certificatePinnerBuilder.Value.Build());
            }

            if (Build.VERSION.SdkInt < BuildVersionCodes.Lollipop)
            {
                // Support TLS1.2 on Android versions before Lollipop
                builder.SslSocketFactory(new TlsSslSocketFactory(_keyManagers, _trustManagers), _x509TrustManager ?? TlsSslSocketFactory.GetSystemDefaultTrustManager());
            }
            else if (_keyMgrFactory != null || _trustMgrFactory != null)
            {
                SSLContext context = SSLContext.GetInstance("TLS");
                context.Init(_keyManagers, _trustManagers, null);
                builder.SslSocketFactory(context.SocketFactory, _x509TrustManager ?? TlsSslSocketFactory.GetSystemDefaultTrustManager());
            }

            return builder.Build();
        }

        /// <inheritdoc />
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var javaUri = request.RequestUri.GetComponents(UriComponents.AbsoluteUri, UriFormat.UriEscaped);
            var url = new Java.Net.URL(javaUri);

            var body = default(RequestBody);
            if (request.Content != null)
            {
                var bytes = await request.Content.ReadAsByteArrayAsync().ConfigureAwait(false);

                var contentType = "text/plain";
                if (request.Content.Headers.ContentType != null)
                {
                    contentType = string.Join(" ", request.Content.Headers.GetValues("Content-Type"));
                }

                body = RequestBody.Create(MediaType.Parse(contentType), bytes);
            }

            var builder = new Request.Builder()
                .Method(request.Method.Method.ToUpperInvariant(), body)
                .Url(url);

            var keyValuePairs = request.Headers
                .Union(request.Content != null ?
                    request.Content.Headers :
                    Enumerable.Empty<KeyValuePair<string, IEnumerable<string>>>());

            foreach (var kvp in keyValuePairs)
            {
                var headerSeparator = kvp.Key == "User-Agent" ? " " : ",";
                builder.AddHeader(kvp.Key, string.Join(headerSeparator, kvp.Value));
            }

            cancellationToken.ThrowIfCancellationRequested();

            var rq = builder.Build();
            var call = _client.Value.NewCall(rq);

            // NB: Even closing a socket must be done off the UI thread. Cray!
            cancellationToken.Register(() => Task.Run(() => call.Cancel()));

            Response resp;
            try
            {
                resp = await call.ExecuteAsync().ConfigureAwait(false);
            }
            catch (IOException ex)
            {
                if (ex.Message != null && ex.Message.StartsWith("Certificate pinning failure!"))
                {
                    throw new WebException(ex.Message, WebExceptionStatus.TrustFailure);
                }
                if (ex.Message != null && ex.Message.ToLowerInvariant().Contains("canceled"))
                {
                    throw new System.OperationCanceledException();
                }
                throw;
            }

            var respBody = resp.Body();

            cancellationToken.ThrowIfCancellationRequested();

            var ret = new HttpResponseMessage((HttpStatusCode) resp.Code())
            {
                RequestMessage = request,
                ReasonPhrase = resp.Message()
            };

            if (respBody != null)
            {
                ret.Content = new StreamContent(respBody.ByteStream());
            }
            else
            {
                ret.Content = new ByteArrayContent(new byte[0]);
            }

            var respHeaders = resp.Headers();
            foreach (var k in respHeaders.Names())
            {
                ret.Headers.TryAddWithoutValidation(k, respHeaders.Get(k));
                ret.Content.Headers.TryAddWithoutValidation(k, respHeaders.Get(k));
            }

            return ret;
        }
    }
}

#endif
