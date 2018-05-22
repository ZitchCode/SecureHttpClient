using Android.App;
using Android.OS;
using Serilog;
using Serilog.Core;
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

            AddTestAssembly(typeof(Test.SslTest).Assembly);
            AutoStart = true;
            base.OnCreate(bundle);
        }

        protected override void OnStop()
        {
            Log.CloseAndFlush();
            base.OnStop();
        }
    }
}

