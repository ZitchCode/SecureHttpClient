using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using SecureHttpClient.Abstractions;

namespace SecureHttpClient.Test
{
    public class TestBase
    {
        private readonly ISecureHttpClientHandler _secureHttpClientHandler;
        private readonly HttpClient _httpClient;

        protected TestBase(TestFixture fixture)
        {
            var httpClientHandler = fixture.ServiceProvider.GetRequiredService<HttpClientHandler>();
            _secureHttpClientHandler = httpClientHandler as SecureHttpClientHandler;
            _httpClient = new HttpClient(httpClientHandler);
        }

        protected async Task<HttpResponseMessage> GetAsync(string page)
        {
            var response = await _httpClient.GetAsync(page).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            return response;
        }

        protected async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
        {
            var response = await _httpClient.SendAsync(request).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
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

        protected void SetTimeout(int timeout)
        {
            _httpClient.Timeout = TimeSpan.FromSeconds(timeout);
        }
    }
}
