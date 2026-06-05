using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Devices;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Quic;
using System.Text.Json;
using System.Threading.Tasks;
using SecureHttpClient.Test.Helpers;
using Xunit;

namespace SecureHttpClient.Test
{
    public class TestBase
    {
        private readonly SecureHttpClientHandler _secureHttpClientHandler;
        private readonly HttpClient _httpClient;

        protected TestBase(TestFixture fixture)
        {
            _secureHttpClientHandler = (fixture.ServiceProvider.GetRequiredService<HttpClientHandler>() as SecureHttpClientHandler)!;
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

        protected static void SkipIfHttp3NotSupported()
        {
            // Android: OkHttp does not support HTTP/3 yet.
            Skip.If(DeviceInfo.Platform == DevicePlatform.Android, "HTTP/3 is not yet supported on Android.");
            // Net: requires OS-level QUIC support (Windows 11 / Windows Server 2022+).
            // iOS: NSURLSession handles HTTP/3 natively — no .NET QUIC stack involved, no skip needed.
            Skip.If(DeviceInfo.Platform != DevicePlatform.iOS && !QuicConnection.IsSupported, "HTTP/3 (QUIC) is not supported on this platform.");
        }

        /// <summary>
        /// Skips the test on platforms where HttpRequestMessage.VersionPolicy is not honoured
        /// (iOS uses NSURLSession, Android uses OkHttp — both manage version negotiation internally).
        /// </summary>
        protected static void SkipIfNotNetPlatform()
        {
            Skip.If(DeviceInfo.Platform == DevicePlatform.iOS || DeviceInfo.Platform == DevicePlatform.Android,
                "HttpRequestMessage.VersionPolicy is not supported on this platform.");
        }

        protected async Task<Dictionary<string, string>> GetCookiesAsync(string page)
        {
            var result = await GetAsync(page).ReceiveString();
            var json = JsonDocument.Parse(result);
            var cookies = json.RootElement.GetProperty("cookies").GetDictionary();
            return cookies;
        }
    }
}
