#if __IOS__

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SecureHttpClient
{
    internal class ByteArrayListStream : Stream
    {
        private Exception _exception;
        private IDisposable _lockRelease;
        private readonly AsyncLock _readStreamLock;
        private readonly List<byte[]> _bytes = new ();

        private bool _isCompleted;
        private long _maxLength;
        private long _position;
        private int _offsetInCurrentBuffer;

        public ByteArrayListStream()
        {
            // Initially we have nothing to read so Reads should be parked
            _readStreamLock = AsyncLock.CreateLocked(out _lockRelease);
        }

        public override bool CanRead => true;
        public override bool CanWrite => false;
        public override void Write(byte[] buffer, int offset, int count) { throw new NotSupportedException(); }
        public override void WriteByte(byte value) { throw new NotSupportedException(); }
        public override bool CanSeek => false;
        public override bool CanTimeout => false;
        public override void SetLength(long value) { throw new NotSupportedException(); }
        public override void Flush() { }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override long Position
        {
            get => _position;
            set => throw new NotSupportedException();
        }

        public override long Length => _maxLength;

        public override int Read(byte[] buffer, int offset, int count)
        {
            return ReadAsync(buffer, offset, count).Result;
        }

        /* OMG THIS CODE IS COMPLICATED
         *
         * Here's the core idea. We want to create a ReadAsync function that
         * reads from our list of byte arrays **until it gets to the end of
         * our current list**.
         *
         * If we're not there yet, we keep returning data, serializing access
         * to the underlying position pointer (i.e. we definitely don't want
         * people concurrently moving position along). If we try to read past
         * the end, we return the section of data we could read and complete
         * it.
         *
         * Here's where the tricky part comes in. If we're not Completed (i.e.
         * the caller still wants to add more byte arrays in the future) and
         * we're at the end of the current stream, we want to *block* the read
         * (not blocking, but async blocking whatever you know what I mean),
         * until somebody adds another byte[] to chew through, or if someone
         * rewinds the position.
         *
         * If we *are* completed, we should return zero to simply complete the
         * read, signalling we're at the end of the stream */
        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            retry:
            var bytesRead = 0;
            var buffersToRemove = 0;

            if (_isCompleted && _position == _maxLength)
            {
                return 0;
            }

            if (_exception != null) throw _exception;

            using (await _readStreamLock.LockAsync().ConfigureAwait(false))
            {
                lock (_bytes)
                {
                    foreach (var buf in _bytes)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        if (_exception != null) throw _exception;

                        var toCopy = Math.Min(count, buf.Length - _offsetInCurrentBuffer);
                        Array.ConstrainedCopy(buf, _offsetInCurrentBuffer, buffer, offset, toCopy);

                        count -= toCopy;
                        offset += toCopy;
                        bytesRead += toCopy;

                        _offsetInCurrentBuffer += toCopy;

                        if (_offsetInCurrentBuffer >= buf.Length)
                        {
                            _offsetInCurrentBuffer = 0;
                            buffersToRemove++;
                        }

                        if (count <= 0) break;
                    }

                    // Remove buffers that we read in this operation
                    _bytes.RemoveRange(0, buffersToRemove);

                    _position += bytesRead;
                }
            }

            // If we're at the end of the stream and it's not done, prepare
            // the next read to park itself unless AddByteArray or Complete 
            // posts
            if (_position >= _maxLength && !_isCompleted)
            {
                _lockRelease = await _readStreamLock.LockAsync().ConfigureAwait(false);
            }

            if (bytesRead == 0 && !_isCompleted)
            {
                // NB: There are certain race conditions where we somehow acquire
                // the lock yet are at the end of the stream, and we're not completed
                // yet. We should try again so that we can get stuck in the lock.
                goto retry;
            }

            if (cancellationToken.IsCancellationRequested)
            {
                Interlocked.Exchange(ref _lockRelease, EmptyDisposable.Instance).Dispose();
                cancellationToken.ThrowIfCancellationRequested();
            }

            if (_exception != null)
            {
                Interlocked.Exchange(ref _lockRelease, EmptyDisposable.Instance).Dispose();
                throw _exception;
            }

            if (_isCompleted && _position < _maxLength)
            {
                // NB: This solves a rare deadlock 
                //
                // 1. ReadAsync called (waiting for lock release)
                // 2. AddByteArray called (release lock)
                // 3. AddByteArray called (release lock)
                // 4. Complete called (release lock the last time)
                // 5. ReadAsync called (lock released at this point, the method completed successfully) 
                // 6. ReadAsync called (deadlock on LockAsync(), because the lock is block, and there is no way to release it)
                // 
                // Current condition forces the lock to be released in the end of 5th point

                Interlocked.Exchange(ref _lockRelease, EmptyDisposable.Instance).Dispose();
            }

            return bytesRead;
        }

        public void AddByteArray(byte[] arrayToAdd)
        {
            if (_exception != null) throw _exception;
            if (_isCompleted) throw new InvalidOperationException("Can't add byte arrays once Complete() is called");

            lock (_bytes)
            {
                _maxLength += arrayToAdd.Length;
                _bytes.Add(arrayToAdd);
            }

            Interlocked.Exchange(ref _lockRelease, EmptyDisposable.Instance).Dispose();
        }

        public void Complete()
        {
            _isCompleted = true;
            Interlocked.Exchange(ref _lockRelease, EmptyDisposable.Instance).Dispose();
        }

        public void SetException(Exception ex)
        {
            _exception = ex;
            Complete();
        }
    }
}

#endif
