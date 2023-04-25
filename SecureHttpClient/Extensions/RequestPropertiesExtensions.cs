using System.Net.Http;

namespace SecureHttpClient.Extensions
{
    /// <summary>
    /// Request properties extensions
    /// </summary>
    public static class RequestPropertiesExtensions
    {
        private const string HeadersOrderPropertyKey = "headersOrder";

        /// <summary>
        /// Set the headers order
        /// </summary>
        /// <param name="request">The request</param>
        /// <param name="headers">The ordered headers names</param>
        public static void SetHeadersOrder(this HttpRequestMessage request, params string[] headers)
        {
            request.Options.Set(new HttpRequestOptionsKey<object>(HeadersOrderPropertyKey), headers);
        }

        internal static string[] GetHeadersOrder(this HttpRequestMessage request)
        {
            if (request.Options.TryGetValue(new HttpRequestOptionsKey<object>(HeadersOrderPropertyKey), out var value))
            {
                return (string[])value;
            }
            return null;
        }
    }
}