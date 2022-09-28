using Foundation;
using Microsoft.Maui;
using Microsoft.Maui.Hosting;
using Serilog;
using Serilog.Core;

namespace SecureHttpClient.TestRunner.Maui
{
    [Register("AppDelegate")]
    public class AppDelegate : MauiUIApplicationDelegate
    {
        protected override MauiApp CreateMauiApp()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.NSLog()
                .Enrich.WithProperty(Constants.SourceContextPropertyName, "TestRunner")
                .CreateLogger();

            return MauiProgram.CreateMauiApp();
        }
    }
}