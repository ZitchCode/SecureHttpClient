#if __IOS__

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SecureHttpClient
{
    internal class CancellableStreamContent : ProgressStreamContent
    {
        private Action _onDispose;

        public CancellableStreamContent(Stream source, Action onDispose) : base(source, CancellationToken.None)
        {
            _onDispose = onDispose;
        }

        protected override void Dispose(bool disposing)
        {
            var disp = Interlocked.Exchange(ref _onDispose, null);
            disp?.Invoke();

            // EVIL HAX: We have to let at least one ReadAsync of the underlying
            // stream fail with OperationCancelledException before we can dispose
            // the base, or else the exception coming out of the ReadAsync will
            // be an ObjectDisposedException from an internal MemoryStream. This isn't
            // the Ideal way to fix this, but #yolo.
            Task.Run(() => base.Dispose(disposing));
        }
    }
}

#endif
