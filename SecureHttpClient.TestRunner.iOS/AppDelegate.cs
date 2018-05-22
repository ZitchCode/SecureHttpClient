using Foundation;
using UIKit;
using Serilog;
using Serilog.Core;
using Xunit.Runner;
using Xunit.Sdk;

namespace SecureHttpClient.TestRunner.iOS
{
    [Register(nameof(AppDelegate))]
    public class AppDelegate : RunnerAppDelegate
    {
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.NSLog()
                .Enrich.WithProperty(Constants.SourceContextPropertyName, "TestRunner")
                .CreateLogger();

            AddExecutionAssembly(typeof(ExtensibilityPointFactory).Assembly);
            AddTestAssembly(typeof(Test.SslTest).Assembly);
            AutoStart = true;
            return base.FinishedLaunching(app, options);
        }

        public override void WillTerminate(UIApplication app)
        {
            Log.CloseAndFlush();
            base.WillTerminate(app);
        }
    }
}
