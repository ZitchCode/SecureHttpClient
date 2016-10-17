using Foundation;
using UIKit;
using Xunit.Runner;
using Xunit.Sdk;

namespace SecureHttpClient.TestRunner.iOS
{
    [Register(nameof(AppDelegate))]
    public class AppDelegate : RunnerAppDelegate
    {
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            AddExecutionAssembly(typeof(ExtensibilityPointFactory).Assembly);
            AddTestAssembly(typeof(Test.SslTest).Assembly);
            AutoStart = true;
            return base.FinishedLaunching(app, options);
        }
    }
}
