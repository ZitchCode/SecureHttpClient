using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using SecureHttpClient.CertificatePinning;
using SecureHttpClient.Test.Helpers;
using Xunit;

namespace SecureHttpClient.Test
{
    public class CertificateHelperTest : IClassFixture<TestFixture>
    {
        [Theory]
        [InlineData("rsa_certificate.pem", "sha256/l6meTmH/OlRPWR/Mn3ncvtBS25C+uYFhP26IOMzAa/E=")]
        [InlineData("dsa_certificate.pem", "sha256/Vq9zQp9NSsMxsGYi3Q1lO3wxv8AP2hSUvtaURKWKAt4=")]
        [InlineData("ecdsa_certificate.pem", "sha256/sfMf1hmimBN4QW8AEMs2hXMX2aZEBTD4E9PTK0FntC0=")]
        public async Task CertificateHelperTest_GetSpkiFingerprint(string resource, string expected)
        {
            var certPem = await ResourceHelper.GetStringAsync(resource);
            var certBytes = System.Text.Encoding.ASCII.GetBytes(certPem);
            var certificate = X509CertificateLoader.LoadCertificate(certBytes);
            var actual = CertificateHelper.GetSpkiFingerprint(certificate);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public async Task CertificateHelperTest_GetCertificate_howsmyssl()
        {
            const string hostname = "www.howsmyssl.com";
            var certificate = await CertificateHelper.GetCertificateAsync(hostname);
            Assert.NotNull(certificate);
            CheckCertificate(certificate, hostname, "CN=R12, O=Let's Encrypt, C=US", "sha256RSA");
        }

        [Fact]
        public async Task CertificateHelperTest_GetCertificate_zitch()
        {
            const string hostname = "www.zitch.com";
            var certificate = await CertificateHelper.GetCertificateAsync(hostname);
            Assert.NotNull(certificate);
            CheckCertificate(certificate, hostname, "CN=WE1, O=Google Trust Services, C=US", "sha256ECDSA");
        }

        [Fact]
        public async Task CertificateHelperTest_GetCertificates_howsmyssl()
        {
            const string hostname = "www.howsmyssl.com";
            var certificates = await CertificateHelper.GetCertificatesAsync(hostname);
            Assert.Single(certificates);
            var certificate = certificates.First();
            CheckCertificate(certificate, hostname, "CN=R12, O=Let's Encrypt, C=US", "sha256RSA");
        }

        [Fact]
        public async Task CertificateHelperTest_GetCertificates_zitch()
        {
            const string hostname = "www.zitch.com";
            var certificates = await CertificateHelper.GetCertificatesAsync(hostname);
            Assert.Single(certificates);
            var certificate = certificates.First();
            CheckCertificate(certificate, hostname, "CN=WE1, O=Google Trust Services, C=US", "sha256ECDSA");
        }

        private static void CheckCertificate(X509Certificate2 certificate, string hostname, string expectedIssuer, string expectedSignatureAlgorithm)
        {
            Assert.Equal(expectedIssuer, certificate.Issuer);
            Assert.Equal($"CN={hostname}", certificate.Subject);
            Assert.Equal(expectedSignatureAlgorithm, certificate.SignatureAlgorithm.FriendlyName);
        }
    }
}
