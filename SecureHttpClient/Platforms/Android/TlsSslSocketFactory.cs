#if __ANDROID__

using Android.Runtime;
using Java.Lang;
using Java.Security;
using Javax.Net.Ssl;

namespace SecureHttpClient
{
    internal static class TlsSslSocketFactory
    {
        public static IX509TrustManager GetSystemDefaultTrustManager()
        {
            IX509TrustManager x509TrustManager = null;
            try
            {
                var trustManagerFactory = TrustManagerFactory.GetInstance(TrustManagerFactory.DefaultAlgorithm);
                trustManagerFactory.Init((KeyStore)null);
                foreach (var trustManager in trustManagerFactory.GetTrustManagers())
                {
                    var manager = trustManager.JavaCast<IX509TrustManager>();
                    if (manager != null)
                    {
                        x509TrustManager = manager;
                        break;
                    }
                }
            }
            catch (Exception ex) when (ex is NoSuchAlgorithmException || ex is KeyStoreException)
            {
                // move along...
            }
            return x509TrustManager;
        }
    }
}

#endif