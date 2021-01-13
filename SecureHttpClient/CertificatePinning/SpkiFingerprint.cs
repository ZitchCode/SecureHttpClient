using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace SecureHttpClient.CertificatePinning
{
    internal class SpkiFingerprint
    {
        public static string Compute(X509Certificate2 certificate)
        {
            // Extract SPKI (der-encoded)
            var spki = SpkiProvider.GetSpki(certificate);

            // Compute spki fingerprint (sha256)
            using var digester = SHA256.Create();
            var digest = digester.ComputeHash(spki);
            var spkiFingerprint = Convert.ToBase64String(digest);

            return $"sha256/{spkiFingerprint}";
        }
    }
}
