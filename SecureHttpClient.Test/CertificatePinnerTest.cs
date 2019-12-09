using System.Threading.Tasks;
using SecureHttpClient.Test.Helpers;
using Xunit;

namespace SecureHttpClient.Test
{
    public class CertificatePinnerTest : TestBase
    {
        private const string Hostname = @"www.howsmyssl.com";
        private const string Page = @"https://www.howsmyssl.com/a/check";
        private static readonly string[] PinsOk = { @"sha256/t9m1w+JfolQ06TCH1SMZsQhUdYgu1lFIpajwn2iSOBo=" };
        private static readonly string[] PinsKo = { @"sha256/xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx=" };

        private const string Hostname2 = @"github.com";
        private const string Page2 = @"https://github.com";
        private static readonly string[] Pins2Ok = { @"sha256/o5oa5F4LbZEfeZ0kXDgmaU2K3sIPYtbQpT3EQLJZquM=" };
        private static readonly string[] Pins2Ko = { @"sha256/yyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyy=" };

        [Fact]
        public async Task CertificatePinnerTest_OneHost_Success()
        {
            AddCertificatePinner(Hostname, PinsOk);
            await GetAsync(Page);
        }

        [Fact]
        public async Task CertificatePinnerTest_OneHost_Failure()
        {
            AddCertificatePinner(Hostname, PinsKo);
            await AssertExtensions.ThrowsTrustFailureAsync(() => GetAsync(Page));
        }

        [Fact]
        public async Task CertificatePinnerTest_TwoHosts_Success()
        {
            AddCertificatePinner(Hostname, PinsOk);
            AddCertificatePinner(Hostname2, Pins2Ok);

            await GetAsync(Page);
            await GetAsync(Page2);
        }

        [Fact]
        public async Task CertificatePinnerTest_TwoHosts_FirstHostFails()
        {
            AddCertificatePinner(Hostname, PinsKo);
            AddCertificatePinner(Hostname2, Pins2Ok);

            await AssertExtensions.ThrowsTrustFailureAsync(() => GetAsync(Page));
            await GetAsync(Page2);
        }

        [Fact]
        public async Task CertificatePinnerTest_TwoHosts_SecondHostFails()
        {
            AddCertificatePinner(Hostname, PinsOk);
            AddCertificatePinner(Hostname2, Pins2Ko);

            await GetAsync(Page);
            await AssertExtensions.ThrowsTrustFailureAsync(() => GetAsync(Page2));
        }
    }
}
