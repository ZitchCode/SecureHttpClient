using Foundation;
using SecureHttpClient.Abstractions;
using Security;

namespace SecureHttpClient
{
    public class ClientCertificateProvider : IClientCertificateProvider
    {

        /// <summary>
        /// The current client certificate
        /// </summary>
        /// <value>The credential.</value>
        public NSUrlCredential Credential { get; protected set; }

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

            NSDictionary[] array;
            var status = SecImportExport.ImportPkcs12(certificate, opt, out array);

            if (status == SecStatusCode.Success)
            {
                var identity = new SecIdentity(array[0]["identity"].Handle);
                SecCertificate[] certs = { identity.Certificate };
                Credential = new NSUrlCredential(identity, certs, NSUrlCredentialPersistence.ForSession);
            }
        }
    }
}
