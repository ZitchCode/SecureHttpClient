using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using Square.OkHttp3;
using Java.IO;
using Android.OS;
using Java.Util.Concurrent;
using Java.Security;
using Javax.Net.Ssl;

namespace SecureHttpClient
{
    public class SecureHttpClientHandler : HttpClientHandler, Abstractions.ISecureHttpClientHandler
    {
        private static readonly Lazy<OkHttpClient> OkHttpClientInstance = new Lazy<OkHttpClient>(CreateOkHttpClientInstance);

        private readonly OkHttpClient.Builder _builder;
        private OkHttpClient _client;
        private readonly Lazy<CertificatePinner.Builder> _certificatePinnerBuilder;

        public SecureHttpClientHandler()
        {
            _builder = OkHttpClientInstance.Value.NewBuilder().CookieJar(new NativeCookieJar());
            _certificatePinnerBuilder = new Lazy<CertificatePinner.Builder>();
        }

        public void AddCertificatePinner(string hostname, string[] pins)
        {
            System.Diagnostics.Debug.WriteLine($"Add CertificatePinner: hostname:{hostname}, pins:{string.Join("|", pins)}");
            var certificatePinner = _certificatePinnerBuilder.Value.Add(hostname, pins).Build();
            _builder.CertificatePinner(certificatePinner);
        }

        public void SetClientCertificate(byte[] certificate, string passphrase) {
            KeyStore keyStore = KeyStore.GetInstance("pkcs12");
            keyStore.Load(new System.IO.MemoryStream(certificate), passphrase.ToCharArray());
            var keyManagerFactory = KeyManagerFactory.GetInstance("X509");
            keyManagerFactory.Init(keyStore, passphrase.ToCharArray());
            if ((int)Build.VERSION.SdkInt < 21)
            {
                _builder.SslSocketFactory(new TlsSslSocketFactory(keyManagerFactory), TlsSslSocketFactory.GetSystemDefaultTrustManager());
            }
            else
            {
                SSLContext context = SSLContext.GetInstance("TLS");
                context.Init(keyManagerFactory.GetKeyManagers(), null, null);
                _builder.SslSocketFactory(context.SocketFactory, TlsSslSocketFactory.GetSystemDefaultTrustManager());
            }
        }

        private static OkHttpClient CreateOkHttpClientInstance()
        {
            var builder = new OkHttpClient.Builder()
                .ConnectTimeout(100, TimeUnit.Seconds)
                .WriteTimeout(100, TimeUnit.Seconds)
                .ReadTimeout(100, TimeUnit.Seconds);
            builder.CookieJar(new NativeCookieJar());
            if ((int)Build.VERSION.SdkInt < 21)
            {
                // Support TLS1.2 on Android versions before Lollipop
                builder.SslSocketFactory(new TlsSslSocketFactory(), TlsSslSocketFactory.GetSystemDefaultTrustManager());
            }
            return builder.Build();
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (_client == null)
            {
                _client = _builder.Build();
            }

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
            var call = _client.NewCall(rq);

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
