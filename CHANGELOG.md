## 1.7.0
- add support for client certificates (by gtbX)
- xamarin : 15.6.5 servicing release (xamarin.vs 4.9.0.753 ; xamarin.android 8.2.0.16 ; xamarin.ios 11.9.1.24 ; mono 5.8.1.0 ; vs for mac 7.4.2.12)
- Portable.BouncyCastle 1.8.1.4
- Newtonsoft.Json 11.0.2
- support Android 8.1
- android : build-tools 27.0.3, platform-tools 27.0.1
- ios : xcode 9.3

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
