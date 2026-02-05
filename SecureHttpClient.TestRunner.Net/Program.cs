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

        private static int _totalTests;
        private static int _testsFailed;
        private static int _testsSkipped;
        private static decimal _executionTime;

        private static string Green(string text) => $"\x1b[32m{text}\x1b[0m";
        private static string Yellow(string text) => $"\x1b[33m{text}\x1b[0m";
        private static string Red(string text) => $"\x1b[31m{text}\x1b[0m";

        private static readonly Lock LogLock = new();

        private static int Main()
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

            Log.Information($"Total: Finished: {_totalTests} tests in {Math.Round(_executionTime, 3)}s ({_testsFailed} failed, {_testsSkipped} skipped)");

            Log.CloseAndFlush();

            return _result;
        }

        private static void RunTests(Assembly testAssembly)
        {
            _finished = new ManualResetEvent(false);

            // Run tests
#pragma warning disable IL3000
            using var runner = AssemblyRunner.WithoutAppDomain(testAssembly.Location);
#pragma warning restore IL3000
            runner.OnDiscoveryComplete = OnDiscoveryComplete;
            runner.OnExecutionComplete = OnExecutionComplete;
            runner.OnTestFailed = OnTestFailed;
            runner.OnTestPassed = OnTestPassed;
            runner.OnTestSkipped = OnTestSkipped;

            Log.Debug($"Processing {testAssembly.FullName}...");
            runner.Start(new AssemblyRunnerStartOptions());

            _finished.WaitOne();
            _finished.Dispose();
        }

        private static void OnDiscoveryComplete(DiscoveryCompleteInfo info)
        {
            Log.Debug($"Running {info.TestCasesToRun} of {info.TestCasesDiscovered} tests...");
        }

        private static void OnExecutionComplete(ExecutionCompleteInfo info)
        {
            Log.Information($"Finished: {info.TotalTests} tests in {Math.Round(info.ExecutionTime, 3)}s ({info.TestsFailed} failed, {info.TestsSkipped} skipped)");

            _totalTests += info.TotalTests;
            _testsFailed += info.TestsFailed;
            _testsSkipped += info.TestsSkipped;
            _executionTime += info.ExecutionTime;

            _finished.Set();
        }

        private static void OnTestFailed(TestFailedInfo info)
        {
            lock (LogLock)
            {
                Log.Error($"{Red("[FAIL]")} {info.TestDisplayName}: {info.ExceptionMessage}");
                if (info.ExceptionStackTrace != null)
                {
                    Log.Error(info.ExceptionStackTrace);
                }
            }
            _result = 1;
        }

        private static void OnTestPassed(TestPassedInfo info)
        {
            lock (LogLock)
            {
                Log.Information($"{Green("[PASS]")} {info.TestDisplayName}");
            }
        }

        private static void OnTestSkipped(TestSkippedInfo info)
        {
            lock (LogLock)
            {
                Log.Information($"{Yellow("[SKIP]")} {info.TestDisplayName}: {info.SkipReason}");
            }
        }
    }
}