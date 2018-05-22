namespace SecureHttpClient.Abstractions
{
    public interface ISecureHttpClientHandler
    {
        void AddCertificatePinner(string hostname, string[] pins);

        void SetClientCertificate(byte[] certificate, string passphrase);
    }
}
