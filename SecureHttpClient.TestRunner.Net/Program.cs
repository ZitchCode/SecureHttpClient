using System;
using System.Collections.Generic;
using System.Threading;
using System.Reflection;
using Xunit.Runners;

namespace SecureHttpClient.TestRunner.Net
{
    internal class Program
    {
        // We use consoleLock because messages can arrive in parallel, so we want to make sure we get
        // consistent console output.
        private static readonly object ConsoleLock = new object();

        // Use an event to know when we're done
        private static ManualResetEvent _finished;

        // Start out assuming success; we'll set this to 1 if we get a failed test
        private static int _result;

        private static int Main(string[] args)
        {
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

            Console.ReadLine();
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

                Console.WriteLine($"Processing {testAssembly.FullName}...");
                runner.Start();

                _finished.WaitOne();
                _finished.Dispose();
            }
        }

        private static void OnDiscoveryComplete(DiscoveryCompleteInfo info)
        {
            lock (ConsoleLock)
            {
                Console.WriteLine($"Running {info.TestCasesToRun} of {info.TestCasesDiscovered} tests...");
            }
        }

        private static void OnExecutionComplete(ExecutionCompleteInfo info)
        {
            lock (ConsoleLock)
            {
                Console.WriteLine($"Finished: {info.TotalTests} tests in {Math.Round(info.ExecutionTime, 3)}s ({info.TestsFailed} failed, {info.TestsSkipped} skipped)");
            }

            _finished.Set();
        }

        private static void OnTestFailed(TestFailedInfo info)
        {
            lock (ConsoleLock)
            {
                Console.ForegroundColor = ConsoleColor.Red;

                Console.WriteLine($"[FAIL] {info.TestDisplayName}: {info.ExceptionMessage}");
                if (info.ExceptionStackTrace != null)
                    Console.WriteLine(info.ExceptionStackTrace);

                Console.ResetColor();
            }

            _result = 1;
        }

        private static void OnTestPassed(TestPassedInfo info)
        {
            lock (ConsoleLock)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"[PASS] {info.TestDisplayName}");
                Console.ResetColor();
            }
        }

        private static void OnTestSkipped(TestSkippedInfo info)
        {
            lock (ConsoleLock)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"[SKIP] {info.TestDisplayName}: {info.SkipReason}");
                Console.ResetColor();
            }
        }
    }
}
