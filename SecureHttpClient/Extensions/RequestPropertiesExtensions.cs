using System.Net.Http;

namespace SecureHttpClient.Extensions
{
    /// <summary>
    /// Request properties extensions
    /// </summary>
    public static class RequestPropertiesExtensions
    {
        private const string HeadersOrderPropertyKey = "headersOrder";
        private static readonly HttpRequestOptionsKey<string[]> HeadersOrderKey = new(HeadersOrderPropertyKey);

        /// <summary>
        /// Set the headers order
        /// </summary>
        /// <param name="request">The request</param>
        /// <param name="headers">The ordered headers names</param>
        public static void SetHeadersOrder(this HttpRequestMessage request, params string[] headers)
        {
            request.Options.Set(HeadersOrderKey, headers);
        }

        internal static string[]? GetHeadersOrder(this HttpRequestMessage request)
        {
            return request.Options.TryGetValue(HeadersOrderKey, out var headers) ? headers : null;
        }
    }
}