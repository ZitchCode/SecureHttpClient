using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using SecureHttpClient.CertificatePinning;

namespace SecureHttpClient
{
    public class SecureHttpClientHandler : HttpClientHandler, Abstractions.ISecureHttpClientHandler
    {
        private readonly Lazy<CertificatePinner> _certificatePinner;
        private X509Certificate2Collection _trustedRoots;

        public SecureHttpClientHandler()
        {
            _certificatePinner = new Lazy<CertificatePinner>();

            // Set Accept-Encoding headers and take care of decompression if needed
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            MaxAutomaticRedirections = 10;
        }

        public void AddCertificatePinner(string hostname, string[] pins)
        {
            Debug.WriteLine($"Add CertificatePinner: hostname:{hostname}, pins:{string.Join("|", pins)}");
            _certificatePinner.Value.AddPins(hostname, pins);
            ServerCertificateCustomValidationCallback = CheckServerCertificate;
        }

        public void SetClientCertificates(Abstractions.IClientCertificateProvider iprovider)
        {
            ClientCertificates.Clear();
            var provider = iprovider as IClientCertificateProvider;
            if (provider != null)
            {
                ClientCertificates.AddRange(provider.Certificates);
            }
        }

        public void SetTrustedRoots(params byte[][] certificates)
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
                Debug.WriteLine("Missing certificate");
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
                Debug.WriteLine($"SSL policy errors {sslPolicyErrors}");
                return false;
            }

            if (_certificatePinner.IsValueCreated)
            {
                // Get request host
                var requestHost = httpRequestMessage?.RequestUri?.Host;
                if (string.IsNullOrEmpty(requestHost))
                {
                    Debug.WriteLine("Failed to get host from request");
                    return false;
                }

                // Check pin
                good = _certificatePinner.Value.Check(requestHost, certificate.RawData);
            }
            return good;
        }
    }
}
