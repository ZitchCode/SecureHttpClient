SecureHttpClient
================

[![NuGet](https://img.shields.io/nuget/v/securehttpclient.svg?label=NuGet)](https://www.nuget.org/packages/securehttpclient/)

SecureHttpClient is a cross-platform HttpClientHandler library, with additional security features:
- certificate pinning
- TLS 1.2+
- client certificates

Usage:
- basic usage is similar to System.Net.Http.HttpClientHandler. 
- for advanced usage examples, look into the SecureHttpClient.Test folder.

Tested on the following platforms:
- Android 5-12 (api 21-31)
- iOS 15.0
- .net 5.0

About cookies and redirects:
- SecureHttpClient handles cookies and redirects, but the behavior can differ a bit from one platform to another, because of different implementations in the native libraries used by SecureHttpClient.
- for identical behavior between platforms, it's recommended to use [Flurl](https://github.com/tmenier/Flurl) on top of SecureHttpClient, and let [Flurl](https://github.com/tmenier/Flurl) handle cookies and redirects.
