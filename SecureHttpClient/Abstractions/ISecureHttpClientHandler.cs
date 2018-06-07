namespace SecureHttpClient.Abstractions
{
    /// <summary>
    /// Interface for SecureHttpClientHandler
    /// </summary>
    public interface ISecureHttpClientHandler
    {
        /// <summary>
        /// Add certificate pins for a given hostname
        /// </summary>
        /// <param name="hostname">The hostname</param>
        /// <param name="pins">The array of certifiate pins (example of pin string: "sha256/fiKY8VhjQRb2voRmVXsqI0xPIREcwOVhpexrplrlqQY=")</param>
        void AddCertificatePinner(string hostname, string[] pins);

        /// <summary>
        /// Set a client certificate
        /// </summary>
        /// <param name="certificate">The client certificate raw data</param>
        /// <param name="passphrase">The client certificate pass phrase</param>
        void SetClientCertificate(byte[] certificate, string passphrase);

        /// <summary>
        /// Set certificates for the trusted Root Certificate Authorities
        /// </summary>
        /// <param name="certificates">Certificates for the CAs to trust</param>
        void SetTrustedRoots(params byte[][] certificates);
    }
}
