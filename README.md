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
- Android 5-13 (api 21-33)
- iOS 16.0
- .net 7.0

About cookies and redirects:
- SecureHttpClient handles cookies and redirects, but the behavior can differ a bit from one platform to another, because of different implementations in the native libraries used by SecureHttpClient.
- for identical behavior between platforms, it's recommended to use [Flurl](https://github.com/tmenier/Flurl) on top of SecureHttpClient, and let [Flurl](https://github.com/tmenier/Flurl) handle cookies and redirects.

Supported frameworks:
- version 1.x: MonoAndroid ; Xamarin.iOS ; NetStandard
- version 2.0: net6.0-android ; net6.0-ios ; net6.0-windows
- version 2.1: net7.0-android ; net7.0-ios ; net7.0-windows
