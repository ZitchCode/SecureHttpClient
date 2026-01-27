using System;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace SecureHttpClient.CertificatePinning
{
    /// <summary>
    /// Provides utility methods for working with X.509 certificates, including retrieving certificates from remote
    /// hosts and computing SPKI fingerprints.
    /// </summary>
    public static class CertificateHelper
    {
        /// <summary>
        /// Computes the SHA-256 fingerprint of the Subject Public Key Info (SPKI) for the specified X.509 certificate,
        /// formatted as a base64-encoded string with a "sha256/" prefix.
        /// </summary>
        /// <param name="certificate">The X.509 certificate from which to extract and compute the SPKI fingerprint. Cannot be null.</param>
        /// <returns>A string containing the SPKI fingerprint in the format "sha256/{base64}".</returns>
        public static string GetSpkiFingerprint(X509Certificate2 certificate)
        {
            // Extract SPKI (der-encoded)
            var spki = certificate.PublicKey.ExportSubjectPublicKeyInfo();

            // Compute spki fingerprint (sha256)
            using var digester = SHA256.Create();
            var digest = digester.ComputeHash(spki);
            var spkiFingerprint = Convert.ToBase64String(digest);

            return $"sha256/{spkiFingerprint}";
        }

        /// <summary>
        /// Retrieves the X.509 certificate presented by a remote server during an SSL/TLS handshake on port 443.
        /// </summary>
        /// <param name="hostname">The DNS name of the remote server from which to retrieve the certificate.</param>
        /// <returns>An X509Certificate2 object representing the server's SSL/TLS certificate.</returns>
        public static async Task<X509Certificate2> GetCertificateAsync(string hostname)
        {
            using var tcpClient = new TcpClient();
            await tcpClient.ConnectAsync(hostname, 443);

            await using var sslStream = new SslStream(
                tcpClient.GetStream(),
                leaveInnerStreamOpen: false,
                userCertificateValidationCallback: (_, _, _, _) => true
            );

            var options = new SslClientAuthenticationOptions
            {
                TargetHost = hostname,
                EnabledSslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13,
                CertificateRevocationCheckMode = X509RevocationMode.NoCheck
            };

            await sslStream.AuthenticateAsClientAsync(options);

            var cert = new X509Certificate2(sslStream.RemoteCertificate!);
            return cert;
        }
    }
}
