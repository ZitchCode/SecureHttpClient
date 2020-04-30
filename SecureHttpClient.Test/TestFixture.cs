using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace SecureHttpClient.Test
{
    public class TestFixture
    {
        public IServiceProvider ServiceProvider { get; }

        public TestFixture()
        {
            ServiceProvider = new ServiceCollection()
                .AddTransient<HttpClientHandler, SecureHttpClientHandler>()
                .AddLogging(configure => configure.AddSerilog())
                .BuildServiceProvider();
        }
    }
}
