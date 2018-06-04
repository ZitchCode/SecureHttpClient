namespace SecureHttpClient.Abstractions
{
    internal interface ISecureHttpClientHandler
    {
        void AddCertificatePinner(string hostname, string[] pins);

        void SetClientCertificates(IClientCertificateProvider provider);

        void SetTrustedRoots(params byte[][] certificates);
    }
}
