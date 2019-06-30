using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Xunit;

namespace SecureHttpClient.Test
{
    public class HttpTest
    {
        [Fact]
        public async Task HttpTest_Get()
        {
            const string page = @"https://httpbin.org/get";
            var result = await GetPageAsync(page).ConfigureAwait(false);
            var json = JToken.Parse(result);
            var url = json["url"].ToString();
            Assert.Equal(page, url);
        }

        [Fact]
        public async Task HttpTest_Gzip()
        {
            const string page = @"https://httpbin.org/gzip";
            var result = await GetPageAsync(page).ConfigureAwait(false);
            var json = JToken.Parse(result);
            var url = json["gzipped"].ToString();
            Assert.Equal("True", url);
        }

        /*
        [Fact]
        public async Task HttpTest_Deflate()
        {
            const string page = @"https://httpbin.org/deflate";
            var result = await GetPageAsync(page).ConfigureAwait(false);
            var json = JToken.Parse(result);
            var url = json["deflated"].ToString();
            Assert.Equal("True", url);
        }
        */

        [Fact]
        public async Task HttpTest_Utf8()
        {
            const string page = @"https://httpbin.org/encoding/utf8";
            var result = await GetPageAsync(page).ConfigureAwait(false);
            Assert.Contains("∮ E⋅da = Q,  n → ∞, ∑ f(i) = ∏ g(i)", result);
        }

        [Fact]
        public async Task HttpTest_Redirect()
        {
            const string page = @"https://httpbin.org/redirect/5";
            const string final = @"https://httpbin.org/get";

            var result = await GetPageAsync(page).ConfigureAwait(false);
            var json = JToken.Parse(result);
            var url = json["url"].ToString();
            Assert.Equal(final, url);

            var request = new HttpRequestMessage(HttpMethod.Get, page);
            var response = await GetResponseAsync(request).ConfigureAwait(false);

            Assert.Equal(final, request.RequestUri.AbsoluteUri);
            Assert.Equal(final, response.RequestMessage.RequestUri.AbsoluteUri);
        }

        [Fact]
        public async Task HttpTest_Delay()
        {
            const string page = @"https://httpbin.org/delay/5";
            var result = await GetPageAsync(page).ConfigureAwait(false);
            var json = JToken.Parse(result);
            var url = json["url"].ToString();
            Assert.Equal(page, url);
        }

        [Fact]
        public async Task HttpTest_Stream()
        {
            const string page = @"https://httpbin.org/stream/50";
            var result = await GetPageAsync(page).ConfigureAwait(false);
            var nbLines = result.Split('\n').Length - 1;
            Assert.Equal(50, nbLines);
        }

        [Fact]
        public async Task HttpTest_Bytes()
        {
            const string page = @"https://httpbin.org/bytes/1024";
            var result = await GetBytesAsync(page).ConfigureAwait(false);
            Assert.Equal(1024, result.Length);
        }

        [Fact]
        public async Task HttpTest_StreamBytes()
        {
            const string page = @"https://httpbin.org/stream-bytes/1024";
            var result = await GetBytesAsync(page).ConfigureAwait(false);
            Assert.Equal(1024, result.Length);
        }

        [Fact]
        public async Task HttpTest_SetCookie()
        {
            const string page = @"https://httpbin.org/cookies/set?k1=v1";
            var result = await GetPageAsync(page).ConfigureAwait(false);
            var json = JToken.Parse(result);
            var cookies = json["cookies"].ToObject<Dictionary<string, string>>();
            Assert.Contains(new KeyValuePair<string, string>("k1", "v1"), cookies);
        }

        [Fact]
        public async Task HttpTest_SetCookieAgain()
        {
            Dictionary<string, string> cookies;
            var secureHttpClientHandler = SecureHttpClientHandlerBuilder.Build();
            using (var httpClient = new HttpClient(secureHttpClientHandler))
            {
                const string page1 = @"https://httpbin.org/cookies/set?k1=v1";
                await GetPageAsync(httpClient, page1).ConfigureAwait(false);
                const string page2 = @"https://httpbin.org/cookies/set?k1=v2";
                var result = await GetPageAsync(httpClient, page2).ConfigureAwait(false);
                var json = JToken.Parse(result);
                cookies = json["cookies"].ToObject<Dictionary<string, string>>();
            }
            Assert.Contains(new KeyValuePair<string, string>("k1", "v2"), cookies);
        }

        [Fact]
        public async Task HttpTest_SetCookies()
        {
            const string cookie1 = "k1=v1; Path=/";
            const string cookie2 = "k2=v2; Path=/";
            Dictionary<string, string> cookies;
            IEnumerable<string> respCookies;
            var secureHttpClientHandler = SecureHttpClientHandlerBuilder.Build();
            using (var httpClient = new HttpClient(secureHttpClientHandler))
            {
                var page1 = $@"https://httpbin.org/response-headers?Set-Cookie={WebUtility.UrlEncode(cookie1)}&Set-Cookie={WebUtility.UrlEncode(cookie2)}";
                var response1 = await httpClient.GetAsync(page1).ConfigureAwait(false);
                response1.Headers.TryGetValues("set-cookie", out respCookies);
                respCookies = respCookies?.SelectMany(c => c.Split(',')).Select(c => c.Trim());
                const string page2 = @"https://httpbin.org/cookies";
                var result = await GetPageAsync(httpClient, page2).ConfigureAwait(false);
                var json = JToken.Parse(result);
                cookies = json["cookies"].ToObject<Dictionary<string, string>>();
            }
            Assert.Equal(new List<string> { cookie1, cookie2 }, respCookies);
            Assert.Contains(new KeyValuePair<string, string>("k1", "v1"), cookies);
            Assert.Contains(new KeyValuePair<string, string>("k2", "v2"), cookies);
        }

        [Fact(Skip = "Failing on Android 24-25")]
        public async Task HttpTest_DeleteCookie()
        {
            Dictionary<string, string> cookies;
            var secureHttpClientHandler = SecureHttpClientHandlerBuilder.Build();
            using (var httpClient = new HttpClient(secureHttpClientHandler))
            {
                const string page1 = @"https://httpbin.org/cookies/set?k1=v1";
                await GetPageAsync(httpClient, page1).ConfigureAwait(false);
                const string page2 = @"https://httpbin.org/cookies/delete?k1";
                var result = await GetPageAsync(httpClient, page2).ConfigureAwait(false);
                var json = JToken.Parse(result);
                cookies = json["cookies"].ToObject<Dictionary<string, string>>();
            }
            Assert.DoesNotContain(new KeyValuePair<string, string>("k1", "v1"), cookies);
        }

        private static async Task<HttpResponseMessage> GetResponseAsync(HttpRequestMessage request)
        {
            HttpResponseMessage response;
            var secureHttpClientHandler = SecureHttpClientHandlerBuilder.Build();
            using (var httpClient = new HttpClient(secureHttpClientHandler))
            {
                response = await httpClient.SendAsync(request).ConfigureAwait(false);
            }
            return response;
        }

        private static async Task<string> GetPageAsync(string page)
        {
            string result;
            var secureHttpClientHandler = SecureHttpClientHandlerBuilder.Build();
            using (var httpClient = new HttpClient(secureHttpClientHandler))
            using (var response = await httpClient.GetAsync(page).ConfigureAwait(false))
            {
                result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            }
            return result;
        }

        private static async Task<string> GetPageAsync(HttpClient httpClient, string page)
        {
            string result;
            using (var response = await httpClient.GetAsync(page).ConfigureAwait(false))
            {
                result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            }
            return result;
        }

        private static async Task<byte[]> GetBytesAsync(string page)
        {
            byte[] result;
            var secureHttpClientHandler = SecureHttpClientHandlerBuilder.Build();
            using (var httpClient = new HttpClient(secureHttpClientHandler))
            using (var response = await httpClient.GetAsync(page).ConfigureAwait(false))
            {
                result = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
            }
            return result;
        }
    }
}
