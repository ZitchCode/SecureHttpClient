using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace SecureHttpClient.Test
{
    public class TestBase
    {
        private readonly SecureHttpClientHandler _secureHttpClientHandler;
        private readonly HttpClient _httpClient;

        protected TestBase(TestFixture fixture)
        {
            _secureHttpClientHandler = fixture.ServiceProvider.GetRequiredService<HttpClientHandler>() as SecureHttpClientHandler;
            _httpClient = new HttpClient(_secureHttpClientHandler);
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:115.0) Gecko/20100101 Firefox/115.0");
        }

        protected async Task<HttpResponseMessage> GetAsync(string page, bool ensureSuccessStatusCode = true)
        {
            var response = await _httpClient.GetAsync(page).ConfigureAwait(false);
            if (ensureSuccessStatusCode)
            {
                response.EnsureSuccessStatusCode();
            }
            return response;
        }

        protected async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, bool ensureSuccessStatusCode = true)
        {
            var response = await _httpClient.SendAsync(request).ConfigureAwait(false);
            if (ensureSuccessStatusCode)
            {
                response.EnsureSuccessStatusCode();
            }
            return response;
        }

        protected void AddCertificatePinner(string hostname, string[] pins)
        {
            _secureHttpClientHandler.AddCertificatePinner(hostname, pins);
        }

        protected void SetClientCertificate(byte[] clientCert, string certPassword)
        {
            var provider = new ImportedClientCertificateProvider();
            provider.Import(clientCert, certPassword);
            _secureHttpClientHandler.SetClientCertificates(provider);
        }

        protected void SetCaCertificate(string caCertEncoded)
        {
            var caCert = System.Text.Encoding.ASCII.GetBytes(caCertEncoded);
            _secureHttpClientHandler.SetTrustedRoots(caCert);
        }

        protected void DoNotFollowRedirects()
        {
            _secureHttpClientHandler.AllowAutoRedirect = false;
        }

        protected void SetTimeout(int timeout)
        {
            _httpClient.Timeout = TimeSpan.FromSeconds(timeout);
        }

        protected void DisableCookies()
        {
            _secureHttpClientHandler.UseCookies = false;
        }
    }
}
