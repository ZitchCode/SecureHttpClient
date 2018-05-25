#if NETSTANDARD2_0

using System;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SecureHttpClient.CertificatePinning;

namespace SecureHttpClient
{
    /// <summary>
    /// Implementation of ISecureHttpClientHandler (NetStandard implementation)
    /// </summary>
    public class SecureHttpClientHandler : HttpClientHandler, Abstractions.ISecureHttpClientHandler
    {
        private readonly Lazy<CertificatePinner> _certificatePinner;
        private readonly ILogger _logger;

        /// <summary>
        /// SecureHttpClientHandler constructor (NetStandard implementation)
        /// </summary>
        /// <param name="logger">Optional logger</param>
        public SecureHttpClientHandler(ILogger logger = null)
        {
            _logger = logger;

            _certificatePinner = new Lazy<CertificatePinner>(() => new CertificatePinner(logger));

            // Set Accept-Encoding headers and take care of decompression if needed
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            MaxAutomaticRedirections = 10;
        }

        /// <summary>
        /// Add certificate pins for a given hostname (NetStandard implementation)
        /// </summary>
        /// <param name="hostname">The hostname</param>
        /// <param name="pins">The array of certifiate pins (example of pin string: "sha256/fiKY8VhjQRb2voRmVXsqI0xPIREcwOVhpexrplrlqQY=")</param>
        public void AddCertificatePinner(string hostname, string[] pins)
        {
            _certificatePinner.Value.AddPins(hostname, pins);
            ServerCertificateCustomValidationCallback = CheckServerCertificate;
        }

        /// <summary>
        /// Set a client certificate (NetStandard implementation)
        /// </summary>
        /// <param name="certificate">The client certificate raw data</param>
        /// <param name="passphrase">The client certificate pass phrase</param>
        public void SetClientCertificate(byte[] certificate, string passphrase)
        {
            ClientCertificates.Clear();
            ClientCertificates.Add(new X509Certificate2(certificate, passphrase));
        }

        /// <inheritdoc />
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            HttpResponseMessage response;
            try
            {
                response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
            }
            catch (HttpRequestException ex)
            {
                if (ex.InnerException != null && ex.InnerException.HResult == -2147012721) // ERROR_WINHTTP_SECURE_FAILURE 0x80072F8F
                {
                    throw new WebException(ex.InnerException.Message, WebExceptionStatus.TrustFailure);
                }
                throw;
            }
            return response;
        }

        private bool CheckServerCertificate(HttpRequestMessage httpRequestMessage, X509Certificate2 certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            if (certificate == null)
            {
                _logger?.LogDebug("Missing certificate");
                return false;
            }

            if (sslPolicyErrors != SslPolicyErrors.None)
            {
                _logger?.LogDebug($"SSL policy errors {sslPolicyErrors}");
                return false;
            }

            // Get request host
            var requestHost = httpRequestMessage?.RequestUri?.Host;
            if (string.IsNullOrEmpty(requestHost))
            {
                _logger?.LogDebug("Failed to get host from request");
                return false;
            }

            // Check pin
            var result = _certificatePinner.Value.Check(requestHost, certificate.RawData);
            return result;
        }
    }
}

#endif
