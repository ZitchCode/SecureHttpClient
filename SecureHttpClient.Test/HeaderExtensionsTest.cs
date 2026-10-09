using System;
using System.Linq;
using System.Net.Http;
using SecureHttpClient.Extensions;
using Xunit;

namespace SecureHttpClient.Test
{
    public class HeaderExtensionsTest
    {
        // Request and content headers with the same name must be merged into a single entry, and
        // multi-value headers must use the right separator (Cookie uses "; ", not ",").
        [Fact]
        public void HeaderExtensionsTest_MergesDuplicateHeadersAndUsesCookieSeparator()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "https://example.com/");
            request.Headers.Add("Cookie", "a=1");
            request.Headers.Add("Cookie", "b=2");
            request.Headers.Add("X-Custom", "1");
            request.Content = new StringContent("x");
            request.Content.Headers.Add("X-Custom", "2");

            var merged = request.GetMergedRequestHeaders(includeContentHeaders: true)
                .ToDictionary(h => h.Key, h => h.Value, StringComparer.OrdinalIgnoreCase);

            // Two Cookie values are kept as a single header, joined with "; ".
            Assert.Equal("a=1; b=2", merged["Cookie"]);

            // X-Custom is present in both the request and the content headers; it must appear once
            // with its values concatenated with ",".
            Assert.Equal("1,2", merged["X-Custom"]);
        }
    }
}
