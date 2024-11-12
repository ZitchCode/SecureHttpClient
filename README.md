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
- Android 5-14 (API 21-34)
- iOS 18.0
- .net 8.0

Older versions support older frameworks (but they are not maintained anymore):
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
