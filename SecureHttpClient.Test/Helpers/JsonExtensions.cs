using System.Collections.Generic;
using System.Text.Json;

namespace SecureHttpClient.Test.Helpers
{
    public static class JsonExtensions
    {
        public static Dictionary<string, string> GetDictionary(this JsonElement jsonElement)
        {
            var d = new Dictionary<string, string>();
            foreach (var p in jsonElement.EnumerateObject())
            {
                d[p.Name] = p.Value.GetString();
            }
            return d;
        }
    }
}
