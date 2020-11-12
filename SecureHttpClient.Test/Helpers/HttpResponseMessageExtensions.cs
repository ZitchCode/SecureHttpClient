using System.Net.Http;
using System.Threading.Tasks;

namespace SecureHttpClient.Test.Helpers
{
    internal static class HttpResponseMessageExtensions
    {
        public static async Task<string> ReceiveString(this Task<HttpResponseMessage> response)
        {
            using var resp = await response.ConfigureAwait(false);
            if (resp == null) return null;
            return await resp.Content.ReadAsStringAsync().ConfigureAwait(false);
        }

        public static async Task<byte[]> ReceiveBytes(this Task<HttpResponseMessage> response)
        {
            using var resp = await response.ConfigureAwait(false);
            if (resp == null) return null;
            return await resp.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
        }
    }
}
