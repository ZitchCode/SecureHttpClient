using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Maui.Devices;
using SecureHttpClient.Test.Helpers;
using Xunit;

namespace SecureHttpClient.Test
{
    public class SslTest : TestBase, IClassFixture<TestPinFixture>
    {
        private readonly TestPinFixture _fixture;

        public SslTest(TestPinFixture testFixture) : base(testFixture)
        {
            _fixture = testFixture;
        }

        [Fact]
        public async Task SslTest_ExpiredCertificate()
        {
            const string page = "https://expired.badssl.com/";
            await AssertExtensions.ThrowsTrustFailureAsync(() => GetAsync(page));
		}

		[Fact]
        public async Task SslTest_WrongHostCertificate()
        {
            const string page = "https://wrong.host.badssl.com/";
            await AssertExtensions.ThrowsTrustFailureAsync(() => GetAsync(page));
        }

        [Fact]
        public async Task SslTest_SelfSignedCertificate()
        {
            const string page = "https://self-signed.badssl.com/";
            await AssertExtensions.ThrowsTrustFailureAsync(() => GetAsync(page));
        }

        [Fact]
        public async Task SslTest_UntrustedRootCertificate()
        {
            const string page = "https://untrusted-root.badssl.com/";
            await AssertExtensions.ThrowsTrustFailureAsync(() => GetAsync(page));
        }

        [Fact]
        public async Task SslTest_SpecificTrustedRootCertificate()
        {
            const string page = "https://untrusted-root.badssl.com/";
            var caCert = await ResourceHelper.GetStringAsync("untrusted_root_badssl_com_certificate.pem");
            SetCaCertificate(caCert);
            await GetAsync(page);
        }

        [Fact]
        public async Task SslTest_SpecificTrustedRootCertificate_WithWrongPin()
        {
            const string page = "https://untrusted-root.badssl.com/";
            var caCert = await ResourceHelper.GetStringAsync("untrusted_root_badssl_com_certificate.pem");
            SetCaCertificate(caCert);
            AddCertificatePinner("untrusted-root.badssl.com", _fixture.PinsKo);
            await AssertExtensions.ThrowsTrustFailureAsync(() => GetAsync(page));
        }

        [Fact]
        public async Task SslTest_OnlyTrustSpecificRootCertificate()
        {
            const string page = "https://badssl.com"; // Has valid public cert, but not signed by our custom root
            var caCert = await ResourceHelper.GetStringAsync("untrusted_root_badssl_com_certificate.pem");
            SetCaCertificate(caCert);
            await AssertExtensions.ThrowsTrustFailureAsync(() => GetAsync(page));
        }

        [Fact(Skip = "Mobile apps usually skip revocation checks")]
        public async Task SslTest_RevokedCertificate()
        {
            const string page = "https://revoked.badssl.com/";
            await AssertExtensions.ThrowsTrustFailureAsync(() => GetAsync(page));
        }

        [Fact]
        public async Task SslTest_Sha256Certificate()
        {
            const string page = "https://sha256.badssl.com/";
            await GetAsync(page);
        }

        [Fact(Skip = "Certificate has expired, badssl.com needs to fix it.")]
        public async Task SslTest_Sha384Certificate()
        {
            const string page = "https://sha384.badssl.com/";
            await GetAsync(page);
        }

        [Fact(Skip = "Certificate has expired, badssl.com needs to fix it.")]
        public async Task SslTest_Sha512Certificate()
        {
            const string page = "https://sha512.badssl.com/";
            await GetAsync(page);
        }

        [Fact]
        public async Task SslTest_Ecc256Certificate()
        {
            const string page = "https://ecc256.badssl.com/";
            await GetAsync(page);
        }

        [Fact]
        public async Task SslTest_Ecc384Certificate()
        {
            const string page = "https://ecc384.badssl.com/";
            await GetAsync(page);
        }

        [Fact]
        public async Task SslTest_Rsa2048Certificate()
        {
            const string page = "https://rsa2048.badssl.com/";
            await GetAsync(page);
        }

        [Fact]
        public async Task SslTest_Rsa4096Certificate()
        {
            const string page = "https://rsa4096.badssl.com/";
            await GetAsync(page);
        }

        [Fact(Skip = "Certificate has expired, badssl.com need to fix it.")]
        public async Task SslTest_Rsa8192Certificate()
        {
            const string page = "https://rsa8192.badssl.com/";
            await GetAsync(page);
        }

        [Fact]
        public async Task SslTest_MissingClientCertificate()
        {
            const string page = "https://client-cert-missing.badssl.com/";
            await Assert.ThrowsAsync<HttpRequestException>(() => GetAsync(page));
        }

        [Fact]
        public async Task SslTest_ClientCertificate()
        {
            const string page = "https://client.badssl.com/";
            var clientCert = await ResourceHelper.GetBytesAsync("badssl.com-client.p12");
            const string certPass = "badssl.com";
            SetClientCertificate(clientCert, certPass);
            await GetAsync(page);
        }

        [Fact]
        public async Task SslTest_HowsMySsl()
        {
            var expectedTlsVersion = (DeviceInfo.Platform == DevicePlatform.Android && DeviceInfo.Version.Major < 10) ? "TLS 1.2" : "TLS 1.3";
            const string expectedRating = "Probably Okay";

            const string page = "https://www.howsmyssl.com/a/check";
            var result = await GetAsync(page).ReceiveString();

            var json = JsonDocument.Parse(result);
            var actualTlsVersion = json.RootElement.GetProperty("tls_version").GetString();
            var actualRating = json.RootElement.GetProperty("rating").GetString();

            Assert.Equal(expectedTlsVersion, actualTlsVersion);
            Assert.Equal(expectedRating, actualRating);
        }
    }
}
