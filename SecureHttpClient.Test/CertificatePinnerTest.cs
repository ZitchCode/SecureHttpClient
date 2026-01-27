using System;
using SecureHttpClient.Test.Helpers;
using System.Threading.Tasks;
using Xunit;

namespace SecureHttpClient.Test
{
    public class CertificatePinnerTest : TestBase, IClassFixture<TestPinFixture>
    {
        private readonly TestPinFixture _fixture;

        public CertificatePinnerTest(TestPinFixture testFixture) : base(testFixture)
        {
            _fixture = testFixture;
        }

        [Fact]
        public async Task CertificatePinnerTest_OneHost_Success()
        {
            AddCertificatePinner(_fixture.Hostname, _fixture.PinsOk);
            await GetAsync(_fixture.Page);
        }

        [Fact]
        public async Task CertificatePinnerTest_OneHost_Failure()
        {
            AddCertificatePinner(_fixture.Hostname, _fixture.PinsKo);
            await AssertExtensions.ThrowsTrustFailureAsync(() => GetAsync(_fixture.Page));
        }

        [Fact]
        public async Task CertificatePinnerTest_TwoHosts_Success()
        {
            AddCertificatePinner(_fixture.Hostname, _fixture.PinsOk);
            AddCertificatePinner(_fixture.Hostname2, _fixture.Pins2Ok);

            await GetAsync(_fixture.Page);
            await GetAsync(_fixture.Page2);
        }

        [Fact]
        public async Task CertificatePinnerTest_TwoHosts_FirstHostFails()
        {
            AddCertificatePinner(_fixture.Hostname, _fixture.PinsKo);
            AddCertificatePinner(_fixture.Hostname2, _fixture.Pins2Ok);

            await AssertExtensions.ThrowsTrustFailureAsync(() => GetAsync(_fixture.Page));
            await GetAsync(_fixture.Page2);
        }

        [Fact]
        public async Task CertificatePinnerTest_TwoHosts_SecondHostFails()
        {
            AddCertificatePinner(_fixture.Hostname, _fixture.PinsOk);
            AddCertificatePinner(_fixture.Hostname2, _fixture.PinsKo);

            await GetAsync(_fixture.Page);
            await AssertExtensions.ThrowsTrustFailureAsync(() => GetAsync(_fixture.Page2));
        }

        [Fact]
        public async Task CertificatePinnerTest_EccCertificate_Success()
        {
            AddCertificatePinner(_fixture.Hostname3, _fixture.Pins3Ok);
            await GetAsync(_fixture.Page3);
        }

        [Fact]
        public async Task CertificatePinnerTest_EccCertificate_Failure()
        {
            AddCertificatePinner(_fixture.Hostname3, _fixture.PinsKo);
            await AssertExtensions.ThrowsTrustFailureAsync(() => GetAsync(_fixture.Page3));
        }

        [Fact]
        public void CertificatePinnerTest_InvalidPattern()
        {
            Assert.Throws<ArgumentException>(() => AddCertificatePinner(_fixture.InvalidPattern, _fixture.PinsOk));
        }

        [Fact]
        public async Task CertificatePinnerTest_Wildcard1_Success()
        {
            AddCertificatePinner(_fixture.Wildcard1, _fixture.PinsOk);
            await GetAsync(_fixture.Page);
        }

        [Fact]
        public async Task CertificatePinnerTest_Wildcard2_Success()
        {
            AddCertificatePinner(_fixture.Wildcard2, _fixture.PinsOk);
            await GetAsync(_fixture.Page);
        }

        [Fact]
        public async Task CertificatePinnerTest_Merge_Success()
        {
            AddCertificatePinner(_fixture.Wildcard1, _fixture.PinsKo);
            AddCertificatePinner(_fixture.Hostname, _fixture.PinsOk);
            await GetAsync(_fixture.Page);
        }

        [Fact]
        public async Task CertificatePinnerTest_Wildcard1_Failure()
        {
            AddCertificatePinner(_fixture.Wildcard1, _fixture.PinsKo);
            await AssertExtensions.ThrowsTrustFailureAsync(() => GetAsync(_fixture.Page));
        }

        [Fact]
        public async Task CertificatePinnerTest_Wildcard2_Failure()
        {
            AddCertificatePinner(_fixture.Wildcard2, _fixture.PinsKo);
            await AssertExtensions.ThrowsTrustFailureAsync(() => GetAsync(_fixture.Page));
        }

        [Fact]
        public async Task CertificatePinnerTest_Merge_Failure()
        {
            AddCertificatePinner(_fixture.Wildcard1, _fixture.PinsKo);
            AddCertificatePinner(_fixture.Hostname, _fixture.PinsKo);
            await AssertExtensions.ThrowsTrustFailureAsync(() => GetAsync(_fixture.Page));
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
