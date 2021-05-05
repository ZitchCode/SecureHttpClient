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
- Android 5-11 (api 21-30)
- iOS 14.4
- .net 5.0
