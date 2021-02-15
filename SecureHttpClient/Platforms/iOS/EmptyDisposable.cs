#if __IOS__

using System;

namespace SecureHttpClient
{
    internal class EmptyDisposable : IDisposable
    {
        public static IDisposable Instance { get; } = new EmptyDisposable();

        private EmptyDisposable() { }
        public void Dispose() { }
    }
}

#endif
