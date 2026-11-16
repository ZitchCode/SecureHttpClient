using System;
using System.Net.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using SecureHttpClient.CertificatePinning;
using Xunit;

namespace SecureHttpClient.Test
{
    public class CustomRootTrustValidatorTest
    {
        // Regression test: when custom roots are configured, trust must be decided by the rebuilt
        // chain. If the rebuild fails, the certificate must be rejected even when the platform
        // reported no policy error, otherwise the custom roots are not actually enforced.
        [Fact]
        public void CustomRootTrustValidatorTest_ChainBuildFailure_IsRejected()
        {
            using var certificate = CreateSelfSignedCertificate(expired: true);
            using var trustedRoot = CreateSelfSignedCertificate(expired: false);

            var trusted = CustomRootTrustValidator.IsCertificateTrusted(
                certificate,
                SslPolicyErrors.None,
                new X509Certificate2Collection { trustedRoot });

            Assert.False(trusted, "A certificate whose chain cannot be built must not be trusted.");
        }

        // The chain must still be buildable against the configured roots (otherwise the validator
        // would reject everything).
        [Fact]
        public void CustomRootTrustValidatorTest_TrustedRootCertificate_IsAccepted()
        {
            using var certificate = CreateSelfSignedCertificate(expired: false);

            var trusted = CustomRootTrustValidator.IsCertificateTrusted(
                certificate,
                SslPolicyErrors.None,
                new X509Certificate2Collection { certificate });

            Assert.True(trusted);
        }

        // Errors other than chain errors remain fatal, even when the certificate chains to a
        // configured root.
        [Fact]
        public void CustomRootTrustValidatorTest_NameMismatch_IsRejected()
        {
            using var certificate = CreateSelfSignedCertificate(expired: false);

            var trusted = CustomRootTrustValidator.IsCertificateTrusted(
                certificate,
                SslPolicyErrors.RemoteCertificateNameMismatch,
                new X509Certificate2Collection { certificate });

            Assert.False(trusted);
        }

        private static X509Certificate2 CreateSelfSignedCertificate(bool expired)
        {
            using var key = RSA.Create(2048);
            var request = new CertificateRequest("CN=test.local", key, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            var notBefore = expired ? DateTimeOffset.UtcNow.AddYears(-2) : DateTimeOffset.UtcNow.AddDays(-1);
            var notAfter = expired ? DateTimeOffset.UtcNow.AddYears(-1) : DateTimeOffset.UtcNow.AddYears(1);
            return request.CreateSelfSigned(notBefore, notAfter);
        }
    }
}
