using Foundation;
using Security;

namespace SecureHttpClient
{
    /// <summary>
    /// IClientCertificateProvider for iOS
    /// </summary>
    public interface IClientCertificateProvider : Abstractions.IClientCertificateProvider
    {
        /// <summary>
        /// The current client certificate
        /// </summary>
        /// <value>The credential.</value>
        NSUrlCredential Credential { get; }
    }

    /// <summary>
    /// Base Client certificate provider for iOS
    /// </summary>
    public class ClientCertificateProvider : IClientCertificateProvider
    {
		/// <summary>
		/// The current client certificate
		/// </summary>
		/// <value>The credential.</value>
		public NSUrlCredential Credential { get; protected set; }
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
            NSDictionary opt;
            if (string.IsNullOrEmpty(passphrase))
            {
                opt = new NSDictionary();
            }
            else
            {
                opt = NSDictionary.FromObjectAndKey(new NSString(passphrase), SecImportExport.Passphrase);
            }

            var status = SecImportExport.ImportPkcs12(certificate, opt, out NSDictionary[] array);

            if (status == SecStatusCode.Success)
            {
                var identity = new SecIdentity(array[0]["identity"].Handle);
                NSArray chain = array[0]["chain"] as NSArray;
                SecCertificate[] certs = new SecCertificate[chain.Count];
                for (System.nuint i = 0; i < chain.Count; i++)
                  certs[i] = chain.GetItem<SecCertificate>(i);
                Credential = new NSUrlCredential(identity, certs, NSUrlCredentialPersistence.ForSession);
            }
        }
    }
}
