using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Maui.Devices;
using SecureHttpClient.Extensions;
using SecureHttpClient.Test.Helpers;
using Xunit;

namespace SecureHttpClient.Test
{
    public class HttpTest : TestBase, IClassFixture<TestFixture>
    {
        public HttpTest(TestFixture testFixture) : base(testFixture)
        {
        }

        [Fact]
        public async Task HttpTest_Get()
        {
            const string page = @"https://httpbingo.org/get";
            var result = await GetAsync(page).ReceiveString();
            var json = JsonDocument.Parse(result);
            var url = json.RootElement.GetProperty("url").GetString();
            Assert.Equal(page, url);
        }

        [Fact]
        public async Task HttpTest_Gzip()
        {
            const string page = @"https://httpbingo.org/gzip";
            var result = await GetAsync(page).ReceiveString();
            var json = JsonDocument.Parse(result);
            var url = json.RootElement.GetProperty("gzipped").GetBoolean();
            Assert.True(url);
        }

        [Fact]
        public async Task HttpTest_Gzip_WithRequestHeader()
        {
            const string page = @"https://httpbingo.org/gzip";
            var req = new HttpRequestMessage(HttpMethod.Get, page);
            req.Headers.Add("Accept-Encoding", "gzip");
            var result = await SendAsync(req).ReceiveString();
            var json = JsonDocument.Parse(result);
            var url = json.RootElement.GetProperty("gzipped").GetBoolean();
            Assert.True(url);
        }

        [Fact]
        public async Task HttpTest_Headers()
        {
            const string page = @"https://postman-echo.com/get";
            var req = new HttpRequestMessage(HttpMethod.Get, page);
            req.Headers.Add("header1", "value1");
            req.Headers.Add("header2", "value2");
            req.Headers.Add("header3", "value3");
            var result = await SendAsync(req).ReceiveString();
            var json = JsonDocument.Parse(result);
            var headers = json.RootElement.GetProperty("headers").Deserialize<Dictionary<string, string>>().Select(kv => kv.Key).ToList();
            Assert.Contains("header1", headers);
            Assert.Contains("header2", headers);
            Assert.Contains("header3", headers);
        }

        [SkippableFact]
        public async Task HttpTest_HeadersOrder()
        {
            Skip.If(DeviceInfo.Platform != DevicePlatform.Android, "Only on Android");

            const string page = @"https://postman-echo.com/get";
            var req = new HttpRequestMessage(HttpMethod.Get, page);
            req.Headers.Add("header1", "value1");
            req.Headers.Add("header2", "value2");
            req.Headers.Add("header3", "value3");
            req.SetHeadersOrder("header3", "header2", "header1");
            var result = await SendAsync(req).ReceiveString();
            var json = JsonDocument.Parse(result);
            var headers = json.RootElement.GetProperty("headers").Deserialize<Dictionary<string, string>>().Select(kv => kv.Key).ToList();
            var index1 = headers.IndexOf("header1");
            var index2 = headers.IndexOf("header2");
            var index3 = headers.IndexOf("header3");
            Assert.Equal(1, index2 - index3);
            Assert.Equal(1, index1 - index2);
        }

        [SkippableFact]
        public async Task HttpTest_Deflate()
        {
            const string page = @"https://httpbingo.org/deflate";
            var result = await GetAsync(page).ReceiveString();
            var json = JsonDocument.Parse(result);
            var url = json.RootElement.GetProperty("deflated").GetBoolean();
            Assert.True(url);
        }

        [Fact]
        public async Task HttpTest_Utf8()
        {
            const string page = @"https://httpbingo.org/encoding/utf8";
            var result = await GetAsync(page).ReceiveString();
            Assert.Contains("∮ E⋅da = Q,  n → ∞, ∑ f(i) = ∏ g(i)", result);
        }

        [Fact]
        public async Task HttpTest_Redirect()
        {
            const string page = @"https://httpbingo.org/redirect/5"; // httpbingo replaces httpbin because of issue https://github.com/postmanlabs/httpbin/issues/617
            const string final = @"https://httpbingo.org/get";

            var result = await GetAsync(page).ReceiveString();
            var json = JsonDocument.Parse(result);
            var url = json.RootElement.GetProperty("url").GetString();
            Assert.Equal(final, url);

            var request = new HttpRequestMessage(HttpMethod.Get, page);
            var response = await SendAsync(request);

            Assert.Equal(final, request.RequestUri.AbsoluteUri);
            Assert.Equal(final, response.RequestMessage.RequestUri.AbsoluteUri);
        }

        [Fact]
        public async Task HttpTest_DoNotFollowRedirects()
        {
            const string page = @"https://httpbingo.org/redirect/5"; // httpbingo replaces httpbin because of issue https://github.com/postmanlabs/httpbin/issues/617
            DoNotFollowRedirects();
            var response = await GetAsync(page, false);
            Assert.Equal(HttpStatusCode.Found, response.StatusCode);
            Assert.Equal(page, response.RequestMessage.RequestUri.AbsoluteUri);
        }

        [Fact]
        public async Task HttpTest_Delay()
        {
            const string page = @"https://httpbingo.org/delay/5";
            var result = await GetAsync(page).ReceiveString();
            var json = JsonDocument.Parse(result);
            var url = json.RootElement.GetProperty("url").GetString();
            Assert.Equal(page, url);
        }

        [Fact]
        public async Task HttpTest_Stream()
        {
            const string page = @"https://httpbingo.org/stream/50";
            var result = await GetAsync(page).ReceiveString();
            var nbLines = result.Split('\n').Length - 1;
            Assert.Equal(50, nbLines);
        }

        [Fact]
        public async Task HttpTest_Bytes()
        {
            const string page = @"https://httpbingo.org/bytes/1024";
            var result = await GetAsync(page).ReceiveBytes();
            Assert.Equal(1024, result.Length);
        }

        [Fact]
        public async Task HttpTest_StreamBytes()
        {
            const string page = @"https://httpbingo.org/stream-bytes/1024";
            var result = await GetAsync(page).ReceiveBytes();
            Assert.Equal(1024, result.Length);
        }

        [Fact]
        public async Task HttpTest_SetCookie()
        {
            const string page = @"https://httpbingo.org/cookies/set?k1=v1";
            var result = await GetAsync(page).ReceiveString();
            var json = JsonDocument.Parse(result);
            var cookies = json.RootElement.Deserialize<Dictionary<string, string>>();
            Assert.Contains(new KeyValuePair<string, string>("k1", "v1"), cookies);
        }

        [Fact]
        public async Task HttpTest_SetCookieAgain()
        {
            const string page1 = @"https://httpbingo.org/cookies/set?k1=v1";
            await GetAsync(page1);
            const string page2 = @"https://httpbingo.org/cookies/set?k1=v2";
            var result = await GetAsync(page2).ReceiveString();
            var json = JsonDocument.Parse(result);
            var cookies = json.RootElement.Deserialize<Dictionary<string, string>>();
            Assert.Contains(new KeyValuePair<string, string>("k1", "v2"), cookies);
        }

        [Fact]
        public async Task HttpTest_SetCookies()
        {
            const string cookie1 = "k1=v1; Path=/; expires=Sat, 01-Jan-2050 00:00:00 GMT";
            const string cookie2 = "k2=v2; Path=/; expires=Fri, 01-Jan-2049 00:00:00 GMT";
            var page1 = $@"https://httpbingo.org/response-headers?Set-Cookie={WebUtility.UrlEncode(cookie1)}&Set-Cookie={WebUtility.UrlEncode(cookie2)}";
            var response1 = await GetAsync(page1);
            response1.Headers.TryGetValues("set-cookie", out var respCookies);
            Assert.Equal(new List<string> { cookie1, cookie2 }, respCookies);
            const string page2 = @"https://httpbingo.org/cookies";
            var result = await GetAsync(page2).ReceiveString();
            var json = JsonDocument.Parse(result);
            var cookies = json.RootElement.Deserialize<Dictionary<string, string>>();
            Assert.Contains(new KeyValuePair<string, string>("k1", "v1"), cookies);
            Assert.Contains(new KeyValuePair<string, string>("k2", "v2"), cookies);
        }

        [SkippableFact]
        public async Task HttpTest_DeleteCookie()
        {
            Skip.If(DeviceInfo.Platform == DevicePlatform.Android && DeviceInfo.Version.Major == 7, "Failing on Android 24-25");

            const string page1 = @"https://httpbingo.org/cookies/set?k1=v1";
            await GetAsync(page1);
            const string page2 = @"https://httpbingo.org/cookies/delete?k1";
            var result = await GetAsync(page2).ReceiveString();
            var json = JsonDocument.Parse(result);
            var cookies = json.RootElement.Deserialize<Dictionary<string, string>>();
            Assert.DoesNotContain(new KeyValuePair<string, string>("k1", "v1"), cookies);
        }

        [Fact]
        public async Task HttpTest_DoNotUseCookies()
        {
            const string page = @"https://httpbingo.org/cookies/set?k1=v1";
            DisableCookies();
            var result = await GetAsync(page).ReceiveString();
            var json = JsonDocument.Parse(result);
            var cookies = json.RootElement.Deserialize<Dictionary<string, string>>();
            Assert.Empty(cookies);
        }

        [Fact(Skip = "Page does not exist anymore, needs to be fixed")]
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

        [Fact]
        public async Task HttpTest_Timeout()
        {
            const string page = @"https://httpbingo.org/delay/5";
            SetTimeout(1);
            await Assert.ThrowsAsync<TaskCanceledException>(() => GetAsync(page));
        }

        [Fact]
        public async Task HttpTest_UnknownHost()
        {
            const string page = @"https://nosuchhostisknown/";
            await Assert.ThrowsAsync<HttpRequestException>(() => GetAsync(page));
        }

        [SkippableFact]
        public async Task HttpTest_GetWithNonEmptyRequestBody()
        {
            Skip.If(DeviceInfo.Platform == DevicePlatform.Android || DeviceInfo.Platform == DevicePlatform.iOS, "GET method must not have a body");

            const string page = @"https://httpbin.org/get";
            var request = new HttpRequestMessage(HttpMethod.Get, page)
            {
                Content = new StringContent("test request body")
            };
            var result = await SendAsync(request).ReceiveString();
            var json = JsonDocument.Parse(result);
            var url = json.RootElement.GetProperty("url").GetString();
            Assert.Equal(page, url);
        }

        [Fact]
        public async Task HttpTest_GetWithEmptyRequestBody()
        {
            const string page = @"https://httpbingo.org/get";
            var request = new HttpRequestMessage(HttpMethod.Get, page)
            {
                Content = new StringContent("")
            };
            var result = await SendAsync(request).ReceiveString();
            var json = JsonDocument.Parse(result);
            var url = json.RootElement.GetProperty("url").GetString();
            Assert.Equal(page, url);
        }
    }
}
