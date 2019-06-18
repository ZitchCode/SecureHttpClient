using System;
using System.Collections.Generic;
using System.Threading;
using System.Reflection;
using Serilog;
using Serilog.Core;
using Xunit.Runners;

namespace SecureHttpClient.TestRunner.Net
{
    internal class Program
    {
        // Use an event to know when we're done
        private static ManualResetEvent _finished;

        // Start out assuming success; we'll set this to 1 if we get a failed test
        private static int _result;

        private static int Main(string[] args)
        {
            // Init logger
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{SourceContext:l}] {Message}{NewLine}{Exception}")
                .Enrich.WithProperty(Constants.SourceContextPropertyName, "TestRunner")
                .CreateLogger();

            // Tests to run
            var testAssemblies = new List<Assembly>
            {
                typeof (Test.SslTest).GetTypeInfo().Assembly
            };

            // Loop on test assemblies
            foreach (var testAssembly in testAssemblies)
            {
                RunTests(testAssembly);
            }

            Log.CloseAndFlush();

            return _result;
        }

        private static void RunTests(Assembly testAssembly)
        {
            _finished = new ManualResetEvent(false);

            // Run tests
            using (var runner = AssemblyRunner.WithoutAppDomain(testAssembly.Location))
            {
                runner.OnDiscoveryComplete = OnDiscoveryComplete;
                runner.OnExecutionComplete = OnExecutionComplete;
                runner.OnTestFailed = OnTestFailed;
                runner.OnTestPassed = OnTestPassed;
                runner.OnTestSkipped = OnTestSkipped;

                Log.Debug($"Processing {testAssembly.FullName}...");
                runner.Start();

                _finished.WaitOne();
                _finished.Dispose();
            }
        }

        private static void OnDiscoveryComplete(DiscoveryCompleteInfo info)
        {
            Log.Debug($"Running {info.TestCasesToRun} of {info.TestCasesDiscovered} tests...");
        }

        private static void OnExecutionComplete(ExecutionCompleteInfo info)
        {
            Log.Information($"Finished: {info.TotalTests} tests in {Math.Round(info.ExecutionTime, 3)}s ({info.TestsFailed} failed, {info.TestsSkipped} skipped)");
            _finished.Set();
        }

        private static void OnTestFailed(TestFailedInfo info)
        {
            Log.Error($"[FAIL] {info.TestDisplayName}: {info.ExceptionMessage}");
            if (info.ExceptionStackTrace != null)
            {
                Log.Error(info.ExceptionStackTrace);
            }
            _result = 1;
        }

        private static void OnTestPassed(TestPassedInfo info)
        {
            Log.Information($"[PASS] {info.TestDisplayName}");
        }

        private static void OnTestSkipped(TestSkippedInfo info)
        {
            Log.Information($"[SKIP] {info.TestDisplayName}: {info.SkipReason}");
        }
    }
}
