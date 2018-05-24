using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace SecureHttpClient.Test
{
    public class CertificatePinnerTest
    {
        private const string Hostname = @"www.howsmyssl.com";
        private const string Page = @"https://www.howsmyssl.com/a/check";
        private static readonly string[] PinsOk = { @"sha256/fiKY8VhjQRb2voRmVXsqI0xPIREcwOVhpexrplrlqQY=" };
        private static readonly string[] PinsKo = { @"sha256/xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx=" };

        private const string Hostname2 = @"github.com";
        private const string Page2 = @"https://github.com/zitch-code/secure-httpclient";
        private static readonly string[] Pins2Ok = { @"sha256/o5oa5F4LbZEfeZ0kXDgmaU2K3sIPYtbQpT3EQLJZquM=" };
        private static readonly string[] Pins2Ko = { @"sha256/yyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyy=" };

        [Fact]
        public async Task CertificatePinnerTest_OneHost_Success()
        {
            var secureHttpClientHandler = SecureHttpClientHandlerBuilder.Build();
            secureHttpClientHandler.AddCertificatePinner(Hostname, PinsOk);

            await AssertCertificatePinnerSuccessAsync(Page, secureHttpClientHandler).ConfigureAwait(false);
        }

        [Fact]
        public async Task CertificatePinnerTest_OneHost_Failure()
        {
            var secureHttpClientHandler = SecureHttpClientHandlerBuilder.Build();
            secureHttpClientHandler.AddCertificatePinner(Hostname, PinsKo);

            await AssertCertificatePinnerFailureAsync(Page, secureHttpClientHandler).ConfigureAwait(false);
        }

        [Fact]
        public async Task CertificatePinnerTest_TwoHosts_Success()
        {
            var secureHttpClientHandler = SecureHttpClientHandlerBuilder.Build();
            secureHttpClientHandler.AddCertificatePinner(Hostname, PinsOk);
            secureHttpClientHandler.AddCertificatePinner(Hostname2, Pins2Ok);

            await AssertCertificatePinnerSuccessAsync(Page, secureHttpClientHandler).ConfigureAwait(false);
            await AssertCertificatePinnerSuccessAsync(Page2, secureHttpClientHandler).ConfigureAwait(false);
        }

        [Fact]
        public async Task CertificatePinnerTest_TwoHosts_FirstHostFails()
        {
            var secureHttpClientHandler = SecureHttpClientHandlerBuilder.Build();
            secureHttpClientHandler.AddCertificatePinner(Hostname, PinsKo);
            secureHttpClientHandler.AddCertificatePinner(Hostname2, Pins2Ok);

            await AssertCertificatePinnerFailureAsync(Page, secureHttpClientHandler).ConfigureAwait(false);
            await AssertCertificatePinnerSuccessAsync(Page2, secureHttpClientHandler).ConfigureAwait(false);
        }

        [Fact]
        public async Task CertificatePinnerTest_TwoHosts_SecondHostFails()
        {
            var secureHttpClientHandler = SecureHttpClientHandlerBuilder.Build();
            secureHttpClientHandler.AddCertificatePinner(Hostname, PinsOk);
            secureHttpClientHandler.AddCertificatePinner(Hostname2, Pins2Ko);

            await AssertCertificatePinnerSuccessAsync(Page, secureHttpClientHandler).ConfigureAwait(false);
            await AssertCertificatePinnerFailureAsync(Page2, secureHttpClientHandler).ConfigureAwait(false);
        }

        private static async Task<string> GetPageAsync(string page, HttpMessageHandler secureHttpClientHandler)
        {
            string result;
            using (var httpClient = new HttpClient(secureHttpClientHandler, false))
            using (var response = await httpClient.GetAsync(page).ConfigureAwait(false))
            {
                result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            }
            return result;
        }

        private static async Task AssertCertificatePinnerSuccessAsync(string page, HttpMessageHandler secureHttpClientHandler)
        {
            await GetPageAsync(page, secureHttpClientHandler).ConfigureAwait(false);
            Assert.True(true);
        }

        private static async Task AssertCertificatePinnerFailureAsync(string page, HttpMessageHandler secureHttpClientHandler)
        {
            var throwsExpectedException = false;
            try
            {
                await GetPageAsync(page, secureHttpClientHandler).ConfigureAwait(false);
            }
            catch (WebException ex)
            {
                throwsExpectedException = ex.Status == WebExceptionStatus.TrustFailure;
            }
            Assert.True(throwsExpectedException);
        }
    }
}
