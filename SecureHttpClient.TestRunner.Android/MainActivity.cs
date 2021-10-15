using Android.App;
using Android.OS;
using Android.Runtime;
using Serilog;
using Serilog.Core;
using Xunit;
using Xunit.Runners.UI;

namespace SecureHttpClient.TestRunner.Android
{
    [Activity(Label = "SecureHttpClient.TestRunner.Android", MainLauncher = true, Theme = "@android:style/Theme.Material.Light")]
    public class MainActivity : RunnerActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.AndroidLog()
                .Enrich.WithProperty(Constants.SourceContextPropertyName, "TestRunner")
                .CreateLogger();

            AndroidEnvironment.UnhandledExceptionRaiser += OnAndroidEnvironmentUnhandledExceptionRaiser;

            AddTestAssembly(typeof(Test.HttpTest).Assembly);
            AutoStart = true;
            base.OnCreate(bundle);
        }

        private static void OnAndroidEnvironmentUnhandledExceptionRaiser(object sender, RaiseThrowableEventArgs e)
        {
            Log.Fatal(e.Exception, "AndroidEnvironment.UnhandledExceptionRaiser");
            Assert.True(false, "AndroidEnvironment.UnhandledExceptionRaiser");
        }

        protected override void OnStop()
        {
            Log.CloseAndFlush();
            base.OnStop();
        }
    }
}

