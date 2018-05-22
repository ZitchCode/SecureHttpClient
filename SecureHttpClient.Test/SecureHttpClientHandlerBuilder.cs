using Microsoft.Extensions.Logging;
using Serilog;

namespace SecureHttpClient.Test
{
    internal static class SecureHttpClientHandlerBuilder
    {
        public static SecureHttpClientHandler Build()
        {
            var logger = new LoggerFactory().AddSerilog().CreateLogger(nameof(SecureHttpClientHandler));
            var secureHttpClientHandler = new SecureHttpClientHandler(logger);
            return secureHttpClientHandler;
        }
    }
}
