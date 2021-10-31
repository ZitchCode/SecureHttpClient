using System.Threading.Tasks;
using SecureHttpClient.Test.Helpers;
using Xunit;

namespace SecureHttpClient.Test
{
    public class CertificatePinnerTest : TestBase, IClassFixture<TestFixture>
    {
        private const string Hostname = @"www.howsmyssl.com";
        private const string Page = @"https://www.howsmyssl.com/a/check";
        private static readonly string[] PinsOk = { @"sha256/O1whOwXMBssoL+vFX8/ieNVsUVgl3Qxdmv3vMr6zOmg=" };
        private static readonly string[] PinsKo = { @"sha256/xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx=" };

        private const string Hostname2 = @"github.com";
        private const string Page2 = @"https://github.com";
        private static readonly string[] Pins2Ok = { @"sha256/7zwAM/YU+zd8BZMCl0J943URuyF35zefKC7QIoxTq+U=" };
        private static readonly string[] Pins2Ko = { @"sha256/yyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyy=" };

        private const string Hostname3 = @"ecc256.badssl.com";
        private const string Page3 = @"https://ecc256.badssl.com/";
        private static readonly string[] Pins3Ok = { @"sha256/fxf7kzJ2eD+yjn1GfHWRkHU24U297K69jSfvf387A0c=" };
        private static readonly string[] Pins3Ko = { @"sha256/zzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzz=" };

        public CertificatePinnerTest(TestFixture testFixture) : base(testFixture)
        {
        }

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

        [Fact]
        public async Task CertificatePinnerTest_EccCertificate_Success()
        {
            AddCertificatePinner(Hostname3, Pins3Ok);
            await GetAsync(Page3);
        }

        [Fact]
        public async Task CertificatePinnerTest_EccCertificate_Failure()
        {
            AddCertificatePinner(Hostname3, Pins3Ko);
            await AssertExtensions.ThrowsTrustFailureAsync(() => GetAsync(Page3));
        }
    }
}
