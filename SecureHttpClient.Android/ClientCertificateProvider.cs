using System.IO;
using Java.Security;
using SecureHttpClient.Abstractions;

namespace SecureHttpClient
{
    public class ClientCertificateProvider : IClientCertificateProvider
    {
        /// <summary>
        /// The current collection of client certificates
        /// </summary>
        /// <value>The key store.</value>
        public KeyStore KeyStore { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:SecureHttpClient.ClientCertificateProvider"/> class
        /// with the default backing store.
        /// </summary>
        public ClientCertificateProvider() : this("pkcs12")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:SecureHttpClient.ClientCertificateProvider"/> class.
        /// </summary>
        /// <param name="type">The type of backing store for this certificate provider.</param>
        public ClientCertificateProvider(string type)
        {
            KeyStore = KeyStore.GetInstance(type);
            KeyStore.Load(null);
        }

        /// <summary>
        /// Import the specified certificate and its associated private key.
        /// </summary>
        /// <param name="certificate">The certificate and key, in PKCS12 format.</param>
        /// <param name="passphrase">The passphrase that protects the private key.</param>
        public void Import(byte[] certificate, string passphrase)
        {
            KeyStore.Load(new MemoryStream(certificate), passphrase.ToCharArray());
        }
    }
}
