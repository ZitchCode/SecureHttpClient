@echo off

SET MacServerAddress=192.168.118.128
SET MacServerUser=bertrand

echo -- CLEAN ---------------------------------------------------------------------------------------------------------------------------------------------------
msbuild /v:m ../SecureHttpClient.Android/SecureHttpClient.Android.csproj /p:Configuration=Release /t:Clean
msbuild /v:m ../SecureHttpClient.iOS/SecureHttpClient.iOS.csproj /p:Configuration=Release /t:Clean /p:ServerAddress=%MacServerAddress% /p:ServerUser=%MacServerUser%
msbuild /v:m ../SecureHttpClient.Portable/SecureHttpClient.Portable.csproj /p:Configuration=Release /t:Clean
if exist "..\bin\" rd /s/q "..\bin"

echo -- BUILD ---------------------------------------------------------------------------------------------------------------------------------------------------
msbuild /v:m ../SecureHttpClient.Android/SecureHttpClient.Android.csproj /p:Configuration=Release
msbuild /v:m ../SecureHttpClient.iOS/SecureHttpClient.iOS.csproj /p:Configuration=Release /p:ServerAddress=%MacServerAddress% /p:ServerUser=%MacServerUser%
msbuild /v:m ../SecureHttpClient.Portable/SecureHttpClient.Portable.csproj /p:Configuration=Release

echo -- MERGE ---------------------------------------------------------------------------------------------------------------------------------------------------
mkdir "..\bin"
..\..\ILRepack.exe /out:..\bin\MonoAndroid\SecureHttpClient.dll ..\SecureHttpClient.Android\bin\Release\SecureHttpClient.dll ..\SecureHttpClient.Android\bin\Release\SecureHttpClient.Abstractions.dll
..\..\ILRepack.exe /out:..\bin\Xamarin.iOS\SecureHttpClient.dll ..\SecureHttpClient.iOS\bin\Release\SecureHttpClient.dll ..\SecureHttpClient.CertificatePinning\bin\Release\SecureHttpClient.CertificatePinning.dll ..\SecureHttpClient.iOS\bin\Release\SecureHttpClient.Abstractions.dll
..\..\ILRepack.exe /out:..\bin\netstandard1.3\SecureHttpClient.dll ..\SecureHttpClient.Portable\bin\Release\SecureHttpClient.dll ..\SecureHttpClient.CertificatePinning\bin\Release\SecureHttpClient.CertificatePinning.dll ..\SecureHttpClient.Portable\bin\Release\SecureHttpClient.Abstractions.dll

echo -- GET VERSION ---------------------------------------------------------------------------------------------------------------------------------------------
set /p Version=<..\version.txt
echo Version: %Version%

echo -- NUGET PACK ----------------------------------------------------------------------------------------------------------------------------------------------
..\..\nuget.exe pack ..\SecureHttpClient.nuspec -OutputDir ..\ -version %Version%

echo -- DONE !! -------------------------------------------------------------------------------------------------------------------------------------------------
