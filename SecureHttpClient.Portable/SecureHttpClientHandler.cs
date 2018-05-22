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
    public class SecureHttpClientHandler : HttpClientHandler, Abstractions.ISecureHttpClientHandler
    {
        private readonly Lazy<CertificatePinner> _certificatePinner;
        private readonly ILogger _logger;

        public SecureHttpClientHandler(ILogger logger = null)
        {
            _logger = logger;

            _certificatePinner = new Lazy<CertificatePinner>(() => new CertificatePinner(logger));

            // Set Accept-Encoding headers and take care of decompression if needed
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            MaxAutomaticRedirections = 10;
        }

        public void AddCertificatePinner(string hostname, string[] pins)
        {
            _certificatePinner.Value.AddPins(hostname, pins);
            ServerCertificateCustomValidationCallback = CheckServerCertificate;
        }

        public void SetClientCertificate(byte[] certificate, string passphrase)
        {
            ClientCertificates.Clear();
            ClientCertificates.Add(new X509Certificate2(certificate, passphrase));
        }

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
