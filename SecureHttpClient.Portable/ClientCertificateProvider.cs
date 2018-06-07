using System.Security.Cryptography.X509Certificates;

namespace SecureHttpClient
{
	/// <summary>
	/// IClientCertificateProvider for Portable .Net
	/// </summary>
	public interface IClientCertificateProvider : Abstractions.IClientCertificateProvider
	{
		/// <summary>
		/// The current collection of client certificates.
		/// </summary>
		/// <value>The certificates.</value>
		X509CertificateCollection Certificates { get; }
	}

	/// <summary>
	/// Base Client certificate provider for Portable .Net
	/// </summary>
    public class ClientCertificateProvider : IClientCertificateProvider
    {
		public virtual X509CertificateCollection Certificates { get; protected set; } = new X509CertificateCollection();
	}

	/// <summary>
	/// Client certificate provider for imported certificates and keys.
	/// </summary>
	public class ImportedClientCertificateProvider : ClientCertificateProvider
	{
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
