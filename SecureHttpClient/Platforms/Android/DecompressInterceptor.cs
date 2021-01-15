using Java.Util.Zip;
using Microsoft.Extensions.Logging;
using Square.OkHttp3;
using Square.OkIO;

namespace SecureHttpClient
{
    internal class DecompressInterceptor : Java.Lang.Object, IInterceptor
    {
        private readonly ILogger _logger;

        public DecompressInterceptor(ILogger logger = null)
        {
            _logger = logger;
        }

        public Response Intercept(IInterceptorChain chain)
        {
            var response = chain.Proceed(chain.Request());
            return IsCompressed(response) ? Decompress(response) : response;
        }
        
        private static bool IsCompressed(Response response)
        {
            return response.Headers("Content-Encoding") != null 
                   && (response.Headers("Content-Encoding").Contains("gzip") || response.Headers("Content-Encoding").Contains("deflate"))
                   && response.Body() != null;
        }

        private Response Decompress(Response response)
        {
            _logger?.LogDebug("Decompress response");
            var source = response.Headers("Content-Encoding").Contains("gzip")
                ? (ISource) new GzipSource(response.Body().Source()) 
                : (ISource) new InflaterSource(response.Body().Source(), new Inflater());
            var bodyString = OkIO.Buffer(source).ReadUtf8();
            var responseBody = ResponseBody.Create(bodyString, response.Body().ContentType());
            var strippedHeaders = response.Headers().NewBuilder()
                .RemoveAll("Content-Encoding")
                .RemoveAll("Content-Length")
                .Build();
            return response.NewBuilder()
                .Headers(strippedHeaders)
                .Body(responseBody)
                .Message(response.Message())
                .Build();
        }
    }
}
