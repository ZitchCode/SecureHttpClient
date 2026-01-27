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
        public async Task CertificateHelperTest_GetCertificate()
        {
            const string hostname = "www.howsmyssl.com";
            var certificate = await CertificateHelper.GetCertificateAsync(hostname);
            Assert.NotNull(certificate);

            const string expectedIssuer = "CN=R12, O=Let's Encrypt, C=US";
            Assert.Equal(expectedIssuer, certificate.Issuer);

            const string expectedSubject = "CN=www.howsmyssl.com";
            Assert.Equal(expectedSubject, certificate.Subject);

            const string expectedSignatureAlgorithm = "sha256RSA";
            Assert.Equal(expectedSignatureAlgorithm, certificate.SignatureAlgorithm.FriendlyName);
        }
    }
}
