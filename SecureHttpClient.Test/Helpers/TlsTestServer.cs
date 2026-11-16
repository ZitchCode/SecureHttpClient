using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SecureHttpClient.Test.Helpers
{
    /// <summary>
    /// Minimal HTTPS server bound to the loopback interface. It serves a certificate issued by a
    /// throw-away certificate authority, so tests can exercise custom trusted roots without relying
    /// on an external service.
    /// </summary>
    internal sealed class TlsTestServer : IAsyncDisposable
    {
        private const string Response = "HTTP/1.1 200 OK\r\nContent-Length: 2\r\nConnection: close\r\n\r\nok";
        private const string PfxPassword = "securehttpclient-test";

        private readonly TcpListener _listener;
        private readonly CancellationTokenSource _cts = new();
        private readonly List<Task> _clientTasks = [];
        private readonly SslStreamCertificateContext _certificateContext;
        private readonly Task _acceptLoop;

        internal X509Certificate2 CertificateAuthority { get; }

        internal int Port { get; }

        internal TlsTestServer()
        {
            (CertificateAuthority, var certificate) = CreateCertificates();
            _certificateContext = SslStreamCertificateContext.Create(certificate, null, offline: true);

            // The certificate only covers "localhost"; the test also reaches this server through
            // 127.0.0.1, which the certificate does not cover (wrong name).
            _listener = new TcpListener(IPAddress.Loopback, 0);
            _listener.Start();
            Port = ((IPEndPoint)_listener.LocalEndpoint).Port;

            _acceptLoop = AcceptLoopAsync();
        }

        public async ValueTask DisposeAsync()
        {
            await _cts.CancelAsync();
            _listener.Stop();

            try
            {
                await _acceptLoop;
            }
            catch (Exception)
            {
                // Expected when the listener is stopped while accepting.
            }

            Task[] tasks;
            lock (_clientTasks)
            {
                tasks = [.. _clientTasks];
            }

            try
            {
                await Task.WhenAll(tasks).WaitAsync(TimeSpan.FromSeconds(5));
            }
            catch (Exception)
            {
                // Best effort: a client may still be connected.
            }

            _cts.Dispose();
            CertificateAuthority.Dispose();
        }

        private async Task AcceptLoopAsync()
        {
            while (!_cts.IsCancellationRequested)
            {
                TcpClient client;
                try
                {
                    client = await _listener.AcceptTcpClientAsync(_cts.Token);
                }
                catch (Exception)
                {
                    return;
                }

                var task = HandleClientAsync(client, _cts.Token);
                lock (_clientTasks)
                {
                    _clientTasks.Add(task);
                }
            }
        }

        private async Task HandleClientAsync(TcpClient client, CancellationToken cancellationToken)
        {
            using (client)
            {
                try
                {
                    using var sslStream = new SslStream(client.GetStream(), leaveInnerStreamOpen: false);
                    await sslStream.AuthenticateAsServerAsync(
                        new SslServerAuthenticationOptions
                        {
                            ServerCertificateContext = _certificateContext,
                            EnabledSslProtocols = SslProtocols.None,
                            ApplicationProtocols = [SslApplicationProtocol.Http11],
                        },
                        cancellationToken);

                    var buffer = new byte[4096];
                    var received = 0;
                    while (received < buffer.Length)
                    {
                        var read = await sslStream.ReadAsync(buffer.AsMemory(received), cancellationToken);
                        if (read == 0)
                        {
                            break;
                        }

                        received += read;
                        if (received >= 4 &&
                            buffer[received - 4] == '\r' && buffer[received - 3] == '\n' &&
                            buffer[received - 2] == '\r' && buffer[received - 1] == '\n')
                        {
                            break;
                        }
                    }

                    await sslStream.WriteAsync(Encoding.ASCII.GetBytes(Response), cancellationToken);
                    await sslStream.FlushAsync(cancellationToken);
                }
                catch (Exception)
                {
                    // Swallowed: the client rejects the certificate (expected) or closes the connection.
                }
            }
        }

        private static (X509Certificate2 CertificateAuthority, X509Certificate2 Certificate) CreateCertificates()
        {
            var notBefore = DateTimeOffset.UtcNow.AddDays(-1);
            var notAfter = DateTimeOffset.UtcNow.AddYears(5);

            using var caKey = RSA.Create(2048);
            var caRequest = new CertificateRequest("CN=SecureHttpClient Test Root CA", caKey, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            caRequest.CertificateExtensions.Add(new X509BasicConstraintsExtension(certificateAuthority: true, hasPathLengthConstraint: false, pathLengthConstraint: 0, critical: true));
            caRequest.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.KeyCertSign | X509KeyUsageFlags.CrlSign, critical: true));
            caRequest.CertificateExtensions.Add(new X509SubjectKeyIdentifierExtension(caRequest.PublicKey, critical: false));
            var certificateAuthority = caRequest.CreateSelfSigned(notBefore, notAfter);

            using var leafKey = RSA.Create(2048);
            var leafRequest = new CertificateRequest("CN=localhost", leafKey, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            leafRequest.CertificateExtensions.Add(new X509BasicConstraintsExtension(certificateAuthority: false, hasPathLengthConstraint: false, pathLengthConstraint: 0, critical: true));
            leafRequest.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature | X509KeyUsageFlags.KeyEncipherment, critical: true));
            leafRequest.CertificateExtensions.Add(new X509EnhancedKeyUsageExtension(new OidCollection { new("1.3.6.1.5.5.7.3.1") }, critical: false));

            var subjectAlternativeNames = new SubjectAlternativeNameBuilder();
            subjectAlternativeNames.AddDnsName("localhost");
            leafRequest.CertificateExtensions.Add(subjectAlternativeNames.Build());

            using var publicCertificate = leafRequest.Create(certificateAuthority, notBefore, notAfter, RandomNumberGenerator.GetBytes(16));
            using var certificateWithKey = publicCertificate.CopyWithPrivateKey(leafKey);

            // Round-trip through PKCS#12 so the private key is stored in a way the platform TLS
            // stack accepts (on Windows, SChannel cannot use an ephemeral key for server auth).
            // DefaultKeySet is the only flag supported by every platform (Apple rejects Exportable).
            var pfx = certificateWithKey.Export(X509ContentType.Pkcs12, PfxPassword);
            var certificate = X509CertificateLoader.LoadPkcs12(pfx, PfxPassword, X509KeyStorageFlags.DefaultKeySet);

            return (certificateAuthority, certificate);
        }
    }
}

