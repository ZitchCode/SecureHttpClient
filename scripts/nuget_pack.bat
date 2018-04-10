@echo off

SET MacServerAddress=192.168.118.129
SET MacServerUser=bertrand

echo -- GET VERSION ---------------------------------------------------------------------------------------------------------------------------------------------
set /p Version=<..\version.txt
echo Version: %Version%

echo -- CLEAN ---------------------------------------------------------------------------------------------------------------------------------------------------
msbuild /v:m ../SecureHttpClient.Android/SecureHttpClient.Android.csproj /p:Configuration=Release /t:Clean
msbuild /v:m ../SecureHttpClient.iOS/SecureHttpClient.iOS.csproj /p:Configuration=Release /t:Clean /p:ServerAddress=%MacServerAddress% /p:ServerUser=%MacServerUser%
msbuild /v:m ../SecureHttpClient.Portable/SecureHttpClient.Portable.csproj /p:Configuration=Release /t:Clean
if exist "..\bin\" rd /s/q "..\bin"

echo -- RESTORE -------------------------------------------------------------------------------------------------------------------------------------------------
msbuild /v:m ../SecureHttpClient.Android/SecureHttpClient.Android.csproj /p:Configuration=Release /t:Restore
msbuild /v:m ../SecureHttpClient.iOS/SecureHttpClient.iOS.csproj /p:Configuration=Release /t:Restore /p:ServerAddress=%MacServerAddress% /p:ServerUser=%MacServerUser%
msbuild /v:m ../SecureHttpClient.Portable/SecureHttpClient.Portable.csproj /p:Configuration=Release /t:Restore

echo -- BUILD ---------------------------------------------------------------------------------------------------------------------------------------------------
msbuild /v:m ../SecureHttpClient.Android/SecureHttpClient.Android.csproj /p:Configuration=Release
msbuild /v:m ../SecureHttpClient.iOS/SecureHttpClient.iOS.csproj /p:Configuration=Release /p:ServerAddress=%MacServerAddress% /p:ServerUser=%MacServerUser%
msbuild /v:m ../SecureHttpClient.Portable/SecureHttpClient.Portable.csproj /p:Configuration=Release

echo -- MERGE ---------------------------------------------------------------------------------------------------------------------------------------------------
mkdir "..\bin"
..\..\ILRepack.exe /out:..\bin\MonoAndroid\SecureHttpClient.dll /ver:%Version% ..\SecureHttpClient.Android\bin\Release\SecureHttpClient.dll ..\SecureHttpClient.Android\bin\Release\SecureHttpClient.Abstractions.dll
..\..\ILRepack.exe /out:..\bin\Xamarin.iOS\SecureHttpClient.dll /ver:%Version% ..\SecureHttpClient.iOS\bin\Release\SecureHttpClient.dll ..\SecureHttpClient.iOS\bin\Release\SecureHttpClient.CertificatePinning.dll ..\SecureHttpClient.iOS\bin\Release\SecureHttpClient.Abstractions.dll
..\..\ILRepack.exe /out:..\bin\netstandard2.0\SecureHttpClient.dll /ver:%Version% ..\SecureHttpClient.Portable\bin\Release\netstandard2.0\SecureHttpClient.dll ..\SecureHttpClient.Portable\bin\Release\netstandard2.0\SecureHttpClient.CertificatePinning.dll ..\SecureHttpClient.Portable\bin\Release\netstandard2.0\SecureHttpClient.Abstractions.dll

echo -- NUGET PACK ----------------------------------------------------------------------------------------------------------------------------------------------
..\..\nuget.exe pack ..\SecureHttpClient.nuspec -OutputDir ..\ -version %Version%

echo -- DONE !! -------------------------------------------------------------------------------------------------------------------------------------------------
