using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Authentication;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xunit;

namespace SecureHttpClient.Test.Helpers
{
    public static class AssertExtensions
    {
        private const int ServerSecurityAccessFailure = -2146697202; // 0x800C000E - 14
        private const int InvalidDateInCertificate = -2147012859; // 0x80072F05 - 12037
        private const int InvalidHostNameInCertificate = -2147012858; // 0x80072F06 - 12038
        private const int InvalidCertificateAuthority = -2147012851; // 0x80072F0D - 12045

        private static readonly List<int> SecurityErrors = new List<int>
        {
            ServerSecurityAccessFailure,
            InvalidDateInCertificate,
            InvalidHostNameInCertificate,
            InvalidCertificateAuthority
        };

        public static async Task ThrowsTrustFailureAsync(Func<Task> testCode)
        {
            var exception = await Assert.ThrowsAsync<HttpRequestException>(testCode).ConfigureAwait(false);
            Assert.IsType<AuthenticationException>(exception.InnerException);
        }
    }
}
