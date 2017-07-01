using System;
using System.Collections.Generic;
using Java.Lang;
using Java.Net;
using Square.OkHttp3;

namespace SecureHttpClient
{
    internal class NativeCookieJar : Java.Lang.Object, ICookieJar
    {
        private static DateTime _epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        private readonly CookieManager _cookieManager;

        public NativeCookieJar()
        {
            _cookieManager = new CookieManager();
        }

        public IList<Cookie> LoadForRequest(HttpUrl httpUrl)
        {
            var headers = new Dictionary<string, IList<string>>();
            var cookieHeaders = _cookieManager.Get(httpUrl.Uri(), headers);
            var cookies = new List<Cookie>();
            if (cookieHeaders != null)
            {
                foreach (var entry in cookieHeaders)
                {
                    if ((entry.Key == "Cookie" || entry.Key == "Cookie2") && entry.Value != null)
                    {
                        foreach (var header in entry.Value)
                        {
                            var newCookies = DecodeHeader(httpUrl, header);
                            cookies.AddRange(newCookies);
                        }
                    }
                }
            }
            return cookies;
        }

        public void SaveFromResponse(HttpUrl url, IList<Cookie> cookies)
        {
            var cookieStrings = new Android.Runtime.JavaList<string>();
            foreach (var cookie in cookies)
            {
                cookieStrings.Add(CookieToString(cookie));
            }
            var map = new Android.Runtime.JavaDictionary<string, IList<string>> { { "Set-Cookie", cookieStrings } };
            _cookieManager.Put(url.Uri(), map);
        }

        private static string CookieToString(Cookie cookie)
        {
            var result = new StringBuilder();

            result.Append(cookie.Name());
            result.Append('=');
            result.Append(cookie.Value());

            if (cookie.Persistent())
            {
                var date = FromUnixTime(cookie.ExpiresAt()).ToUniversalTime().ToString("r");
                result.Append("; expires=").Append(date);
            }

            if (!cookie.HostOnly())
            {
                result.Append("; domain=");
                result.Append(".");
                result.Append(cookie.Domain());
            }

            result.Append("; path=").Append(cookie.Path());

            if (cookie.Secure())
            {
                result.Append("; secure");
            }

            if (cookie.HttpOnly())
            {
                result.Append("; httponly");
            }

            return result.ToString();
        }

        private static DateTime FromUnixTime(long unixTimeMillis)
        {
            return unixTimeMillis == Long.MinValue ? _epoch : _epoch.AddMilliseconds(unixTimeMillis);
        }

        private static IEnumerable<Cookie> DecodeHeader(HttpUrl httpUrl, string header)
        {
            var result = new List<Cookie>();
            var limit = header.Length;
            int pairEnd;
            for (var pos = 0; pos < limit; pos = pairEnd + 1)
            {
                pairEnd = DelimiterOffset(header, pos, limit, ";,");
                var equalsSign = DelimiterOffset(header, pos, pairEnd, "=");
                var name = TrimSubstring(header, pos, equalsSign);
                if (name.StartsWith("$"))
                {
                    continue;
                }

                // We have either name=value or just a name.
                var value = equalsSign < pairEnd ? TrimSubstring(header, equalsSign + 1, pairEnd) : "";

                result.Add(new Cookie.Builder().Name(name).Value(value).Domain(httpUrl.Host()).Build());
            }
            return result;
        }

        private static int DelimiterOffset(string input, int pos, int limit, string delimiters)
        {
            for (var i = pos; i < limit; i++)
            {
                if (delimiters.IndexOf(input[i]) != -1)
                {
                    return i;
                }
            }
            return limit;
        }

        private static string TrimSubstring(string str, int pos, int limit)
        {
            var start = SkipLeadingAsciiWhitespace(str, pos, limit);
            var end = SkipTrailingAsciiWhitespace(str, start, limit);
            return str.Substring(start, end - start);
        }

        private static int SkipLeadingAsciiWhitespace(string input, int pos, int limit)
        {
            for (var i = pos; i < limit; ++i)
            {
                switch (input[i])
                {
                    case '\t':
                    case '\n':
                    case '\f':
                    case '\r':
                    case ' ':
                        continue;
                    default:
                        return i;
                }
            }
            return limit;
        }

        private static int SkipTrailingAsciiWhitespace(string input, int pos, int limit)
        {
            for (var i = limit - 1; i >= pos; --i)
            {
                switch (input[i])
                {
                    case '\t':
                    case '\n':
                    case '\f':
                    case '\r':
                    case ' ':
                        continue;
                    default:
                        return i + 1;
                }
            }
            return pos;
        }
    }
}