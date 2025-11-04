using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace SecureHttpClient.Test.Helpers
{
    public static class ResourceHelper
    {
        public static async Task<string> GetStringAsync(string resource)
        {
            await using var resourceStream = GetResourceStream(resource);
            using var reader = new StreamReader(resourceStream);
            var res = await reader.ReadToEndAsync();
            return res;
        }

        public static async Task<byte[]> GetBytesAsync(string resource)
        {
            await using var resourceStream = GetResourceStream(resource);
            using var memoryStream = new MemoryStream();
            await resourceStream.CopyToAsync(memoryStream);
            return memoryStream.ToArray();
        }

        private static Stream GetResourceStream(string resource)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = assembly.GetManifestResourceNames().Single(str => str.EndsWith($"res.{resource}"));
            var resourceStream = assembly.GetManifestResourceStream(resourceName);
            return resourceStream;
        }
    }
}
