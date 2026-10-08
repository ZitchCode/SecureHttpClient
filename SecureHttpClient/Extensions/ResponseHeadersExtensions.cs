using System.Collections.Generic;
using System.Net.Http;

namespace SecureHttpClient.Extensions
{
    internal static class ResponseHeadersExtensions
    {
        // Route a response header to a single collection. .NET rejects a header added to the
        // wrong collection, so trying response headers first and falling back to content headers
        // keeps the two mutually exclusive (like the default handler). Adding to both would leak
        // general/custom headers (Set-Cookie, Keep-Alive, X-*, ...) into Content.Headers.
        internal static void AddResponseHeader(this HttpResponseMessage response, string name, IEnumerable<string> values)
        {
            if (!response.Headers.TryAddWithoutValidation(name, values))
            {
                response.Content.Headers.TryAddWithoutValidation(name, values);
            }
        }
    }
}
