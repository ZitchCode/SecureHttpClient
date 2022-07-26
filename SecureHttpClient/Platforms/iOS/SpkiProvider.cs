﻿using System.Security.Cryptography.X509Certificates;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.X509;

namespace SecureHttpClient
{
    internal class SpkiProvider
    {
        public static byte[] GetSpki(X509Certificate2 certificate)
        {
            // Load ASN.1 encoded certificate structure
            var certAsn1 = Asn1Object.FromByteArray(certificate.RawData);
            var certStruct = X509CertificateStructure.GetInstance(certAsn1);

            // Extract SPKI and DER-encode it
            var spki = certStruct.SubjectPublicKeyInfo;
            var spkiDer = spki.GetDerEncoded();

            return spkiDer;
        }
    }
}
