using System.Security.Cryptography.X509Certificates;
using SecureHttpClient.Abstractions;

namespace SecureHttpClient
{
    public class ClientCertificateProvider : IClientCertificateProvider
    {

        /// <summary>
        /// The current collection of client certificates.
        /// </summary>
        /// <value>The certificates.</value>
        public X509CertificateCollection Certificates { get; } = new X509CertificateCollection();

        /// <summary>
        /// Import the specified certificate and its associated private key.
        /// </summary>
        /// <param name="certificate">The certificate and key, in PKCS12 format.</param>
        /// <param name="passphrase">The passphrase that protects the private key.</param>
        public void Import(byte[] certificate, string passphrase)
        {
            Certificates.Add(new X509Certificate2(certificate, passphrase));
        }
    }
}
