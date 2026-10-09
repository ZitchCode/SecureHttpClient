#if __IOS__

using System;
using System.IO;
using System.Threading;

namespace SecureHttpClient
{
    internal class CancellableStreamContent : ProgressStreamContent
    {
        private Action? _onDispose;

        public CancellableStreamContent(Stream source, Action onDispose) : base(source, CancellationToken.None)
        {
            _onDispose = onDispose;
        }

        protected override void Dispose(bool disposing)
        {
            var disp = Interlocked.Exchange(ref _onDispose, null);
            disp?.Invoke();

            // onDispose completes the response pipe with an OperationCanceledException, so any
            // pending read observes the cancellation before the base stream is disposed.
            base.Dispose(disposing);
        }
    }
}

#endif