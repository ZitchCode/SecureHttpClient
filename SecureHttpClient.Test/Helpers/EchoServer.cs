using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SecureHttpClient.Test.Helpers
{
    // Minimal in-process TCP server that accepts a single HTTP/1.1 connection.
    // Captures request header names in wire order so tests do not depend on
    // any external echo service.
    internal sealed class EchoServer : IAsyncDisposable
    {
        private readonly TcpListener _listener;

        internal int Port { get; }

        internal EchoServer()
        {
            _listener = new TcpListener(IPAddress.Loopback, 0);
            _listener.Start();
            Port = ((IPEndPoint)_listener.LocalEndpoint).Port;
        }

        // Accepts one connection, reads request headers in wire order, sends a
        // 200 OK response, and returns the header names as a list.
        internal async Task<List<string>> AcceptAndGetHeaderNamesAsync()
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            using var tcpClient = await _listener.AcceptTcpClientAsync(cts.Token);
            var stream = tcpClient.GetStream();
            using var reader = new StreamReader(stream, Encoding.ASCII,
                detectEncodingFromByteOrderMarks: false, bufferSize: 4096, leaveOpen: true);

            // Skip the request line (e.g. "GET / HTTP/1.1")
            await reader.ReadLineAsync(cts.Token);

            var names = new List<string>();
            string? line;
            while (!string.IsNullOrEmpty(line = await reader.ReadLineAsync(cts.Token)))
            {
                var colon = line.IndexOf(':');
                if (colon > 0)
                    names.Add(line[..colon].Trim());
            }

            const string response = "HTTP/1.1 200 OK\r\nContent-Length: 0\r\nConnection: close\r\n\r\n";
            await stream.WriteAsync(Encoding.ASCII.GetBytes(response), cts.Token);
            await stream.FlushAsync(cts.Token);

            return names;
        }

        // Accepts one connection, drains the request, then responds with the given
        // headers plus a Content-Length and Connection: close. Used to assert where
        // response headers end up on the client side.
        internal Task AcceptAndRespondWithHeadersAsync(IEnumerable<KeyValuePair<string, string>> responseHeaders, string body = "")
            => AcceptAndRespondWithBytesAsync(responseHeaders, Encoding.UTF8.GetBytes(body));

        // Same as above but with a raw body, used to send pre-compressed payloads.
        internal async Task AcceptAndRespondWithBytesAsync(IEnumerable<KeyValuePair<string, string>> responseHeaders, byte[] body)
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            using var tcpClient = await _listener.AcceptTcpClientAsync(cts.Token);
            var stream = tcpClient.GetStream();
            using var reader = new StreamReader(stream, Encoding.ASCII,
                detectEncodingFromByteOrderMarks: false, bufferSize: 4096, leaveOpen: true);

            // Skip the request line and the request headers
            await reader.ReadLineAsync(cts.Token);
            string? line;
            while (!string.IsNullOrEmpty(line = await reader.ReadLineAsync(cts.Token))) { }

            var builder = new StringBuilder("HTTP/1.1 200 OK\r\n");
            foreach (var (name, value) in responseHeaders)
            {
                builder.Append(name).Append(": ").Append(value).Append("\r\n");
            }
            builder.Append("Content-Length: ").Append(body.Length).Append("\r\n");
            builder.Append("Connection: close\r\n\r\n");

            await stream.WriteAsync(Encoding.ASCII.GetBytes(builder.ToString()), cts.Token);
            if (body.Length > 0)
            {
                await stream.WriteAsync(body, cts.Token);
            }
            await stream.FlushAsync(cts.Token);
        }

        // Accepts one connection and sends a response with a partial body while declaring a much
        // larger Content-Length, then waits for the client to close the connection. Used to assert
        // that the client returns as soon as the headers are available instead of buffering the body.
        internal async Task AcceptAndSendPartialResponseThenWaitAsync(IEnumerable<KeyValuePair<string, string>> responseHeaders, byte[] partialBody, long declaredContentLength)
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            using var tcpClient = await _listener.AcceptTcpClientAsync(cts.Token);
            var stream = tcpClient.GetStream();
            using var reader = new StreamReader(stream, Encoding.ASCII,
                detectEncodingFromByteOrderMarks: false, bufferSize: 4096, leaveOpen: true);

            // Skip the request line and the request headers
            await reader.ReadLineAsync(cts.Token);
            string? line;
            while (!string.IsNullOrEmpty(line = await reader.ReadLineAsync(cts.Token))) { }

            var builder = new StringBuilder("HTTP/1.1 200 OK\r\n");
            foreach (var (name, value) in responseHeaders)
            {
                builder.Append(name).Append(": ").Append(value).Append("\r\n");
            }
            builder.Append("Content-Length: ").Append(declaredContentLength).Append("\r\n");
            builder.Append("Connection: close\r\n\r\n");

            await stream.WriteAsync(Encoding.ASCII.GetBytes(builder.ToString()), cts.Token);
            await stream.WriteAsync(partialBody, cts.Token);
            await stream.FlushAsync(cts.Token);

            // Park until the client disposes the response (closing the connection) or the timeout elapses.
            var buffer = new byte[1024];
            try
            {
                while (await stream.ReadAsync(buffer, cts.Token) > 0) { }
            }
            catch (Exception) { /* connection reset or cancellation — expected */ }
        }

        // Accepts one connection, reads the request, signals the caller that the request was
        // received, then waits until the client closes the connection. Completes normally on
        // disconnect and throws OperationCanceledException if the timeout elapses first.
        internal async Task AcceptAndWaitForClientDisconnectAsync(TaskCompletionSource requestReceived, TimeSpan timeout)
        {
            using var cts = new CancellationTokenSource(timeout);
            using var tcpClient = await _listener.AcceptTcpClientAsync(cts.Token);
            var stream = tcpClient.GetStream();
            using var reader = new StreamReader(stream, Encoding.ASCII,
                detectEncodingFromByteOrderMarks: false, bufferSize: 4096, leaveOpen: true);

            // Skip the request line and the request headers
            await reader.ReadLineAsync(cts.Token);
            string? line;
            while (!string.IsNullOrEmpty(line = await reader.ReadLineAsync(cts.Token))) { }

            requestReceived.TrySetResult();

            // Wait for the client to close the connection.
            var buffer = new byte[1024];
            while (await stream.ReadAsync(buffer, cts.Token) > 0) { }
        }

        // Accepts one connection and drains all incoming data without sending a
        // response. Used for tests that expect a client-side exception raised
        // before the response is needed.
        internal async Task AcceptAndDrainAsync()
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            try
            {
                using var tcpClient = await _listener.AcceptTcpClientAsync(cts.Token);
                var stream = tcpClient.GetStream();
                var buffer = new byte[8192];
                try
                {
                    while (await stream.ReadAsync(buffer, cts.Token) > 0) { }
                }
                catch (Exception) { /* connection reset or cancellation — expected */ }
            }
            catch (OperationCanceledException) { }
        }

        public ValueTask DisposeAsync()
        {
            _listener.Stop();
            return ValueTask.CompletedTask;
        }
    }
}
