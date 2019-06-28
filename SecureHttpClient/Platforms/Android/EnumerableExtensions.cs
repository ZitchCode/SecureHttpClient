#if __ANDROID__

using System;
using System.Collections.Generic;

namespace SecureHttpClient
{
    internal static class EnumerableExtensions
    {
        public static void ForEach<T>(this IEnumerable<T> enumeration, Action<T> action)
        {
            foreach (var item in enumeration)
            {
                action(item);
            }
        }
    }
}

#endif