#if __IOS__

using System;
using System.IO;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using Foundation;
using Microsoft.Extensions.Logging;
using SecureHttpClient.CertificatePinning;
using SecureHttpClient.Extensions;
using System.Security.Cryptography.X509Certificates;

namespace SecureHttpClient
{
    /// <summary>
    /// Implementation of ISecureHttpClientHandler (iOS implementation)
    /// </summary>
    public class SecureHttpClientHandler : HttpClientHandler, Abstractions.ISecureHttpClientHandler
    {
        internal readonly Dictionary<NSUrlSessionTask, InflightOperation> InflightRequests;
        internal NSUrlCredential? ClientCertificate { get; private set; }
        private X509Certificate2Collection? _trustedRoots;
        private readonly Lazy<CertificatePinner> _certificatePinner;
        private readonly ILogger<Abstractions.ISecureHttpClientHandler> _logger;
        private readonly Lazy<NSUrlSession> _session;

        /// <summary>
        /// SecureHttpClientHandler constructor (iOS implementation)
        /// </summary>
        /// <param name="logger">Logger</param>
        public SecureHttpClientHandler(ILogger<Abstractions.ISecureHttpClientHandler> logger)
        {
            _logger = logger;
            InflightRequests = new Dictionary<NSUrlSessionTask, InflightOperation>();
            _certificatePinner = new Lazy<CertificatePinner>(() => new CertificatePinner(logger));
            _session = new Lazy<NSUrlSession>(InitSession);
        }

        /// <summary>
        /// Add certificate pins for a given hostname (iOS implementation)
        /// </summary>
        /// <param name="hostname">The hostname</param>
        /// <param name="pins">The array of certifiate pins (example of pin string: "sha256/fiKY8VhjQRb2voRmVXsqI0xPIREcwOVhpexrplrlqQY=")</param>
        public virtual void AddCertificatePinner(string hostname, string[] pins)
        {
            EnsureNotStarted();
            _certificatePinner.Value.AddPins(hostname, pins);
        }

        /// <summary>
        /// Set the client certificate provider (iOS implementation)
        /// </summary>
        /// <param name="provider">The provider for client certificates on this platform</param>
        public virtual void SetClientCertificates(Abstractions.IClientCertificateProvider provider)
        {
            EnsureNotStarted();
            ClientCertificate = (provider as IClientCertificateProvider)?.Credential;
        }

        /// <summary>
        /// Set certificates for the trusted Root Certificate Authorities (iOS implementation)
        /// </summary>
        /// <param name="certificates">Certificates for the CAs to trust</param>
        public virtual void SetTrustedRoots(params byte[][] certificates)
        {
            EnsureNotStarted();
            if (certificates.Length == 0)
            {
                _trustedRoots = null;
                return;
            }
            _trustedRoots = new X509Certificate2Collection();
            foreach (var cert in certificates)
            {
                _trustedRoots.Add(X509CertificateLoader.LoadCertificate(cert));
            }
        }

        // Certificate pinning, trusted roots and client certificates are captured when the
        // NSUrlSession is created, so they cannot be changed once the handler has been used.
        private void EnsureNotStarted()
        {
            if (_session.IsValueCreated)
            {
                throw new InvalidOperationException("Certificate pinning, trusted roots and client certificates must be configured before the first request.");
            }
        }

        private NSUrlSession InitSession()
        {
            using var configuration = NSUrlSessionConfiguration.EphemeralSessionConfiguration;
            if (!UseProxy)
            {
                configuration.ConnectionProxyDictionary = new NSDictionary();
            }
#pragma warning disable CA1416
            else if (Proxy is WebProxy webProxy)
#pragma warning restore CA1416
            {
                configuration.ConnectionProxyDictionary = GetProxyDictionary(webProxy);
            }
            if (!UseCookies)
            {
                configuration.HttpCookieStorage = null;
            }
            var nsUrlSessionDelegate = (INSUrlSessionDelegate)new DataTaskDelegate(this, _certificatePinner.IsValueCreated ? _certificatePinner.Value : null, _trustedRoots);
            return NSUrlSession.FromConfiguration(configuration, nsUrlSessionDelegate, null);
        }

        private static NSDictionary GetProxyDictionary(WebProxy webProxy)
        {
            var host = webProxy.Address!.Host;
            var port = webProxy.Address!.Port;
            var values = new []
            {
                NSObject.FromObject(host),
                NSNumber.FromInt32(port),
                NSNumber.FromInt32(1),
                NSObject.FromObject(host),
                NSNumber.FromInt32(port),
                NSNumber.FromInt32(1)
            };
            var keys = new NSObject[]
            {
                NSObject.FromObject("HTTPProxy")!,
                NSObject.FromObject("HTTPPort")!,
                NSObject.FromObject("HTTPEnable")!,
                NSObject.FromObject("HTTPSProxy")!,
                NSObject.FromObject("HTTPSPort")!,
                NSObject.FromObject("HTTPSEnable")!
            };
            var proxyDictionary = NSDictionary.FromObjectsAndKeys(values, keys);
            return proxyDictionary;
        }

        /// <inheritdoc />
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.GetHeadersOrder() != null)
            {
                _logger?.LogWarning("SetHeadersOrder is not supported on this platform and will be ignored.");
            }

            var session = _session.Value;

            var includeContentHeaders = false;
            var body = default(NSData);
            string? tempFilePath = null;

            try
            {
                if (request.Content != null)
                {
                    const long memoryBufferThreshold = 64 * 1024;
                    var contentLength = request.Content.Headers.ContentLength;

                    if (contentLength is null || contentLength > memoryBufferThreshold)
                    {
                        // Large or unknown-size bodies are buffered to a temporary file so
                        // NSURLSession streams them from disk instead of holding them in memory.
                        tempFilePath = Path.Combine(Path.GetTempPath(), $"securehttpclient-{Guid.NewGuid():N}.tmp");

                        long written;
                        await using (var file = File.Create(tempFilePath))
                        {
                            await request.Content.CopyToAsync(file, cancellationToken).ConfigureAwait(false);
                            written = file.Length;
                        }

                        if (written > 0 || request.Method != HttpMethod.Get)
                        {
                            includeContentHeaders = true;
                        }
                        else
                        {
                            TryDeleteFile(tempFilePath);
                            tempFilePath = null;
                        }
                    }
                    else
                    {
                        // Small bodies stay in memory (no temp file churn).
                        var bytes = await request.Content.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false);
                        if (bytes.Length > 0 || request.Method != HttpMethod.Get)
                        {
                            body = NSData.FromArray(bytes);
                            includeContentHeaders = true;
                        }
                    }
                }

                var headers = request.GetMergedRequestHeaders(includeContentHeaders).ToArray();

                var rq = new NSMutableUrlRequest
                {
                    AllowsCellularAccess = true,
                    CachePolicy = NSUrlRequestCachePolicy.UseProtocolCachePolicy,
                    HttpMethod = request.Method.ToString().ToUpperInvariant(),
                    Url = NSUrl.FromString(request.RequestUri!.AbsoluteUri),
                };

                foreach (var (name, value) in headers)
                {
                    rq[name] = value;
                }

                // Attempt HTTP/3 opportunistically from the first request (iOS 15+).
                // NSURLSession will fall back to HTTP/2 or HTTP/1.1 if QUIC is unavailable.
                rq.AssumesHttp3Capable = true;

                NSUrlSessionTask op;
                if (tempFilePath != null)
                {
                    op = session.CreateUploadTask(rq, NSUrl.FromFilename(tempFilePath));
                }
                else if (body != null)
                {
                    op = session.CreateUploadTask(rq, body);
                }
                else
                {
                    op = session.CreateDataTask(rq);
                }

                cancellationToken.ThrowIfCancellationRequested();

                var ret = new TaskCompletionSource<HttpResponseMessage>(TaskCreationOptions.RunContinuationsAsynchronously);

                lock (InflightRequests)
                {
                    InflightRequests[op] = new InflightOperation
                    {
                        FutureResponse = ret,
                        Request = request,
                        ResponseBody = new PipeStream(),
                        CancellationToken = cancellationToken,
                        TempFilePath = tempFilePath,
                    };
                }

                // The inflight operation now owns the temp file; DidCompleteWithError deletes it.
                tempFilePath = null;

                op.Resume();

                // Propagate cancellation to the underlying NSUrlSession task (so a cancelled request
                // does not keep running in the background) and dispose the registration once the
                // operation completes, so the callback is not retained by the token indefinitely.
                using var registration = cancellationToken.Register(() =>
                {
                    ret.TrySetCanceled(cancellationToken);
                    try
                    {
                        op.Cancel();
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogDebug(ex, "Ignoring exception while cancelling NSUrlSession task.");
                    }
                });

                return await ret.Task.ConfigureAwait(false);
            }
            finally
            {
                // Cleans up if setup failed before the request was registered; afterwards the
                // inflight operation owns the file and DidCompleteWithError deletes it.
                if (tempFilePath != null)
                {
                    TryDeleteFile(tempFilePath);
                }
            }
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (disposing && _session.IsValueCreated)
            {
                _session.Value.InvalidateAndCancel();
            }
            base.Dispose(disposing);
        }

        internal static void TryDeleteFile(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
            catch
            {
                // Best effort: the OS eventually cleans up the temp directory.
            }
        }
    }
}

#endif