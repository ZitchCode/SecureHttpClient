using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
        /// Retrieves the Subject Public Key Info (SPKI) fingerprint of the TLS certificate presented by the specified host, using default address family.
        /// </summary>
        /// <param name="hostname">The DNS name of the remote host from which to retrieve the TLS certificate. Cannot be null or empty.</param>
        /// <returns>A task which result contains the SPKI fingerprint in the format "sha256/{base64}".</returns>
        public static async Task<string> GetSpkiFingerprintAsync(string hostname)
        {
            var certificate = await GetCertificateAsync(hostname).ConfigureAwait(false);
            var spkiFingerprint = GetSpkiFingerprint(certificate);
            return spkiFingerprint;
        }

        /// <summary>
        /// Retrieves the Subject Public Key Info (SPKI) fingerprints of the TLS certificates presented by the specified host.
        /// </summary>
        /// <param name="hostname">The DNS name of the remote host from which to retrieve the TLS certificates. Cannot be null or empty.</param>
        /// <returns>A task which result contains the SPKI fingerprints in the format "sha256/{base64}".</returns>
        public static async Task<IReadOnlyCollection<string>> GetSpkiFingerprintsAsync(string hostname)
        {
            var certificates = await GetCertificatesAsync(hostname).ConfigureAwait(false);
            var spkiFingerprints = certificates.Select(GetSpkiFingerprint).ToList();
            return spkiFingerprints;
        }

        internal static async Task<X509Certificate2> GetCertificateAsync(string hostname)
        {
            using var tcpClient = new TcpClient();
            await tcpClient.ConnectAsync(hostname, 443).ConfigureAwait(false);
            var cert = await GetCertificateAsync(hostname, tcpClient).ConfigureAwait(false);
            return cert;
        }

        internal static async Task<IReadOnlyCollection<X509Certificate2>> GetCertificatesAsync(string hostname)
        {
            var addresses = await Dns.GetHostAddressesAsync(hostname).ConfigureAwait(false); // DNS resolution (IPv4 + IPv6)
            var tasks = addresses.Select(async address =>
            {
                using var tcpClient = new TcpClient(address.AddressFamily);
                await tcpClient.ConnectAsync(address, 443).ConfigureAwait(false);
                return await GetCertificateAsync(hostname, tcpClient).ConfigureAwait(false);
            });
            var certs = await Task.WhenAll(tasks).ConfigureAwait(false);
            var seenThumbprints = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var results = new List<X509Certificate2>();
            foreach (var cert in certs)
            {
                if (seenThumbprints.Add(cert.Thumbprint))
                {
                    results.Add(cert);
                }
            }
            return results;
        }

        private static async Task<X509Certificate2> GetCertificateAsync(string hostname, TcpClient tcpClient)
        {
            using var sslStream = new SslStream(
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

            await sslStream.AuthenticateAsClientAsync(options).ConfigureAwait(false);

            var cert = new X509Certificate2(sslStream.RemoteCertificate!);
            return cert;
        }
    }
}
