## 1.17.6
- fix support for AllowAutoRedirect=false (was missing on android)

## 1.17.5
- vs : 16.8.5 (xamarin.vs 16.8.000.262 ; xamarin.android 11.1.0.26 ; xamarin.ios 14.10.0.4)
- xcode 12.4 (ios 14.4)
- Microsoft.Extensions.* 5.0.x

## 1.17.4
- fix nuspec

## 1.17.3
- remove duplicate code
- Xamarin.SquareUp.OkHttp3 4.9.1
- Xamarin.SquareUp.OkHttp3.UrlConnection 4.9.1
- test : fix pin

## 1.17.2
- add buildTransitive for nuget targets
- test : Xamarin.Essentials 1.6.1
- test : Xamarin.Forms 5.0.0.1931

## 1.17.1
- improve handling of timeout and unknownhost exceptions on android
- Xamarin.SquareUp.Okio 2.10.0

## 1.17.0
- vs : 16.8.4 (xamarin.vs 16.8.000.261 ; xamarin.android 11.1.0.26 ; xamarin.ios 14.8.0.3)
- Portable.BouncyCastle 1.8.9
- netstandard2.1
- compute spki without bouncycastle thanks to netstandard2.1 on .net
- C# 9.0
- dotnet sdk 5.0.101
- xcode 12.3 (ios 14.3)
- Microsoft.Extensions.* 3.1.11
- MSBuild.Sdk.Extras 3.0.23
- test : remove UWP project
- test : netcoreapp5.0
- test : Xamarin.Kotlin.StdLib 1.4.20
- test : Xamarin.Essentials 1.6.0
- test : Xamarin.Forms 5.0.0.1874

## 1.16.1
- vs : 16.8.3 (xamarin.vs 16.8.000.260 ; xamarin.android 11.1.0.17 ; xamarin.ios 14.6.0.15)
- xcode 12.2 (ios 14.2)
- MSBuild.Sdk.Extras 3.0.22
- test : Microsoft.NETCore.UniversalWindowsPlatform 6.2.11

## 1.16.0
- android now supports both system's proxy and httpclienthandler's proxy
- fix decompression (deflate and gzip) on android
- get rid of specific code for android version < 21
- test : add android network security config in order to trust user ca and simplify http traffic inspection

## 1.15.0
- vs : 16.8.0 (xamarin.vs 16.8.000.255 ; xamarin.android 11.1.0.17 ; xamarin.ios 14.4.1.3)
- fix msbuild warnings (ignore VSX1000 warning ; use license instead of licenseUrl in nuspec)
- Microsoft.Extensions.Logging.Abstractions 3.1.10
- Xamarin.SquareUp.OkHttp3 4.9.0
- Xamarin.SquareUp.OkHttp3.UrlConnection 4.9.0
- Xamarin.SquareUp.Okio 2.9.0
- android 11
- xcode 12.1 & ios 14.1
- test : Xamarin.Kotlin.StdLib 1.3.61
- test : remove Flurl
- test : netcoreapp3.1
- test : Microsoft.Extensions.DependencyInjection 3.1.10
- test : Microsoft.Extensions.Logging 3.1.10
- test : Xamarin.Forms 4.8.0.1687

## 1.14.1
- vs : 16.7.6 (xamarin.vs 16.7.000.456 ; xamarin.android 11.0.2.0 ; xamarin.ios 14.00.0.0)
- .net core sdk 3.1.400
- ios : xcode 12.0 (ios 14.0)
- fix tests
- MSBuild.Sdk.Extras 2.1.2
- Microsoft.Extensions.Logging.Abstractions 3.1.9
- Portable.BouncyCastle 1.8.8
- test : Xamarin.Essentials 1.5.3.2
- test : Xamarin.Forms 4.8.0.1534
- test : Xunit.SkippableFact 1.4.13
- test : Microsoft.Extensions.DependencyInjection 3.1.9
- test : Microsoft.Extensions.Logging 3.1.9
- test : Serilog 2.10.0
- test : reference nuget only in release

## 1.14.0
- vs : 16.6.0 preview 4.0 (xamarin.vs 16.6.000.1052 ; xamarin.android 10.3.0.74 ; xamarin.ios 13.18.1.31)
- make logger mandatory (required for DI scenario)
- test : add UWP test runner
- test : add test fixture
- test : Xamarin.Essentials 1.5.3.1
- test : Xamarin.Forms 4.6.0.726
- test : Xunit.SkippableFact 1.4.8
- test : Serilog.Sinks.Xamarin 0.2.0.64
- test : Microsoft.Extensions.DependencyInjection 3.1.3

## 1.13.7
- vs : 16.6.0 preview 2.1 (xamarin.vs 16.6.000.984 ; xamarin.android 10.3.0.33 ; xamarin.ios 13.18.0.22)
- .net core sdk 3.0.200
- add tests for ecc certificates
- Microsoft.Extensions.Logging.Abstractions 3.1.3
- Portable.BouncyCastle 1.8.6.7
- test : Xamarin.Essentials 1.5.2
- test : Xamarin.Forms 4.5.0.617

## 1.13.6
- vs : 16.5.0 preview 4.0 (xamarin.vs 16.5.000.468 ; xamarin.android 10.2.0.99 ; xamarin.ios 13.14.1.38)
- Microsoft.Extensions.Logging.Abstractions 3.1.2
- Portable.BouncyCastle 1.8.6
- test : Xamarin.Essentials 1.5.0
- test : Xamarin.Forms 4.5.0.356

## 1.13.5
- fix nuspec

## 1.13.4
- vs : 16.5.0 preview 2.0 (xamarin.vs 16.5.000.400 ; xamarin.android 10.2.0.84 ; xamarin.ios 13.14.1.17)
- C# 8.0
- Microsoft.Extensions.Logging.Abstractions 3.1.1
- test : Xamarin.Forms 4.3.0.991640

## 1.13.3
- vs : 16.5.0 preview 1.0 (xamarin.vs 16.5.000.307 ; xamarin.android 10.2.0.16 ; xamarin.ios 13.14.0.6)
- test : Xamarin.Forms 4.4.0.991265
- add proguard.cfg to nuget package

## 1.13.2
- vs : 16.3.10 (xamarin.vs 16.3.0.281 ; xamarin.android 10.0.6.2 ; xamarin.ios 13.6.0.12)
- Microsoft.Extensions.Logging.Abstractions 3.1.0
- Portable.BouncyCastle 1.8.5.2
- test : Newtonsoft.Json 12.0.3
- test : Xamarin.Forms 4.3.0.991211
- test : move certs to resources
- test : add http2 check

## 1.13.1
- fix exception mess
- test project cleanup
- test : Xunit.SkippableFact 1.3.12
- test : Xamarin.Essentials 1.3.1
- test : Xamarin.Forms 4.3.0.908675
- test : Flurl.Http 2.4.2

## 1.13.0
- vs : 16.3.6 (xamarin.vs 16.3.0.277 ; xamarin.android 10.0.3.0 ; xamarin.ios 13.4.0.2)
- .net core sdk 3.0.100
- MSBuild.Sdk.Extras 2.0.54
- add build files to solution
- portable pdb
- test on android 10 and ios 13
- netcoreapp3.0
- fix pin in tests
- fix expected tls version in tests
- Serilog 2.9.0
- Serilog.Sinks.Xamarin 0.1.37
- Serilog.Extensions.Logging 3.0.1
- Microsoft.Extensions.Logging.Abstractions 3.0.0
- Square.OkHttp3 3.14.4
- Square.Okio 1.17.4
- Square.OkHttp3.UrlConnection 3.12.3
- removed Karamunting.Square.*

## 1.12.4
- finally fixing properly the redirect uri bug on android

## 1.12.3
- fix missing data in reponse's last request for android

## 1.12.2
- vs : 16.2.0 preview 3.0 (xamarin.vs 16.2.0.81 ; xamarin.android 9.4.0.34 ; xamarin.ios 12.14.0.93)
- android: make sure the request url in the response corresponds to the last redirect url
- MSBuild.Sdk.Extras 2.0.29

## 1.12.1
- vs : 16.2.0 preview 2.0 (xamarin.vs 16.2.0.61 ; xamarin.android 9.4.0.17 ; xamarin.ios 12.14.0.83)
- .net core sdk 2.1.700
- Karamunting.Android.Square.OkHttp 3.14.2
- Karamunting.Android.Square.Okio 1.17.4
- Karamunting.Square.OkHttp3.UrlConnection 3.14.2
- Newtonsoft.Json 12.0.2
- Serilog.Extensions.Logging 2.0.4

## 1.12.0
- vs 2019 : 16.0.0 preview 4.3 (xamarin.vs 16.0.0.513 ; xamarin.android 9.2.0.5 ; xamarin.ios 12.6.0.23 ; vs for mac 8.0 build 2931)
- global.json dotnet sdk 2.1.602
- MSBuild.Sdk.Extras 1.6.68
- Portable.BouncyCastle 1.8.5
- Serilog 2.8.0
- Newtonsoft.Json 12.0.1
- Microsoft.Extensions.Logging.Abstractions 2.2.0
- xunit 2.4.1
- xunit.runner.utility 2.4.1
- xunit.runner.devices 2.5.25
- fix certificate pin in tests
- Karamunting.Android.Square.OkHttp 3.14.0
- Karamunting.Android.Square.Okio 1.17.3
- Karamunting.Square.OkHttp3.UrlConnection 3.14.0
- min Android version 21 (for okhttp 3.13.0)
- clean android test csproj ; use d8

## 1.11.0
- xamarin : 15.8.5 (xamarin.vs 4.11.0.776 ; xamarin.android 9.0.0.19 ; xamarin.ios 12.0.0.15 ; vs for mac 7.6.8.38)
- MSBuild.Sdk.Extras 1.6.55
- Portable.BouncyCastle 1.8.3
- xunit 2.4.0
- add global.json for .net sdk version 2.1.402
- support Android 9.0 and iOS 12
- use JavaNetCookieJar from OkHttp3.UrlConnection

## 1.10.0
- fix android Set-Cookie header issue (#7)
- ios : xcode 9.4.1 (ios 11.4)
- abstract client certificates into certificate providers (#6)

## 1.9.0
- xamarin : 15.7.3 (xamarin.vs 4.10.10.1 ; xamarin.android 8.3.3.2 ; xamarin.ios 11.12.0.4 ; mono 5.10.1.57 ; vs for mac 7.5.2.40)
- add support for setting custom Root CAs
- Portable.BouncyCastle 1.8.2

## 1.8.0
- xamarin : 15.7.2 (xamarin.vs 4.10.0.448 ; xamarin.android 8.3.0.19 ; xamarin.ios 11.10.1.178 ; mono 5.10.1.47 ; vs for mac 7.5.1.22)
- fix certificatepinner test
- better logging with ILogger (and Serilog in test runners)
- use multi-targeting for source project
- use default debugtype (portable) as it's now supported by xamarin
- simplify nuget pack
- add xmldoc

## 1.7.0
- add support for client certificates (by gtbX)
- xamarin : 15.6.5 servicing release (xamarin.vs 4.9.0.753 ; xamarin.android 8.2.0.16 ; xamarin.ios 11.9.1.24 ; mono 5.8.1.0 ; vs for mac 7.4.2.12)
- Portable.BouncyCastle 1.8.1.4
- Newtonsoft.Json 11.0.2
- support Android 8.1
- android : build-tools 27.0.3, platform-tools 27.0.1
- ios : xcode 9.3
- netstandard 2.0 ; clean csproj ; fix warnings
- fix certificatepinner test
- xUnit 2.3.1
- Square.OkHttp3 3.8.1 ; Square.OkIO 1.13.0

## 1.6.0
- xamarin : 15.4 stable (xamarin.vs 4.7.10.22 ; xamarin.android 8.0.0.33 ; xamarin.ios 11.2.0.8 ; mono 5.4.0.201)
- android : build-tools 26.0.2, platform-tools 26.0.1
- support Android 8.0
- support iOS 11
- fix pin change in certificate test

## 1.5.1
- security fix for iOS
- fix android certificate pinner when adding several hostnames
- change architecture for iOS
- more certificatePinner tests

## 1.5.0
- several bug fixes
- simplify version management
- migration for VS2017 & VS For Mac
- xamarin : xamarin 4.5.0.486 ; xamarin.android 7.3.1.2 ; xamarin.ios 10.10.0.37 ; mono 5.0.1
- android : build-tools 26, platform-tools 26
- Portable.BouncyCastle 1.8.1.2
- Newtonsoft.Json 10.0.3
- xunit 2.2.0
- Square.OkHttp3 3.5.0 ; Square.OkIO 1.11.0
- support Android 7.1

## 1.4.2
- netstandard 1.6.1
- netcoreapp 1.1.0
- system.net.requests 4.3.0
- fix iOS projects order (xamarin bug #44887)

## 1.4.1
- test : fix delete cookie test
- android : better implementation of cookiejar

## 1.4.0
- xamarin : upgrade to cycle 8 sr1 stable (xamarin.vs 4.2.1.60 ; xamarin.android 7.0.2.37 ; xamarin.ios 10.2.1.5 ; mono 4.6.2.7)
- test : add delete cookie test
- test : fix pin in ssl test following certificate change
- android : fix delete cookie

## 1.3.0
- xamarin : upgrade to cycle 8 sr0 stable update (xamarin.vs 4.2.0.703 ; xamarin.android 7.0.1.3 ; xamarin.ios 10.0.1.10 ; mono 4.6.1.5)
- rename project : NativeHttpClient -> SecureHttpClientHandler
- iOS / portable : certificate pinner is not static anymore
- test : fix certificate pinner tests
- test : autostart
- get rid of proxy
- Square.OkIO 1.10.0
- Square.OkHttp3 3.4.1.1
- Microsoft.NETCore.App 1.0.1

## 1.2.3
- build : modify test csproj to delete nuget.props file after build
- test : add http tests

## 1.2.2
- add certificatePinning project (for iOS and netstandard)
- iOS : add certificate pinning

## 1.2.1
- xamarin : upgrade to cycle 8 sr0 beta (xamarin.vs 4.2.0.688 ; xamarin.android 7.0.1.0 ; xamarin.ios 10.0.1.5 ; mono 4.6.0.251)
- xamarin : upgrade to cycle 8 sr0 stable (xamarin.vs 4.2.0.695 ; xamarin.android 7.0.1.2 ; xamarin.ios 10.0.1.8 ; mono 4.6.1.3)
- test : now accepts improvable tls1.2 (required for ios)

## 1.2.0
- xamarin : upgrade to cycle 8 stable (xamarin.vs 4.2.0.680 ; xamarin.android 7.0.0.18 ; xamarin.ios 10.0.0.6 ; mono 4.6.0.245)
- add iOS projects and update nuget script
- iOs implementation using NSUrlSessionHandler, but no certificate pinning for the moment

## 1.1.5
- generate versions automatically from version file

## 1.1.4
- xamarin : upgrade to cycle 8 beta pre3 (xamarin.vs 4.2.0.628 ; xamarin.android 7.0.0.3 ; mono 4.6.0.182)
- android : support android N (7.0)

## 1.1.3
- android : remove one useless dependency
- merge assemblies before packing nuget

## 1.1.2
- clean android resources

## 1.1.1
- remove pdb and mdb from nuget

## 1.1.0
- remove useless bait dll and use directly the portable versionning
- do not expose some public classes (thanks to internalsvisibleto)

## 1.0.5
- bait and switch : add abstractions dll
- clean nuget pack

## 1.0.4
- clean some references, force Square.OkIO 1.9.0
- implement bait and switch trick more properly with abstract dll

## 1.0.3
- add proxy (on supported platforms)

## 1.0.2
- webexception in case of trust failure

## 1.0.1
- re-add debug logs
- change nativemessagehandler constructors

## 1.0.0
- new project name : NativeHttpClient
- android : get rid of useless options (progressstreamcontent ; throwoncaptive ; disablecaching)
- android : use okhttp's certificatepinner and get rid of dirty legacy code
- portable : add certificate pinner

## 0.3.4.1
- use OkHttp 3.4.1
- fix concurrency issue with cookie manager
- increase timeout to 100s

## 0.3.1.2
- merge tests in common dll
- get rid of servicepointmanager
- cleaning

## 0.2.7.5
- support Tls1.2 on android <5.0
- validate certificates properly using subjectAltNames (for zesto)
- use OkHttp 2.7.5
- cleaning
