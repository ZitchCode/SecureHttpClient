#if __IOS__

using System.Net.Http;
using System.Threading.Tasks;
using System.Threading;
using Foundation;

namespace SecureHttpClient
{
    internal class InflightOperation
    {
        public HttpRequestMessage Request { get; set; }
        public TaskCompletionSource<HttpResponseMessage> FutureResponse { get; set; }
        public ProgressDelegate Progress { get; set; }
        public ByteArrayListStream ResponseBody { get; set; }
        public CancellationToken CancellationToken { get; set; }
        public bool IsCompleted { get; set; }
        public NSError Error { get; set; }
    }
}

#endif