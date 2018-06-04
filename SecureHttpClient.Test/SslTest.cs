using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Xunit;

namespace SecureHttpClient.Test
{
    public class SslTest
    {
        [Fact]
        public async Task SslTest_ExpiredCertificate()
        {
            const string page = @"https://expired.badssl.com/";
            var expectedExceptions = new List<string> { "Javax.Net.Ssl.SSLHandshakeException", "System.Net.WebException" };
            var throwsExpectedException = false;
            try
            {
                await GetPageAsync(page).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                var exceptionType = ex.GetType().ToString();
                throwsExpectedException = expectedExceptions.Contains(exceptionType);
            }
            Assert.True(throwsExpectedException);
        }

        [Fact]
        public async Task SslTest_WrongHostCertificate()
        {
            const string page = @"https://wrong.host.badssl.com/";
            var expectedExceptions = new List<string> { "Javax.Net.Ssl.SSLPeerUnverifiedException", "System.Net.WebException" };
            var throwsExpectedException = false;
            try
            {
                await GetPageAsync(page).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                var exceptionType = ex.GetType().ToString();
                throwsExpectedException = expectedExceptions.Contains(exceptionType);
            }
            Assert.True(throwsExpectedException);
        }

        [Fact]
        public async Task SslTest_SelfSignedCertificate()
        {
            const string page = @"https://self-signed.badssl.com/";
            var expectedExceptions = new List<string> { "Javax.Net.Ssl.SSLHandshakeException", "System.Net.WebException" };
            var throwsExpectedException = false;
            try
            {
                await GetPageAsync(page).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                var exceptionType = ex.GetType().ToString();
                throwsExpectedException = expectedExceptions.Contains(exceptionType);
            }
            Assert.True(throwsExpectedException);
        }

        [Fact]
        public async Task SslTest_UntrustedRootCertificate()
        {
            const string page = @"https://untrusted-root.badssl.com/";
            var expectedExceptions = new List<string> { "Javax.Net.Ssl.SSLHandshakeException", "System.Net.WebException" };
            var throwsExpectedException = false;
            try
            {
                await GetPageAsync(page).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                var exceptionType = ex.GetType().ToString();
                throwsExpectedException = expectedExceptions.Contains(exceptionType);
            }
            Assert.True(throwsExpectedException);
        }

        [Fact]
        public async Task SslTest_SpecificTrustedRootCertificate()
        {
            // NB: Using this feature on iOS 11 requires setting NSExceptionDomains in Info.plist,
            // particularly NSExceptionRequiresForwardSecrecy=NO : https://stackoverflow.com/q/46316604/5652125
            const string page = @"https://untrusted-root.badssl.com/";
            await GetPageAsync(page, caCert: System.Text.Encoding.ASCII.GetBytes(untrusted_root_badssl_com_certificate)).ConfigureAwait(false);
            Assert.True(true);
        }

        [Fact]
        public async Task SslTest_OnlyTrustSpecificRootCertificate()
        {
            const string page = @"https://badssl.com"; // Has valid public cert, but not signed by our custom root
            var expectedExceptions = new List<string> { "Javax.Net.Ssl.SSLHandshakeException", "System.Net.WebException" };
            var throwsExpectedException = false;
            try
            {
                await GetPageAsync(page, caCert: System.Text.Encoding.ASCII.GetBytes(untrusted_root_badssl_com_certificate)).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                var exceptionType = ex.GetType().ToString();
                throwsExpectedException = expectedExceptions.Contains(exceptionType);
            }
            Assert.True(throwsExpectedException);
        }

        [Fact(Skip = "Unsupported on Android, not implemented on UWP")]
        public async Task SslTest_RevokedCertificate()
        {
            const string page = @"https://revoked.badssl.com/";
            var expectedExceptions = new List<string> { "Javax.Net.Ssl.SSLHandshakeException", "System.Net.WebException" };
            var throwsExpectedException = false;
            try
            {
                await GetPageAsync(page).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                var exceptionType = ex.GetType().ToString();
                throwsExpectedException = expectedExceptions.Contains(exceptionType);
            }
            Assert.True(throwsExpectedException);
        }

        [Fact]
        public async Task SslTest_Sha256Certificate()
        {
            const string page = @"https://sha256.badssl.com/";
            await GetPageAsync(page).ConfigureAwait(false);
            Assert.True(true);
        }

        [Fact]
        public async Task SslTest_MissingClientCertificate()
        {
            const string page = @"https://client-cert-missing.badssl.com/";
            await Assert.ThrowsAsync<HttpResponseException>(async () => await GetPageAsync(page).ConfigureAwait(false));
        }

        [Fact]
        public async Task SslTest_ClientCertificate()
        {
            const string page = @"https://client.badssl.com/";
            var base64Cert = "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";
            await GetPageAsync(page, clientCert: Convert.FromBase64String(base64Cert), certPassword: "badssl.com").ConfigureAwait(false);
            Assert.True(true);
        }

        [Fact]
        public async Task SslTest_SubjectAltName()
        {
            const string page = @"https://www.prive.livretzesto.fr/";
            await GetPageAsync(page).ConfigureAwait(false);
            Assert.True(true);
        }

        [Fact]
        public async Task SslTest_HowsMySsl()
        {
            const string expectedTlsVersion = "TLS 1.2";
            const string expectedRating = "[Probably Okay|Improvable]";

            const string page = @"https://www.howsmyssl.com/a/check";
            var result = await GetPageAsync(page).ConfigureAwait(false);

            var json = JToken.Parse(result);
            var actualTlsVersion = json["tls_version"].ToString();
            var actualRating = json["rating"].ToString();

            Assert.Equal(expectedTlsVersion, actualTlsVersion);
            Assert.Matches(expectedRating, actualRating);
        }

        class HttpResponseException : Exception
        {
            public HttpStatusCode Code { get; private set; }
            public HttpResponseException(HttpStatusCode code, string message) : base(message)
            {
                Code = code;
            }
        }

        private static async Task<string> GetPageAsync(string page, string hostname = null, string[] pins = null, byte[] clientCert = null, string certPassword = null, byte[] caCert = null)
        {
            var secureHttpClientHandler = new SecureHttpClientHandler();
            if (pins != null)
            {
                secureHttpClientHandler.AddCertificatePinner(hostname, pins);
            }
            if (clientCert != null)
            {
                var provider = new ClientCertificateProvider();
                provider.Import(clientCert, certPassword);
                secureHttpClientHandler.SetClientCertificates(provider);
            }
            if (caCert != null)
            {
                secureHttpClientHandler.SetTrustedRoots(caCert);
            }
            string result;
            using (var httpClient = new HttpClient(secureHttpClientHandler))
            using (var response = await httpClient.GetAsync(page).ConfigureAwait(false))
            {
                result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                if (!response.IsSuccessStatusCode)
                {
                    throw new HttpResponseException(response.StatusCode, result);
                }
            }
            return result;
        }

        private static readonly string untrusted_root_badssl_com_certificate = @"-----BEGIN CERTIFICATE-----
MIIGfjCCBGagAwIBAgIJAJeg/PrX5Sj9MA0GCSqGSIb3DQEBCwUAMIGBMQswCQYD
VQQGEwJVUzETMBEGA1UECAwKQ2FsaWZvcm5pYTEWMBQGA1UEBwwNU2FuIEZyYW5j
aXNjbzEPMA0GA1UECgwGQmFkU1NMMTQwMgYDVQQDDCtCYWRTU0wgVW50cnVzdGVk
IFJvb3QgQ2VydGlmaWNhdGUgQXV0aG9yaXR5MB4XDTE2MDcwNzA2MzEzNVoXDTM2
MDcwMjA2MzEzNVowgYExCzAJBgNVBAYTAlVTMRMwEQYDVQQIDApDYWxpZm9ybmlh
MRYwFAYDVQQHDA1TYW4gRnJhbmNpc2NvMQ8wDQYDVQQKDAZCYWRTU0wxNDAyBgNV
BAMMK0JhZFNTTCBVbnRydXN0ZWQgUm9vdCBDZXJ0aWZpY2F0ZSBBdXRob3JpdHkw
ggIiMA0GCSqGSIb3DQEBAQUAA4ICDwAwggIKAoICAQDKQtPMhEH073gis/HISWAi
bOEpCtOsatA3JmeVbaWal8O/5ZO5GAn9dFVsGn0CXAHR6eUKYDAFJLa/3AhjBvWa
tnQLoXaYlCvBjodjLEaFi8ckcJHrAYG9qZqioRQ16Yr8wUTkbgZf+er/Z55zi1yn
CnhWth7kekvrwVDGP1rApeLqbhYCSLeZf5W/zsjLlvJni9OrU7U3a9msvz8mcCOX
fJX9e3VbkD/uonIbK2SvmAGMaOj/1k0dASkZtMws0Bk7m1pTQL+qXDM/h3BQZJa5
DwTcATaa/Qnk6YHbj/MaS5nzCSmR0Xmvs/3CulQYiZJ3kypns1KdqlGuwkfiCCgD
yWJy7NE9qdj6xxLdqzne2DCyuPrjFPS0mmYimpykgbPnirEPBF1LW3GJc9yfhVXE
Cc8OY8lWzxazDNNbeSRDpAGbBeGSQXGjAbliFJxwLyGzZ+cG+G8lc+zSvWjQu4Xp
GJ+dOREhQhl+9U8oyPX34gfKo63muSgo539hGylqgQyzj+SX8OgK1FXXb2LS1gxt
VIR5Qc4MmiEG2LKwPwfU8Yi+t5TYjGh8gaFv6NnksoX4hU42gP5KvjYggDpR+NSN
CGQSWHfZASAYDpxjrOo+rk4xnO+sbuuMk7gORsrl+jgRT8F2VqoR9Z3CEdQxcCjR
5FsfTymZCk3GfIbWKkaeLQIDAQABo4H2MIHzMB0GA1UdDgQWBBRvx4NzSbWnY/91
3m1u/u37l6MsADCBtgYDVR0jBIGuMIGrgBRvx4NzSbWnY/913m1u/u37l6MsAKGB
h6SBhDCBgTELMAkGA1UEBhMCVVMxEzARBgNVBAgMCkNhbGlmb3JuaWExFjAUBgNV
BAcMDVNhbiBGcmFuY2lzY28xDzANBgNVBAoMBkJhZFNTTDE0MDIGA1UEAwwrQmFk
U1NMIFVudHJ1c3RlZCBSb290IENlcnRpZmljYXRlIEF1dGhvcml0eYIJAJeg/PrX
5Sj9MAwGA1UdEwQFMAMBAf8wCwYDVR0PBAQDAgEGMA0GCSqGSIb3DQEBCwUAA4IC
AQBQU9U8+jTRT6H9AIFm6y50tXTg/ySxRNmeP1Ey9Zf4jUE6yr3Q8xBv9gTFLiY1
qW2qfkDSmXVdBkl/OU3+xb5QOG5hW7wVolWQyKREV5EvUZXZxoH7LVEMdkCsRJDK
wYEKnEErFls5WPXY3bOglBOQqAIiuLQ0f77a2HXULDdQTn5SueW/vrA4RJEKuWxU
iD9XPnVZ9tPtky2Du7wcL9qhgTddpS/NgAuLO4PXh2TQ0EMCll5reZ5AEr0NSLDF
c/koDv/EZqB7VYhcPzr1bhQgbv1dl9NZU0dWKIMkRE/T7vZ97I3aPZqIapC2ulrf
KrlqjXidwrGFg8xbiGYQHPx3tHPZxoM5WG2voI6G3s1/iD+B4V6lUEvivd3f6tq7
d1V/3q1sL5DNv7TvaKGsq8g5un0TAkqaewJQ5fXLigF/yYu5a24/GUD783MdAPFv
gWz8F81evOyRfpf9CAqIswMF+T6Dwv3aw5L9hSniMrblkg+ai0K22JfoBcGOzMtB
Ke/Ps2Za56dTRoY/a4r62hrcGxufXd0mTdPaJLw3sJeHYjLxVAYWQq4QKJQWDgTS
dAEWyN2WXaBFPx5c8KIW95Eu8ShWE00VVC3oA4emoZ2nrzBXLrUScifY6VaYYkkR
2O2tSqU8Ri3XRdgpNPDWp8ZL49KhYGYo3R/k98gnMHiY5g==
-----END CERTIFICATE-----";
    }
}
