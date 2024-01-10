using System;
using Android.App;
using Android.Runtime;
using Microsoft.Maui;
using Microsoft.Maui.Hosting;
using Serilog;
using Serilog.Core;
using Xunit;

namespace SecureHttpClient.TestRunner.Maui
{
    [Application]
    public class MainApplication : MauiApplication
    {
        public MainApplication(IntPtr handle, JniHandleOwnership ownership)
            : base(handle, ownership)
        {
        }

        protected override MauiApp CreateMauiApp()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.AndroidLog()
                .Enrich.WithProperty(Constants.SourceContextPropertyName, "TestRunner")
                .CreateLogger();

            AndroidEnvironment.UnhandledExceptionRaiser += OnAndroidEnvironmentUnhandledExceptionRaiser;

            return MauiProgram.CreateMauiApp();
        }

        private static void OnAndroidEnvironmentUnhandledExceptionRaiser(object sender, RaiseThrowableEventArgs e)
        {
            Log.Fatal(e.Exception, "AndroidEnvironment.UnhandledExceptionRaiser");
            Assert.Fail("AndroidEnvironment.UnhandledExceptionRaiser");
        }
    }
}