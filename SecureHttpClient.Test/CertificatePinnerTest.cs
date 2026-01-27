using System;
using SecureHttpClient.Test.Helpers;
using System.Threading.Tasks;
using Xunit;

namespace SecureHttpClient.Test
{
    public class CertificatePinnerTest : TestBase, IClassFixture<TestFixture>
    {
        private const string Hostname = "www.howsmyssl.com";
        private const string Wildcard1 = "*.howsmyssl.com";
        private const string Wildcard2 = "**.howsmyssl.com";
        private const string InvalidPattern = "*.*.howsmyssl.com";
        private const string Page = "https://www.howsmyssl.com/a/check";
        private static readonly string[] PinsOk = ["sha256/6ToiIE9b1hro92gx1pbb6saffAG3p0jRIN8M5wgT0GM="];
        private static readonly string[] PinsKo = ["sha256/xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx="];

        private const string Hostname2 = "github.com";
        private const string Page2 = "https://github.com";
        private static readonly string[] Pins2Ok = ["sha256/HKlrX9VOPI9IC6usNi99M9wgWigfPdJmPCF7IPg0BVE="];
        private static readonly string[] Pins2Ko = ["sha256/yyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyy="];

        private const string Hostname3 = "ecc256.badssl.com";
        private const string Page3 = "https://ecc256.badssl.com/";
        private static readonly string[] Pins3Ok = ["sha256/5sHD7cOasciMsgZIFvJs56nfhmPpuoJ+Cot9ZS3harA="];
        private static readonly string[] Pins3Ko = ["sha256/zzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzz="];

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

        [Fact]
        public void CertificatePinnerTest_InvalidPattern()
        {
            Assert.Throws<ArgumentException>(() => AddCertificatePinner(InvalidPattern, PinsOk));
        }

        [Fact]
        public async Task CertificatePinnerTest_Wildcard1_Success()
        {
            AddCertificatePinner(Wildcard1, PinsOk);
            await GetAsync(Page);
        }

        [Fact]
        public async Task CertificatePinnerTest_Wildcard2_Success()
        {
            AddCertificatePinner(Wildcard2, PinsOk);
            await GetAsync(Page);
        }

        [Fact]
        public async Task CertificatePinnerTest_Merge_Success()
        {
            AddCertificatePinner(Wildcard1, PinsKo);
            AddCertificatePinner(Hostname, PinsOk);
            await GetAsync(Page);
        }

        [Fact]
        public async Task CertificatePinnerTest_Wildcard1_Failure()
        {
            AddCertificatePinner(Wildcard1, PinsKo);
            await AssertExtensions.ThrowsTrustFailureAsync(() => GetAsync(Page));
        }

        [Fact]
        public async Task CertificatePinnerTest_Wildcard2_Failure()
        {
            AddCertificatePinner(Wildcard2, PinsKo);
            await AssertExtensions.ThrowsTrustFailureAsync(() => GetAsync(Page));
        }

        [Fact]
        public async Task CertificatePinnerTest_Merge_Failure()
        {
            AddCertificatePinner(Wildcard1, PinsKo);
            AddCertificatePinner(Hostname, PinsKo);
            await AssertExtensions.ThrowsTrustFailureAsync(() => GetAsync(Page));
        }

        [Theory]
        [InlineData("abc.example.com", "abc.example.com", true)]
        [InlineData("abc.example.com", "def.example.com", false)]
        [InlineData("*.example.com", "abc.example.com", true)]
        [InlineData("*.example.com", "example.com", false)]
        [InlineData("*.example.com", "abc.def.example.com", false)]
        [InlineData("*.example.com", "abc.example.org", false)]
        [InlineData("**.example.com", "example.com", true)]
        [InlineData("**.example.com", "abc.example.com", true)]
        [InlineData("**.example.com", "abc.def.example.com", true)]
        [InlineData("**.example.com", "abc.def.example.org", false)]
        public void CertificatePinnerTest_MatchesPattern(string pattern, string hostname, bool expected)
        {
            var actual = CertificatePinning.CertificatePinner.MatchesPattern(pattern, hostname);
            Assert.Equal(expected, actual);
        }
    }
}
