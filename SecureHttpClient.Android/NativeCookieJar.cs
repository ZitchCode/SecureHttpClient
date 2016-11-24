using System.Collections.Generic;
using System.Linq;
using Java.Net;
using Square.OkHttp3;

namespace SecureHttpClient
{
    internal class NativeCookieJar : Java.Lang.Object, ICookieJar
    {
        private readonly CookieManager _cookieManager;

        public NativeCookieJar()
        {
            _cookieManager = new CookieManager();
        }

        public IList<Cookie> LoadForRequest(HttpUrl httpUrl)
        {
            // No domain matching done here, because OkHttp.Cookie's "hostonly" does not exist in Java.Net.HttpCookie
            return _cookieManager.CookieStore.Cookies.Select(ToNetCookie).ToList();
        }

        public void SaveFromResponse(HttpUrl url, IList<Cookie> cookies)
        {
            foreach (var cookie in cookies.Select(ToNativeCookie))
            {
                if (cookie.Discard)
                {
                    _cookieManager.CookieStore.Remove(new URI(cookie.Domain), cookie);
                }
                else
                {
                    _cookieManager.CookieStore.Add(new URI(cookie.Domain), cookie);
                }
            }
        }

        private static HttpCookie ToNativeCookie(Cookie cookie)
        {
            return new HttpCookie(cookie.Name(), cookie.Value())
            {
                Domain = cookie.Domain(),
                Path = cookie.Path(),
                Secure = cookie.Secure(),
                Discard = cookie.ExpiresAt() == long.MinValue
            };
        }

        private static Cookie ToNetCookie(HttpCookie cookie)
        {
            var cookieBuilder = new Cookie.Builder().Name(cookie.Name).Value(cookie.Value).Path(cookie.Path).Domain(cookie.Domain);
            if (cookie.Secure)
            {
                cookieBuilder.Secure();
            }
            return cookieBuilder.Build();
        }
    }
}