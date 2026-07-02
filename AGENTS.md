# SecureHttpClient Coding Instructions

- Cross-platform .NET `HttpClientHandler` library with certificate pinning, TLS enforcement, and advanced HTTP security. 
- Targets **Android** (OkHttp), **iOS** (NSUrlSession), and **.NET** (SocketsHttpHandler).
- See [README.md](README.md) for feature overview and installation.

---

## Build & Test

```bash
# Build all projects (Debug)
dotnet build

# Build Release (generates NuGet package)
dotnet build -c Release

# Run .NET tests (Windows/Linux/macOS)
dotnet test SecureHttpClient.Test/SecureHttpClient.Test.csproj

# Run test runner console app
dotnet run --project SecureHttpClient.TestRunner.Net

# SDK pinned in global.json: .NET 10.0.301, rollForward: disabled
```

> **Configuration**: Package versions managed centrally in `Directory.Packages.props`. Shared properties (`LangVersion`, `Nullable`, TFMs) in `Directory.Build.props`.

---

## Architecture

| Project | Purpose |
|---------|---------|
| `SecureHttpClient/` | Main library — shared pinning logic + per-platform handlers in `Platforms/` |
| `SecureHttpClient.OkHttp/` | Android-only Java bindings (OkHttp interceptors, binding metadata) |
| `SecureHttpClient.Test/` | xUnit integration tests (net10.0 only) |
| `SecureHttpClient.TestRunner.Net/` | Console test runner |
| `SecureHttpClient.TestRunner.Maui/` | MAUI GUI test runner (Android/iOS/Windows) |

Platform selection uses `#if __ANDROID__` / `#if __IOS__` directives, with `.csproj` conditional item groups filtering `Platforms/` subdirectories.

Key abstractions: `ISecureHttpClientHandler` and `IClientCertificateProvider` in `SecureHttpClient/Abstractions/`.

---

## Related Documentation

- [C# Coding Instructions](docs/coding/csharp.instructions.md) — applicable sections: Naming Conventions, Formatting, Nullable Reference Types, Testing. Not all sections apply (ASP.NET Core / EF Core / JWT / Azure-specific guidance is not relevant to this library).

---

## Code Conventions

- **Nullable reference types**: Enabled globally (`<Nullable>enable</Nullable>`).
- **C# version**: Uses `LangVersion 14.0` (C# 14), defined in `Directory.Build.props`.
- **File-scoped namespaces**: Used everywhere.
- **Collection expressions**: Preferred (e.g., `["val1", "val2"]`).
- **Pattern matching**: Use pattern matching and switch expressions where possible.
- **Structured logging**: `_logger?.LogDebug("msg {Param}", value)` — avoid string interpolation in log calls.
- **ConfigureAwait(false)**: Used in library code; NOT in test code.
- **Internal fields**: `_camelCase` prefix.
- **Null checks**: Use `is null` / `is not null` instead of `== null` / `!= null`.
- **`nameof`**: Prefer `nameof(...)` over string literals when referring to member names.
- **XML doc comments**: Required on all public APIs; include `<example>` and `<code>` where applicable.
- **Braces**: Insert newline before opening curly braces of any code block.
- **Comments**: Avoid unnecessary descriptive comments — code should be self-documenting. Only comment tricky logic, workarounds, or non-obvious constraints. All comments must be in English.
- **Test methods**: Naming pattern `{ClassName}_{Scenario}`.
- **HttpRequestException** with `AuthenticationException` InnerException: Consistent pattern for trust failures across all platforms.

---

## Platform-Specific Notes

### Android (`net10.0-android36.1`)
- HTTP engine: Square OkHttp 5.4.0
- Compression via custom `DecompressInterceptor.java` (gzip, deflate, brotli)
- Certificate pinning delegated to OkHttp's native `CertificatePinner.Builder`
- Headers ordering supported via `HeadersOrderInterceptor.java`
- HTTP/3 NOT supported (OkHttp lacks QUIC)
- Extra NuGet deps: `Square.OkHttp3.Android`, `Square.OkIO`, `Xamarin.Brotli.Dec`

### iOS (`net10.0-ios26.5`)
- HTTP engine: NSUrlSession (ephemeral)
- Certificate pinning validated in `DataTaskDelegate.DidReceiveChallenge`
- HTTP/3: enabled from first request (`AssumesHttp3Capable = true`)
- Headers ordering NOT supported (logs warning)
- No extra NuGet deps beyond logging
- Complex infrastructure: 11 files under `Platforms/iOS/`

### .NET (`net10.0`)
- HTTP engine: `SocketsHttpHandler`
- Simplest platform — good reference for understanding the API surface
- AOT compatible (`IsAotCompatible=true`)
- Headers ordering NOT supported (logs warning)
- No extra NuGet deps beyond logging
- Reference: `SecureHttpClient/Platforms/Net/SecureHttpClientHandler.cs`

---

## Common Pitfalls

1. **`SetHeadersOrder` is Android-only** — iOS and Net silently log a warning and ignore.
2. **HTTP/3 unsupported on Android** — OkHttp doesn't support QUIC. Tests use `SkipIfHttp3NotSupported()`.
3. **Content-Length mismatch on Android** — writing fewer bytes than `HttpContent.ContentLength` causes OkHttp `ProtocolException` (JNI crash risk). See `HttpTest_ContentLengthMismatchDoesNotCrashOkHttpThread`.
4. **GET with body** — allowed on .NET, throws on Android/iOS. Tests use `Skip.If` for those platforms.
5. **Deflate encoding ambiguity** — Android's decompressor distinguishes zlib (RFC 1950) from raw deflate (RFC 1951) by sniffing the first two bytes.
6. **iOS large uploads** — `httpbingo.org` intermittently fails on iOS due to HTTP/2 flow control; use `httpbin.org` instead for large upload tests.

---

## Testing

- **Framework**: xUnit v2.9.3 with `SkippableFact`
- **Target**: `net10.0` only
- **External test dependencies**: `httpbingo.org`, `httpbin.org`, `badssl.com`, `howsmyssl.com`, `github.com`, `cloudflare.com`
- Debug builds reference `SecureHttpClient.csproj` directly; Release builds use NuGet package reference
- Key test files:
  - `SecureHttpClient.Test/HttpTest.cs` — comprehensive HTTP functionality tests
  - `SecureHttpClient.Test/SslTest.cs` — SSL/TLS and certificate tests
  - `SecureHttpClient.Test/CertificatePinnerTest.cs` — pinning success/failure, wildcards
  - `SecureHttpClient.Test/CertificateHelperTest.cs` — SPKI fingerprint computation
  - `SecureHttpClient.Test/TestBase.cs` — shared test infrastructure pattern
