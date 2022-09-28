using System.Reflection;
using Microsoft.Maui.Hosting;
using Xunit.Runners.Maui;

namespace SecureHttpClient.TestRunner.Maui
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            return MauiApp
                .CreateBuilder()
                .ConfigureTests(new TestOptions
                {
                    Assemblies =
                    {
                        typeof (Test.SslTest).GetTypeInfo().Assembly
                    }
                })
                .UseVisualRunner()
                .Build();
        }
    }
}