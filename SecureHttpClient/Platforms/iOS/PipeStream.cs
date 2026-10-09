#if __IOS__

using System;
using System.IO;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;

namespace SecureHttpClient
{
    /// <summary>
    /// Read-only <see cref="Stream"/> fed by the NSUrlSession delegate callbacks.
    /// Data written by the producer is buffered in a <see cref="Pipe"/> so it never blocks a
    /// slow consumer, and readers can await data asynchronously until the response completes
    /// or faults.
    /// </summary>
    internal sealed class PipeStream : Stream
    {
        private readonly Pipe _pipe = new();
        private readonly Stream _readStream;
        private readonly object _sync = new();
        private bool _completed;

        public PipeStream()
        {
            _readStream = _pipe.Reader.AsStream(leaveOpen: false);
        }

        // Producer side (NSUrlSession delegate callbacks)
        public void AddByteArray(byte[] bytes)
        {
            if (bytes.Length == 0)
            {
                return;
            }

            lock (_sync)
            {
                if (_completed)
                {
                    return;
                }

                var span = _pipe.Writer.GetSpan(bytes.Length);
                bytes.CopyTo(span);
                _pipe.Writer.Advance(bytes.Length);

                // Signal the reader without blocking the delegate thread. The returned ValueTask must
                // NOT be awaited: the pipe reuses its writer ValueTask source, so awaiting an earlier
                // flush after a newer one has started triggers a "concurrent operations" fault.
                _ = _pipe.Writer.FlushAsync();
            }
        }

        public void Complete()
        {
            lock (_sync)
            {
                if (_completed)
                {
                    return;
                }
                _completed = true;
                _pipe.Writer.Complete();
            }
        }

        public void SetException(Exception exception)
        {
            lock (_sync)
            {
                if (_completed)
                {
                    return;
                }
                _completed = true;
                _pipe.Writer.Complete(exception);
            }
        }

        // Consumer side (Stream)
        public override bool CanRead => _readStream.CanRead;
        public override bool CanSeek => _readStream.CanSeek;
        public override bool CanWrite => _readStream.CanWrite;
        public override bool CanTimeout => _readStream.CanTimeout;
        public override long Length => _readStream.Length;
        public override long Position
        {
            get => _readStream.Position;
            set => _readStream.Position = value;
        }

        public override void Flush() => _readStream.Flush();
        public override long Seek(long offset, SeekOrigin origin) => _readStream.Seek(offset, origin);
        public override void SetLength(long value) => _readStream.SetLength(value);
        public override int Read(byte[] buffer, int offset, int count) => _readStream.Read(buffer, offset, count);
        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            => _readStream.ReadAsync(buffer, offset, count, cancellationToken);
        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
            => _readStream.ReadAsync(buffer, cancellationToken);
        public override void Write(byte[] buffer, int offset, int count) => _readStream.Write(buffer, offset, count);

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _readStream.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}

#endif
