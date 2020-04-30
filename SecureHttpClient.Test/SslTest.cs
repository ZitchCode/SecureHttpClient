﻿using System.Net.Http;
using System.Threading.Tasks;
using Flurl.Http;
using Newtonsoft.Json.Linq;
using SecureHttpClient.Test.Helpers;
using Xamarin.Essentials;
using Xunit;

namespace SecureHttpClient.Test
{
    public class SslTest : TestBase, IClassFixture<TestFixture>
    {
        public SslTest(TestFixture testFixture) : base(testFixture)
        {
        }

        [Fact]
        public async Task SslTest_ExpiredCertificate()
        {
            const string page = @"https://expired.badssl.com/";
            await AssertExtensions.ThrowsTrustFailureAsync(() => GetAsync(page));
		}

		[Fact]
        public async Task SslTest_WrongHostCertificate()
        {
            const string page = @"https://wrong.host.badssl.com/";
            await AssertExtensions.ThrowsTrustFailureAsync(() => GetAsync(page));
        }

        [Fact]
        public async Task SslTest_SelfSignedCertificate()
        {
            const string page = @"https://self-signed.badssl.com/";
            await AssertExtensions.ThrowsTrustFailureAsync(() => GetAsync(page));
        }

        [Fact]
        public async Task SslTest_UntrustedRootCertificate()
        {
            const string page = @"https://untrusted-root.badssl.com/";
            await AssertExtensions.ThrowsTrustFailureAsync(() => GetAsync(page));
        }

        [SkippableFact]
        public async Task SslTest_SpecificTrustedRootCertificate()
        {
            Skip.If(DeviceInfo.Platform == DevicePlatform.iOS || DeviceInfo.Platform == DevicePlatform.UWP, "Not working on iOS 13 and on UWP");

            // NB: Using this feature on iOS 11 requires setting NSExceptionDomains in Info.plist,
            // particularly NSExceptionRequiresForwardSecrecy=NO : https://stackoverflow.com/q/46316604/5652125
            const string page = @"https://untrusted-root.badssl.com/";
            var caCert = await ResourceHelper.GetStringAsync("untrusted_root_badssl_com_certificate.pem");
            SetCaCertificate(caCert);
            await GetAsync(page);
        }

        [Fact]
        public async Task SslTest_OnlyTrustSpecificRootCertificate()
        {
            const string page = @"https://badssl.com"; // Has valid public cert, but not signed by our custom root
            var caCert = await ResourceHelper.GetStringAsync("untrusted_root_badssl_com_certificate.pem");
            SetCaCertificate(caCert);
            await AssertExtensions.ThrowsTrustFailureAsync(() => GetAsync(page));
        }

        [SkippableFact]
        public async Task SslTest_RevokedCertificate()
        {
            Skip.IfNot(DeviceInfo.Platform == DevicePlatform.iOS, "Unsupported on Android, not implemented on UWP");

            const string page = @"https://revoked.badssl.com/";
            await AssertExtensions.ThrowsTrustFailureAsync(() => GetAsync(page));
        }

        [Fact]
        public async Task SslTest_Sha256Certificate()
        {
            const string page = @"https://sha256.badssl.com/";
            await GetAsync(page);
        }

        [Fact]
        public async Task SslTest_Sha384Certificate()
        {
            const string page = @"https://sha384.badssl.com/";
            await GetAsync(page);
        }

        [Fact]
        public async Task SslTest_Sha512Certificate()
        {
            const string page = @"https://sha512.badssl.com/";
            await GetAsync(page);
        }

        [Fact]
        public async Task SslTest_Ecc256Certificate()
        {
            const string page = @"https://ecc256.badssl.com/";
            await GetAsync(page);
        }

        [Fact]
        public async Task SslTest_Ecc384Certificate()
        {
            const string page = @"https://ecc384.badssl.com/";
            await GetAsync(page);
        }

        [Fact]
        public async Task SslTest_Rsa2048Certificate()
        {
            const string page = @"https://rsa2048.badssl.com/";
            await GetAsync(page);
        }

        [Fact]
        public async Task SslTest_Rsa4096Certificate()
        {
            const string page = @"https://rsa4096.badssl.com/";
            await GetAsync(page);
        }

        [Fact]
        public async Task SslTest_Rsa8192Certificate()
        {
            const string page = @"https://rsa8192.badssl.com/";
            await GetAsync(page);
        }

        [Fact]
        public async Task SslTest_MissingClientCertificate()
        {
            const string page = @"https://client-cert-missing.badssl.com/";
            await Assert.ThrowsAsync<HttpRequestException>(() => GetAsync(page));
        }

        [SkippableFact]
        public async Task SslTest_ClientCertificate()
        {
            Skip.If(DeviceInfo.Platform == DevicePlatform.UWP, "Not supported on UWP");

            const string page = @"https://client.badssl.com/";
            var clientCert = await ResourceHelper.GetBytesAsync("badssl.com-client.p12");
            const string certPass = "badssl.com";
            SetClientCertificate(clientCert, certPass);
            await GetAsync(page);
        }

        [Fact]
        public async Task SslTest_SubjectAltName()
        {
            const string page = @"https://www.prive.livretzesto.fr/";
            await GetAsync(page);
        }

        [Fact]
        public async Task SslTest_HowsMySsl()
        {
            var expectedTlsVersion = (DeviceInfo.Platform == DevicePlatform.iOS || (DeviceInfo.Platform == DevicePlatform.Android && DeviceInfo.Version.Major >= 10)) ? "TLS 1.3" : "TLS 1.2";
            const string expectedRating = "Probably Okay";

            const string page = @"https://www.howsmyssl.com/a/check";
            var result = await GetAsync(page).ReceiveString();

            var json = JToken.Parse(result);
            var actualTlsVersion = json["tls_version"].ToString();
            var actualRating = json["rating"].ToString();

            Assert.Equal(expectedTlsVersion, actualTlsVersion);
            Assert.Equal(expectedRating, actualRating);
        }
    }
}
