using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.Maui.Devices;
using SecureHttpClient.CertificatePinning;
using SecureHttpClient.Test.Helpers;
using Xunit;

namespace SecureHttpClient.Test
{
    public class SpkiFingerprintTest : IClassFixture<TestFixture>
    {
        [SkippableTheory]
        [InlineData("rsa_certificate.pem", "sha256/l6meTmH/OlRPWR/Mn3ncvtBS25C+uYFhP26IOMzAa/E=")]
        [InlineData("dsa_certificate.pem", "sha256/Vq9zQp9NSsMxsGYi3Q1lO3wxv8AP2hSUvtaURKWKAt4=")]
        [InlineData("ecdsa_certificate.pem", "sha256/sfMf1hmimBN4QW8AEMs2hXMX2aZEBTD4E9PTK0FntC0=")]
        public async Task SpkiFingerprintTest_RsaCertificate(string resource, string expected)
        {
            Skip.If(DeviceInfo.Platform == DevicePlatform.Android, "Not implemented on Android");

            var certPem = await ResourceHelper.GetStringAsync(resource);
            var certBytes = System.Text.Encoding.ASCII.GetBytes(certPem);
            var certificate = new X509Certificate2(certBytes);
            var actual = SpkiFingerprint.Compute(certificate);
            Assert.Equal(expected, actual);
        }
    }
}
