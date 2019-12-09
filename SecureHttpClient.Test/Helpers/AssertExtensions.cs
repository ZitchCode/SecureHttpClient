using System;
using System.Net.Http;
using System.Security.Authentication;
using System.Threading.Tasks;
using Xunit;

namespace SecureHttpClient.Test.Helpers
{
    public static class AssertExtensions
    {
        public static async Task ThrowsTrustFailureAsync(Func<Task> testCode)
        {
            var exception = await Assert.ThrowsAsync<HttpRequestException>(testCode).ConfigureAwait(false);
            Assert.IsType<AuthenticationException>(exception.InnerException);
        }
    }
}
