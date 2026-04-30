using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SecureHttpClient.Test.Helpers
{
    // HttpContent that declares a larger ContentLength than what it actually provides.
    // Used to reproduce the OkHttp ProtocolException when WriteTo closes the sink with
    // fewer bytes written than declared (Exchange$RequestBodySink.close checks the delta).
    internal sealed class TruncatedContent : HttpContent
    {
        private readonly byte[] _actualBytes;
        private readonly long _declaredLength;

        internal TruncatedContent(byte[] actualBytes, long declaredLength)
        {
            _actualBytes = actualBytes;
            _declaredLength = declaredLength;
            Headers.ContentLength = declaredLength;
        }

        protected override void SerializeToStream(Stream stream, TransportContext? context, CancellationToken cancellationToken)
            => stream.Write(_actualBytes);

        protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context)
            => stream.WriteAsync(_actualBytes).AsTask();

        protected override bool TryComputeLength(out long length)
        {
            length = _declaredLength;
            return true;
        }

        // ReadAsStream() falls back to CreateContentReadStreamAsync(); returning a short
        // stream here ensures WriteTo copies only _actualBytes.Length bytes into the OkHttp sink.
        protected override Task<Stream> CreateContentReadStreamAsync()
            => Task.FromResult<Stream>(new MemoryStream(_actualBytes));

        protected override Task<Stream> CreateContentReadStreamAsync(CancellationToken cancellationToken)
            => Task.FromResult<Stream>(new MemoryStream(_actualBytes));
    }
}
