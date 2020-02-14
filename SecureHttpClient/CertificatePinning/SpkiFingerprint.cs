﻿using System;
using System.Security.Cryptography;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.X509;

namespace SecureHttpClient.CertificatePinning
{
    internal class SpkiFingerprint
    {
        public static string Compute(byte[] certificate)
        {
            // Load ASN.1 encoded certificate structure
            var certAsn1 = Asn1Object.FromByteArray(certificate);
            var certStruct = X509CertificateStructure.GetInstance(certAsn1);

            // Extract SPKI and DER-encode it
            var spki = certStruct.SubjectPublicKeyInfo;
            var spkiDer = spki.GetDerEncoded();

            // Compute spki fingerprint (sha256)
            using var digester = SHA256.Create();
            var digest = digester.ComputeHash(spkiDer);
            var spkiFingerprint = Convert.ToBase64String(digest);

            return $"sha256/{spkiFingerprint}";
        }
    }
}
