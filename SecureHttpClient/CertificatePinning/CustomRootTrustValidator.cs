using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace SecureHttpClient.CertificatePinning
{
    /// <summary>
    /// Validates a server certificate against a custom set of trusted roots. Used by the .NET
    /// handler, where the platform validation callback only reports the system trust result.
    /// </summary>
    internal static class CustomRootTrustValidator
    {
        /// <summary>
        /// Returns whether the certificate is trusted. When <paramref name="trustedRoots"/> is
        /// configured, the chain is rebuilt with those roots as the only trust anchors, so a
        /// certificate the system trusts but that does not chain to them is rejected.
        /// </summary>
        internal static bool IsCertificateTrusted(
            X509Certificate2 certificate,
            SslPolicyErrors sslPolicyErrors,
            X509Certificate2Collection? trustedRoots)
        {
            // Errors unrelated to the chain (name mismatch, unavailable certificate, ...) are fatal.
            if ((sslPolicyErrors & ~SslPolicyErrors.RemoteCertificateChainErrors) != 0)
            {
                return false;
            }

            if (trustedRoots is not { Count: > 0 })
            {
                return sslPolicyErrors == SslPolicyErrors.None;
            }

            using var chain = new X509Chain();
            chain.ChainPolicy.TrustMode = X509ChainTrustMode.CustomRootTrust;
            chain.ChainPolicy.CustomTrustStore.AddRange(trustedRoots);
            chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
            chain.ChainPolicy.DisableCertificateDownloads = true;

            // A failed build (expired certificate, partial chain, ...) must reject, even when the
            // platform reported no chain error.
            return chain.Build(certificate);
        }
    }
}
