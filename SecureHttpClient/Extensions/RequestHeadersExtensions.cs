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
            // Merge request and content headers into a single ordered list, combining the values of
            // header names present in both collections (case-insensitive) so a header is emitted once.
            var merged = new List<KeyValuePair<string, IEnumerable<string>>>(request.Headers);

            if (includeContentHeaders && request.Content != null)
            {
                foreach (var header in request.Content.Headers)
                {
                    if (ExcludedFramingHeaders.Contains(header.Key, StringComparer.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    var index = merged.FindIndex(h => string.Equals(h.Key, header.Key, StringComparison.OrdinalIgnoreCase));
                    if (index >= 0)
                    {
                        merged[index] = new KeyValuePair<string, IEnumerable<string>>(
                            merged[index].Key, merged[index].Value.Concat(header.Value));
                    }
                    else
                    {
                        merged.Add(header);
                    }
                }
            }

            foreach (var (name, values) in merged)
            {
                yield return new KeyValuePair<string, string>(name, string.Join(GetSeparator(name), values));
            }
        }

        private static string GetSeparator(string headerName)
        {
            // A few headers have special multi-value separators per RFC 7230.
            if (headerName.Equals("User-Agent", StringComparison.OrdinalIgnoreCase))
            {
                return " ";
            }
            if (headerName.Equals("Cookie", StringComparison.OrdinalIgnoreCase))
            {
                return "; ";
            }
            return ",";
        }
    }
}