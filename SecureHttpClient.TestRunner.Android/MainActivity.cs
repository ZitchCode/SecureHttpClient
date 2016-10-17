using Android.App;
using Android.OS;
using Xunit.Runners.UI;

namespace SecureHttpClient.TestRunner.Android
{
    [Activity(Label = "SecureHttpClient.TestRunner.Android", MainLauncher = true, Theme = "@android:style/Theme.Material.Light")]
    public class MainActivity : RunnerActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            AddTestAssembly(typeof(Test.SslTest).Assembly);
            AutoStart = true;
            base.OnCreate(bundle);
        }
    }
}

