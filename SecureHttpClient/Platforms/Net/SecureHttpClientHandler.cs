#if NETSTANDARD2_0

using System;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Security.Authentication;
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
        private X509Certificate2Collection _trustedRoots;

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
        public virtual void AddCertificatePinner(string hostname, string[] pins)
        {
            _certificatePinner.Value.AddPins(hostname, pins);
            ServerCertificateCustomValidationCallback = CheckServerCertificate;
        }

        /// <summary>
        /// Set the client certificate provider (NetStandard implementation)
        /// </summary>
        /// <param name="provider">The provider for client certificates on this platform</param>
        public virtual void SetClientCertificates(Abstractions.IClientCertificateProvider provider)
        {
            ClientCertificates.Clear();
            if (provider is IClientCertificateProvider netProvider)
            {
                ClientCertificates.AddRange(netProvider.Certificates);
            }
        }

        /// <summary>
        /// Set certificates for the trusted Root Certificate Authorities (NetStandard implementation)
        /// </summary>
        /// <param name="certificates">Certificates for the CAs to trust</param>
        public virtual void SetTrustedRoots(params byte[][] certificates)
        {
            if (certificates.Length == 0)
            {
                _trustedRoots = null;
                return;
            }
            _trustedRoots = new X509Certificate2Collection();
            foreach (var cert in certificates)
            {
                _trustedRoots.Import(cert);
            }
            ServerCertificateCustomValidationCallback = CheckServerCertificate;
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
                if (ex.InnerException is AuthenticationException)
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

            bool good = sslPolicyErrors == SslPolicyErrors.None;
            if (_trustedRoots != null && (sslPolicyErrors & ~SslPolicyErrors.RemoteCertificateChainErrors) == 0)
            {
                chain.ChainPolicy.ExtraStore.AddRange(_trustedRoots);
                chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllowUnknownCertificateAuthority;
                if (chain.Build(certificate))
                {
                    var root = chain.ChainElements[chain.ChainElements.Count - 1].Certificate;
                    good = _trustedRoots.Find(X509FindType.FindByThumbprint, root.Thumbprint, false).Count > 0;
                }
            }

            if (!good)
            {
                _logger?.LogDebug($"SSL policy errors {sslPolicyErrors}");
                return false;
            }

            if (_certificatePinner.IsValueCreated)
            {
                // Get request host
                var requestHost = httpRequestMessage?.RequestUri?.Host;
                if (string.IsNullOrEmpty(requestHost))
                {
                    _logger?.LogDebug("Failed to get host from request");
                    return false;
                }

                // Check pin
                good = _certificatePinner.Value.Check(requestHost, certificate.RawData);
            }
            return good;
        }
    }
}

#endif
