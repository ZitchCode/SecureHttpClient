using System.IO;
using Java.Security;

namespace SecureHttpClient
{
	/// <summary>
	/// IClientCertificateProvider for Android
	/// </summary>
	public interface IClientCertificateProvider : Abstractions.IClientCertificateProvider
	{
		/// <summary>
		/// The current collection of client certificates
		/// </summary>
		/// <value>The key store.</value>
		KeyStore KeyStore { get; }
	}

	/// <summary>
	/// Base Client certificate provider for Android
	/// </summary>
    public class ClientCertificateProvider : IClientCertificateProvider
    {
        /// <summary>
        /// The current collection of client certificates
        /// </summary>
        /// <value>The key store.</value>
        public virtual KeyStore KeyStore { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:SecureHttpClient.ClientCertificateProvider"/> class.
        /// </summary>
        /// <param name="type">The type of backing store for this certificate provider.</param>
        public ClientCertificateProvider(string type)
        {
            KeyStore = KeyStore.GetInstance(type);
            KeyStore.Load(null);
        }
	}

	/// <summary>
	/// Client certificate provider for imported certificates and keys.
	/// </summary>
	public class ImportedClientCertificateProvider : ClientCertificateProvider
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="T:SecureHttpClient.ClientCertificateProvider"/> class
		/// with the default backing store.
		/// </summary>
		public ImportedClientCertificateProvider() : base("pkcs12")
		{
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
