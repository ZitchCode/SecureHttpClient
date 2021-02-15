#if __IOS__

using System;
using System.Threading.Tasks;
using System.Threading;

namespace SecureHttpClient
{
    // Straight-up thieved from http://www.hanselman.com/blog/ComparingTwoTechniquesInNETAsynchronousCoordinationPrimitives.aspx 
    internal sealed class AsyncLock
    {
        private readonly SemaphoreSlim _semaphore;
        private readonly Task<IDisposable> _releaser;

        public static AsyncLock CreateLocked(out IDisposable releaser)
        {
            var asyncLock = new AsyncLock(true);
            releaser = asyncLock._releaser.Result;
            return asyncLock;
        }

        private AsyncLock(bool isLocked)
        {
            _semaphore = new SemaphoreSlim(isLocked ? 0 : 1, 1);
            _releaser = Task.FromResult((IDisposable)new Releaser(this));
        }

        public Task<IDisposable> LockAsync()
        {
            var wait = _semaphore.WaitAsync();
            return wait.IsCompleted ?
                _releaser :
                wait.ContinueWith((_, state) => (IDisposable)state,
                    _releaser.Result, CancellationToken.None,
                    TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
        }

        private sealed class Releaser : IDisposable
        {
            readonly AsyncLock _toRelease;
            internal Releaser(AsyncLock toRelease) { _toRelease = toRelease; }
            public void Dispose() { _toRelease._semaphore.Release(); }
        }
    }
}

#endif
