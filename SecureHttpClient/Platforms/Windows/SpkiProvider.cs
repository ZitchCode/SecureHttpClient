using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace SecureHttpClient
{
    internal class SpkiProvider
    {
        public static byte[] GetSpki(X509Certificate2 certificate)
        {
            // Extract SPKI (der-encoded)
            using var key = certificate.GetECDsaPublicKey() ?? certificate.GetRSAPublicKey() ?? (AsymmetricAlgorithm)certificate.GetDSAPublicKey();
            var spki = key.ExportSubjectPublicKeyInfo();
            return spki;
        }
    }
}
