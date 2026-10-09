using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using SecureHttpClient.CertificatePinning;
using SecureHttpClient.Test.Helpers;
using Xunit;

namespace SecureHttpClient.Test
{
    public class Http3TestFixture : TestPinFixture
    {
        // Cloudflare supports HTTP/3 reliably and is used for version negotiation tests.
        public string Http3Hostname => "www.cloudflare.com";
        public string Http3Page => "https://www.cloudflare.com/";
    }

    public class Http3Test : TestBase, IClassFixture<Http3TestFixture>
    {
        private readonly Http3TestFixture _fixture;

        public Http3Test(Http3TestFixture fixture) : base(fixture)
        {
            _fixture = fixture;
        }

        // HTTP/3 is negotiated via Alt-Svc: the first request goes over TCP (HTTP/2 or 1.1),
        // then subsequent requests to the same server upgrade to QUIC. The upgrade is best-effort
        // so a few requests may be needed; retry until HTTP/3 is observed.
        [SkippableFact]
        public async Task Http3Test_NegotiatedOnHttp3Server()
        {
            SkipIfHttp3NotSupported();

            var response = await GetUntilVersionAsync(_fixture.Http3Page, HttpVersion.Version30);
            Assert.Equal(HttpVersion.Version30, response.Version);
        }

        [Fact]
        public async Task Http3Test_Http11FallbackOnHttp11Server()
        {
            // httpbingo.org does not advertise Alt-Svc h3, so the version must stay ≤ 2.0
            var response = await GetAsync("https://httpbingo.org/get");
            Assert.True(
                response.Version == HttpVersion.Version11 || response.Version == HttpVersion.Version20,
                $"Expected HTTP/1.1 or HTTP/2, got {response.Version}");
        }

        [SkippableFact]
        public async Task Http3Test_CertificatePinning_WorksWithHttp3()
        {
            SkipIfHttp3NotSupported();

            // Fetch the real pin for Cloudflare then set it — first call goes over TCP so
            // the pin fetch itself does not need QUIC.
            var pin = await CertificateHelper.GetSpkiFingerprintAsync(_fixture.Http3Hostname);
            AddCertificatePinner(_fixture.Http3Hostname, [pin]);

            var response = await GetUntilVersionAsync(_fixture.Http3Page, HttpVersion.Version30);
            Assert.Equal(HttpVersion.Version30, response.Version);
        }

        // HTTP/3 is negotiated on a best-effort basis (Alt-Svc + QUIC handshake), so a few
        // requests may be needed before the connection upgrades from HTTP/2.
        private async Task<HttpResponseMessage> GetUntilVersionAsync(string page, Version expectedVersion, int maxAttempts = 10)
        {
            var response = await GetAsync(page);
            for (var attempt = 1; attempt < maxAttempts && response.Version != expectedVersion; attempt++)
            {
                await Task.Delay(200);
                response = await GetAsync(page);
            }
            return response;
        }

        [SkippableFact]
        public async Task Http3Test_CertificatePinning_WrongPin_WithHttp3()
        {
            SkipIfHttp3NotSupported();

            AddCertificatePinner(_fixture.Http3Hostname, ["sha256/xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx="]);

            await AssertExtensions.ThrowsTrustFailureAsync(() => GetAsync(_fixture.Http3Page));
        }

        // Verify that an explicit per-request version policy is respected even when the handler
        // default would upgrade to a higher version.
        // This is a Net-only test: NSURLSession (iOS) and OkHttp (Android) do not expose
        // per-request version negotiation — HttpRequestMessage.VersionPolicy is ignored on those platforms.
        [SkippableFact]
        public async Task Http3Test_ExplicitVersionExactOnRequest_IsRespected()
        {
            SkipIfNotNetPlatform();

            var request = new HttpRequestMessage(HttpMethod.Get, "https://httpbingo.org/get")
            {
                Version = HttpVersion.Version11,
                VersionPolicy = HttpVersionPolicy.RequestVersionExact
            };
            var response = await SendAsync(request);
            Assert.Equal(HttpVersion.Version11, response.Version);
        }
    }
}
