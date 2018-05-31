namespace SecureHttpClient.Abstractions
{
    internal interface ISecureHttpClientHandler
    {
        void AddCertificatePinner(string hostname, string[] pins);

        void SetClientCertificate(byte[] certificate, string passphrase);

        void SetTrustedRoots(params byte[][] certificates);
    }
}
