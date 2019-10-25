using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Serilog;

namespace SecureHttpClient.Test
{
    public class TestBase
    {
        private readonly SecureHttpClientHandler _secureHttpClientHandler;
        private readonly HttpClient _httpClient;

        public TestBase()
        {
            var logger = new LoggerFactory().AddSerilog().CreateLogger(nameof(SecureHttpClientHandler));
            _secureHttpClientHandler = new SecureHttpClientHandler(logger);
            _httpClient = new HttpClient(_secureHttpClientHandler);
        }

        public async Task<HttpResponseMessage> GetAsync(string page)
        {
            var response = await _httpClient.GetAsync(page).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            return response;
        }

        public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
        {
            var response = await _httpClient.SendAsync(request).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            return response;
        }

        public void AddCertificatePinner(string hostname, string[] pins)
        {
            _secureHttpClientHandler.AddCertificatePinner(hostname, pins);
        }

        public void SetClientCertificate(string clientCertBase64, string certPassword)
        {
            var clientCert = Convert.FromBase64String(clientCertBase64);
            var provider = new ImportedClientCertificateProvider();
            provider.Import(clientCert, certPassword);
            _secureHttpClientHandler.SetClientCertificates(provider);
        }

        public void SetCaCertificate(string caCertEncoded)
        {
            var caCert = System.Text.Encoding.ASCII.GetBytes(caCertEncoded);
            _secureHttpClientHandler.SetTrustedRoots(caCert);
        }
    }
}
