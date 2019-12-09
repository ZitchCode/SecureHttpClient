using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Flurl.Http;
using Newtonsoft.Json.Linq;
using Xamarin.Essentials;
using Xunit;

namespace SecureHttpClient.Test
{
    public class HttpTest : TestBase
    {
        [Fact]
        public async Task HttpTest_Get()
        {
            const string page = @"https://httpbin.org/get";
            var result = await GetAsync(page).ReceiveString();
            var json = JToken.Parse(result);
            var url = json["url"].ToString();
            Assert.Equal(page, url);
        }

        [Fact]
        public async Task HttpTest_Gzip()
        {
            const string page = @"https://httpbin.org/gzip";
            var result = await GetAsync(page).ReceiveString();
            var json = JToken.Parse(result);
            var url = json["gzipped"].ToString();
            Assert.Equal("True", url);
        }

        [SkippableFact]
        public async Task HttpTest_Deflate()
        {
            Skip.IfNot(DeviceInfo.Platform == DevicePlatform.iOS, "Failing on Android and .Net");

            const string page = @"https://httpbin.org/deflate";
            var result = await GetAsync(page).ReceiveString();
            var json = JToken.Parse(result);
            var url = json["deflated"].ToString();
            Assert.Equal("True", url);
        }

        [Fact]
        public async Task HttpTest_Utf8()
        {
            const string page = @"https://httpbin.org/encoding/utf8";
            var result = await GetAsync(page).ReceiveString();
            Assert.Contains("∮ E⋅da = Q,  n → ∞, ∑ f(i) = ∏ g(i)", result);
        }

        [Fact]
        public async Task HttpTest_Redirect()
        {
            const string page = @"https://httpbin.org/redirect/5";
            const string final = @"https://httpbin.org/get";

            var result = await GetAsync(page).ReceiveString();
            var json = JToken.Parse(result);
            var url = json["url"].ToString();
            Assert.Equal(final, url);

            var request = new HttpRequestMessage(HttpMethod.Get, page);
            var response = await SendAsync(request);

            Assert.Equal(final, request.RequestUri.AbsoluteUri);
            Assert.Equal(final, response.RequestMessage.RequestUri.AbsoluteUri);
        }

        [Fact]
        public async Task HttpTest_Delay()
        {
            const string page = @"https://httpbin.org/delay/5";
            var result = await GetAsync(page).ReceiveString();
            var json = JToken.Parse(result);
            var url = json["url"].ToString();
            Assert.Equal(page, url);
        }

        [Fact]
        public async Task HttpTest_Stream()
        {
            const string page = @"https://httpbin.org/stream/50";
            var result = await GetAsync(page).ReceiveString();
            var nbLines = result.Split('\n').Length - 1;
            Assert.Equal(50, nbLines);
        }

        [Fact]
        public async Task HttpTest_Bytes()
        {
            const string page = @"https://httpbin.org/bytes/1024";
            var result = await GetAsync(page).ReceiveBytes();
            Assert.Equal(1024, result.Length);
        }

        [Fact]
        public async Task HttpTest_StreamBytes()
        {
            const string page = @"https://httpbin.org/stream-bytes/1024";
            var result = await GetAsync(page).ReceiveBytes();
            Assert.Equal(1024, result.Length);
        }

        [Fact]
        public async Task HttpTest_SetCookie()
        {
            const string page = @"https://httpbin.org/cookies/set?k1=v1";
            var result = await GetAsync(page).ReceiveString();
            var json = JToken.Parse(result);
            var cookies = json["cookies"].ToObject<Dictionary<string, string>>();
            Assert.Contains(new KeyValuePair<string, string>("k1", "v1"), cookies);
        }

        [Fact]
        public async Task HttpTest_SetCookieAgain()
        {
            const string page1 = @"https://httpbin.org/cookies/set?k1=v1";
            await GetAsync(page1);
            const string page2 = @"https://httpbin.org/cookies/set?k1=v2";
            var result = await GetAsync(page2).ReceiveString();
            var json = JToken.Parse(result);
            var cookies = json["cookies"].ToObject<Dictionary<string, string>>();
            Assert.Contains(new KeyValuePair<string, string>("k1", "v2"), cookies);
        }

        [Fact]
        public async Task HttpTest_SetCookies()
        {
            const string cookie1 = "k1=v1; Path=/";
            const string cookie2 = "k2=v2; Path=/";
            var page1 = $@"https://httpbin.org/response-headers?Set-Cookie={WebUtility.UrlEncode(cookie1)}&Set-Cookie={WebUtility.UrlEncode(cookie2)}";
            var response1 = await GetAsync(page1);
            response1.Headers.TryGetValues("set-cookie", out var respCookies);
            respCookies = respCookies?.SelectMany(c => c.Split(',')).Select(c => c.Trim());
            const string page2 = @"https://httpbin.org/cookies";
            var result = await GetAsync(page2).ReceiveString();
            var json = JToken.Parse(result);
            var cookies = json["cookies"].ToObject<Dictionary<string, string>>();
            Assert.Equal(new List<string> { cookie1, cookie2 }, respCookies);
            Assert.Contains(new KeyValuePair<string, string>("k1", "v1"), cookies);
            Assert.Contains(new KeyValuePair<string, string>("k2", "v2"), cookies);
        }

        [SkippableFact]
        public async Task HttpTest_DeleteCookie()
        {
            Skip.If(DeviceInfo.Platform == DevicePlatform.Android && DeviceInfo.Version.Major == 7, "Failing on Android 24-25");

            const string page1 = @"https://httpbin.org/cookies/set?k1=v1";
            await GetAsync(page1);
            const string page2 = @"https://httpbin.org/cookies/delete?k1";
            var result = await GetAsync(page2).ReceiveString();
            var json = JToken.Parse(result);
            var cookies = json["cookies"].ToObject<Dictionary<string, string>>();
            Assert.DoesNotContain(new KeyValuePair<string, string>("k1", "v1"), cookies);
        }

        [Fact]
        public async Task HttpTest_Protocol()
        {
            const string page = @"https://http2.golang.org/reqinfo";
            var request = new HttpRequestMessage(HttpMethod.Get, page);
            if (DeviceInfo.Platform != DevicePlatform.Android && DeviceInfo.Platform != DevicePlatform.iOS)
            {
                request.Version = new Version(2, 0);
            }
            var response = await SendAsync(request).ReceiveString();
            var protocol = response.Split('\n').Single(str => str.StartsWith("Protocol:"));
            Assert.Contains("HTTP/2.0", protocol);
        }
    }
}
