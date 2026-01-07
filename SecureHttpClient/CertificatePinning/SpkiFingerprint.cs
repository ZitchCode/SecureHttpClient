using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace SecureHttpClient.CertificatePinning
{
    /// <summary>
    /// Computes SPKI (Subject Public Key Info) fingerprints for X.509 certificates.
    /// Used for certificate pinning validation.
    /// </summary>
    internal class SpkiFingerprint
    {
        /// <summary>
        /// Computes the SHA-256 SPKI fingerprint of a certificate.
        /// </summary>
        /// <param name="certificate">The X.509 certificate to compute the fingerprint for.</param>
        /// <returns>A string in the format "sha256/&lt;base64-encoded-fingerprint&gt;".</returns>
        public static string Compute(X509Certificate2 certificate)
        {
            // Extract SPKI (der-encoded)
            var spki = certificate.PublicKey.ExportSubjectPublicKeyInfo();

            // Compute spki fingerprint (sha256)
            using var digester = SHA256.Create();
            var digest = digester.ComputeHash(spki);
            var spkiFingerprint = Convert.ToBase64String(digest);

            return $"sha256/{spkiFingerprint}";
        }
    }
}
