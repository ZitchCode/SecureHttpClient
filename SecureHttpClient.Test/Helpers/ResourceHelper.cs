using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace SecureHttpClient.Test.Helpers
{
    public static class ResourceHelper
    {
        public static Task<string> GetStringAsync(string resource)
        {
            using var resourceStream = GetResourceStream(resource);
            using var reader = new StreamReader(resourceStream);
            return reader.ReadToEndAsync();
        }

        public static async Task<byte[]> GetBytesAsync(string resource)
        {
            using var resourceStream = GetResourceStream(resource);
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
