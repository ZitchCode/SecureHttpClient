using Serilog;
using Serilog.Core;
using Xunit.Runners.UI;

namespace SecureHttpClient.TestRunner.Uwp
{
    sealed partial class App : RunnerApplication
    {
        protected override void OnInitializeRunner()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Debug(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{SourceContext:l}] {Message}{NewLine}{Exception}")
                .Enrich.WithProperty(Constants.SourceContextPropertyName, "TestRunner")
                .CreateLogger();

            AddTestAssembly(typeof(Test.SslTest).Assembly);
            AutoStart = true;
        }
    }
}