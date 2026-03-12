using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace SecureHttpClient.Extensions
{
    internal static class RequestHeadersExtensions
    {
        private static readonly string[] ExcludedFramingHeaders =
        [
            "Content-Length",
            "Transfer-Encoding"
        ];

        internal static IEnumerable<KeyValuePair<string, string>> GetMergedRequestHeaders(this HttpRequestMessage request, bool includeContentHeaders)
        {
            IEnumerable<KeyValuePair<string, IEnumerable<string>>> headers = request.Headers;

            if (includeContentHeaders && request.Content != null)
            {
                headers = headers.Union(request.Content.Headers.Where(h => !ExcludedFramingHeaders.Contains(h.Key, StringComparer.OrdinalIgnoreCase)));
            }

            foreach (var (name, values) in headers)
            {
                var headerSeparator = name.Equals("User-Agent", StringComparison.OrdinalIgnoreCase) ? " " : ",";
                yield return new KeyValuePair<string, string>(name, string.Join(headerSeparator, values));
            }
        }
    }
}