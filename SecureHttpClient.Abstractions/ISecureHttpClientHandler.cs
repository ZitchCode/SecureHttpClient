namespace SecureHttpClient.Abstractions
{
    internal interface ISecureHttpClientHandler
    {
        void AddCertificatePinner(string hostname, string[] pins);
    }
}
