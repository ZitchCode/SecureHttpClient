# Benchmark Project (Future Enhancement)

## Overview

A benchmarking project using BenchmarkDotNet would be valuable for:
- Measuring performance of certificate pinning validation
- Comparing performance across platforms
- Tracking performance regressions
- Optimizing hot paths

## Suggested Structure

```
SecureHttpClient.Benchmarks/
├── SecureHttpClient.Benchmarks.csproj
├── CertificatePinningBenchmarks.cs
├── HttpClientBenchmarks.cs
└── README.md
```

## Sample Benchmark

```csharp
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using System.Security.Cryptography.X509Certificates;

namespace SecureHttpClient.Benchmarks
{
    [MemoryDiagnoser]
    [SimpleJob(launchCount: 1, warmupCount: 3, iterationCount: 10)]
    public class CertificatePinningBenchmarks
    {
        private X509Certificate2 _certificate;
        private CertificatePinner _pinner;

        [GlobalSetup]
        public void Setup()
        {
            // Load test certificate
            _certificate = LoadTestCertificate();
            _pinner = new CertificatePinner();
            _pinner.AddPins("example.com", new[] { "sha256/..." });
        }

        [Benchmark]
        public void ComputeSpkiFingerprint()
        {
            var fingerprint = SpkiFingerprint.Compute(_certificate);
        }

        [Benchmark]
        public void ValidateCertificatePin()
        {
            var result = _pinner.Check("example.com", _certificate);
        }

        [Benchmark]
        public void MatchHostnamePattern_Exact()
        {
            var matches = CertificatePinner.MatchesPattern("example.com", "example.com");
        }

        [Benchmark]
        public void MatchHostnamePattern_Wildcard()
        {
            var matches = CertificatePinner.MatchesPattern("**.example.com", "api.example.com");
        }
    }
}
```

## Running Benchmarks

```bash
cd SecureHttpClient.Benchmarks
dotnet run -c Release
```

## Project File

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <IsPackable>false</IsPackable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="BenchmarkDotNet" Version="0.14.0" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\SecureHttpClient\SecureHttpClient.csproj" />
  </ItemGroup>
</Project>
```

## Implementation Notes

- Focus on hot paths and performance-critical code
- Benchmark certificate validation and pin matching
- Include baseline comparisons
- Track results over time to detect regressions
- Consider adding to CI/CD for automated performance testing

## Future Metrics to Track

1. **Certificate Pinning Performance**
   - SPKI fingerprint computation time
   - Pin validation time
   - Pattern matching time

2. **HTTP Performance**
   - Request/response throughput
   - TLS handshake time
   - Memory allocations per request

3. **Platform Comparisons**
   - Android vs iOS vs Windows performance
   - Native vs managed implementation performance

## References

- [BenchmarkDotNet Documentation](https://benchmarkdotnet.org/)
- [Performance Testing Best Practices](https://docs.microsoft.com/en-us/dotnet/framework/performance/)
