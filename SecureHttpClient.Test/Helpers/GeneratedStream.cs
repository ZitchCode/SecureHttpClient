using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SecureHttpClient.Test.Helpers
{
    // Non-seekable stream that lazily produces a fixed number of identical bytes, so tests can
    // upload a large body without allocating it up front.
    internal sealed class GeneratedStream : Stream
    {
        private readonly long _length;
        private readonly byte _value;
        private long _position;

        internal GeneratedStream(long length, byte value)
        {
            _length = length;
            _value = value;
        }

        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => _length;
        public override long Position
        {
            get => _position;
            set => throw new NotSupportedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var toCopy = (int)Math.Min(count, _length - _position);
            for (var i = 0; i < toCopy; i++)
            {
                buffer[offset + i] = _value;
            }
            _position += toCopy;
            return toCopy;
        }

        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            var toCopy = (int)Math.Min(buffer.Length, _length - _position);
            buffer.Span[..toCopy].Fill(_value);
            _position += toCopy;
            return new ValueTask<int>(toCopy);
        }

        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        public override void Flush() { }
    }
}
