# SecureHttpClient

SecureHttpClient is a dotnet cross-platform HttpClientHandler library, with additional security features.

## Features

| Feature | Android | iOS | Windows |
| ---: | :---: | :---: | :---: |
| Certificate pinning | :white_check_mark: | :white_check_mark: | :white_check_mark: |
| TLS 1.2+ | :white_check_mark: | :white_check_mark: | :white_check_mark: |
| HTTP/2 | :white_check_mark: | :white_check_mark: | :white_check_mark: |
| Compression (gzip / deflate / br) | :white_check_mark: | :white_check_mark: | :white_check_mark: |
| Client certificates | :white_check_mark: | :white_check_mark: | :white_check_mark: |
| Headers ordering | :white_check_mark: | :x: | :x: |
| Cookies | :white_check_mark: | :white_check_mark: | :white_check_mark: |

## Installation

[![NuGet](https://img.shields.io/nuget/v/SecureHttpClient)](https://www.nuget.org/packages/SecureHttpClient/)

The most recent version is available (and is tested) on the following platforms:
- Android 8-16.1 (API 26-36.1)
- iOS 26.1
- .net 10.0

Older versions support older frameworks (but they are not maintained anymore):
- v2.3: net9.0 (android / ios / windows)
- v2.2: net8.0 (android / ios / windows)
- v2.1: net7.0 (android / ios / windows)
- v2.0: net6.0 (android / ios / windows)
- v1.x: MonoAndroid ; Xamarin.iOS ; NetStandard

## Basic usage

Basic usage is similar to using `System.Net.Http.HttpClientHandler`.
```csharp
// create the SecureHttpClientHandler
var secureHttpClientHandler = new SecureHttpClientHandler(null);

// create the HttpClient
var httpClient = new HttpClient(secureHttpClientHandler);

// example of a simple GET request
var response = await httpClient.GetAsync("https://www.github.com");
var html = await response.Content.ReadAsStringAsync();
```

## Certificate pining

### Usage

After creating a `SecureHttpClientHandler` object, call `AddCertificatePinner` to add one or more certificate pinner.

The request will fail if the certificate pin is not correct.

```csharp
// create the SecureHttpClientHandler
var secureHttpClientHandler = new SecureHttpClientHandler(null);

// add certificate pinner
secureHttpClientHandler.AddCertificatePinner("www.github.com", ["sha256/YH8+l6PDvIo1Q5o6varvw2edPgfyJFY5fHuSlsVdvdc="]);

// create the HttpClient
var httpClient = new HttpClient(secureHttpClientHandler);

// example of a simple GET request
var response = await httpClient.GetAsync("https://www.github.com");
var html = await response.Content.ReadAsStringAsync();
```

### Domain patterns

`SecureHttpClient` behaves the same as `OkHttp`: pinning is per-hostname and/or per-wildcard pattern.

To pin both `example.com` and `www.example.com` you must configure both hostnames. Or you may use patterns to match sets of related domain names. The following forms are permitted:
- Full domain name: you may pin an exact domain name like `www.example.com`. It won't match additional prefixes (`abc.www.example.com`) or suffixes (`example.com`).
- Any number of subdomains: Use two asterisks like `**.example.com` to match any number of prefixes (`abc.www.example.com`, `www.example.com`) including no prefix at all (`example.com`). For most applications this is the best way to configure certificate pinning.
- Exactly one subdomain: Use a single asterisk like `*.example.com` to match exactly one prefix (`www.example.com`, `api.example.com`). Be careful with this approach as no pinning will be enforced if additional prefixes are present, or if no prefixes are present.

Note that any other form is unsupported. You may not use asterisks in any position other than the leftmost label.

If multiple patterns match a hostname, any match is sufficient. For example, suppose pin A applies to *.example.com and pin B applies to `api.example.com`. Handshakes for `api.example.com` are valid if either A's or B's certificate is in the chain.

### Compute the pin

In order to compute the pin (SPKI fingerprint of the server's SSL certificate), you can execute the following command (here for `www.github.com` host):
```shell
openssl s_client -connect www.github.com:443 -servername www.github.com | sed -ne '/-BEGIN CERTIFICATE-/,/-END CERTIFICATE-/p' | openssl x509 -noout -pubkey | openssl pkey -pubin -outform der | openssl dgst -sha256 -binary | openssl enc -base64
```

## Cookies and Redirect

SecureHttpClient handles cookies and redirects, but the behavior can differ a bit from one platform to another, because of different implementations in the native libraries used internally.

For strictly identical behavior between platforms, it's recommended to use [Flurl](https://github.com/tmenier/Flurl) on top of SecureHttpClient, and let it handle cookies and redirects.

```csharp
// create the SecureHttpClientHandler
var secureHttpClientHandler = new SecureHttpClientHandler(null);

// disable redirect and cookies management in this handler
secureHttpClientHandler.AllowAutoRedirect = false;
secureHttpClientHandler.UseCookies = false;

// create the FlurlClient and CookieSession, they will manage redirect and cookies
var httpClient = new HttpClient(secureHttpClientHandler);
var flurlClient = new FlurlClient(httpClient);
var flurlSession = new CookieSession(flurlClient);

// example of a simple GET request using Flurl
var html = await flurlSession
    .Request("https://www.github.com")
    .GetStringAsync();
```

## Advanced usage

For more advanced usage (logging, client certificates, cookies ordering...), have a look into the SecureHttpClient.Test folder for more code examples.

## Troubleshooting

### Certificate Pinning Issues

**Problem**: Certificate pin validation fails

**Solutions**:
- Verify the pin is correct using the openssl command provided in the documentation
- Check if the certificate has been rotated/renewed
- Ensure you're using the correct hostname pattern (exact, `*.domain`, or `**.domain`)
- Test the connection without pinning first to ensure basic connectivity works

**Problem**: Certificate pins need updating after server certificate renewal

**Solutions**:
- Always maintain backup pins for certificate rotation
- Monitor certificate expiration dates
- Plan app updates before certificates expire

### Platform-Specific Issues

**Android**:
- Ensure Android SDK is properly configured
- Verify minimum API level is 26 (Android 8.0) or higher
- Check that network security configuration doesn't conflict with pinning

**iOS**:
- Ensure minimum iOS version is 15.0 or higher
- Check App Transport Security (ATS) settings in Info.plist
- Verify keychain access permissions for client certificates

**Windows/.NET**:
- Ensure .NET 10.0 runtime is installed
- Check that Windows certificate store is accessible
- Verify proxy settings if behind corporate proxy

### Connection Issues

**Problem**: Timeout errors or slow connections

**Solutions**:
- Check network connectivity
- Verify firewall settings
- Adjust timeout settings if needed:
  ```csharp
  httpClient.Timeout = TimeSpan.FromSeconds(30);
  ```

**Problem**: SSL/TLS handshake failures

**Solutions**:
- Ensure server supports TLS 1.2 or higher
- Check if certificate chain is complete on server
- Verify system time is correct (affects certificate validation)

### Build Issues

**Problem**: Missing workload errors when building

**Solutions**:
```bash
# Install required workloads
dotnet workload restore
# Or install specific workloads
dotnet workload install maui android ios
```

**Problem**: Build fails with "NETSDK1147" error

**Solutions**:
- If only targeting Windows/.NET, build specific framework:
  ```bash
  dotnet build SecureHttpClient/SecureHttpClient.csproj -f net10.0
  ```
- For mobile platforms, install required workloads

### Common Mistakes

1. **Not handling certificate pinning failures gracefully**: Always handle exceptions and provide fallback or user feedback
2. **Disabling certificate validation in production**: Never set `ServerCertificateCustomValidationCallback` to always return true
3. **Logging sensitive data**: Be careful not to log request/response bodies containing sensitive information
4. **Not testing on all target platforms**: Behavior can differ between Android, iOS, and Windows

## Getting Help

- 📖 Check the [CONTRIBUTING.md](CONTRIBUTING.md) guide
- 🔒 Review [SECURITY.md](SECURITY.md) for security-related questions
- 🐛 Report bugs via [GitHub Issues](https://github.com/ZitchCode/SecureHttpClient/issues)
- 💬 Ask questions in [GitHub Discussions](https://github.com/ZitchCode/SecureHttpClient/discussions)

## Contributing

We welcome contributions! Please see [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines on how to contribute to this project.

## Security

Security is a top priority for this project. Please review our [Security Policy](SECURITY.md) for information on supported versions, security features, and how to report vulnerabilities.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
