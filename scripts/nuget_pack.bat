@echo off

echo -- GET VERSION ---------------------------------------------------------------------------------------------------------------------------------------------
set /p Version=<..\version.txt
echo Version: %Version%

echo -- CLEAN ---------------------------------------------------------------------------------------------------------------------------------------------------
msbuild /v:m ../SecureHttpClient/SecureHttpClient.csproj /p:Configuration=Release /t:Clean
if exist "..\bin\" rd /s/q "..\bin"

echo -- RESTORE -------------------------------------------------------------------------------------------------------------------------------------------------
msbuild /v:m ../SecureHttpClient/SecureHttpClient.csproj /p:Configuration=Release /t:Restore

echo -- BUILD ---------------------------------------------------------------------------------------------------------------------------------------------------
msbuild /v:m ../SecureHttpClient/SecureHttpClient.csproj /p:Configuration=Release

echo -- MERGE ---------------------------------------------------------------------------------------------------------------------------------------------------
mkdir "..\bin"
..\..\ILRepack.exe /out:..\bin\monoandroid81\SecureHttpClient.dll /ver:%Version% ..\SecureHttpClient\bin\Release\monoandroid81\SecureHttpClient.dll
..\..\ILRepack.exe /out:..\bin\xamarin.ios10\SecureHttpClient.dll /ver:%Version% ..\SecureHttpClient\bin\Release\xamarin.ios10\SecureHttpClient.dll
..\..\ILRepack.exe /out:..\bin\netstandard2.0\SecureHttpClient.dll /ver:%Version% ..\SecureHttpClient\bin\Release\netstandard2.0\SecureHttpClient.dll

echo -- NUGET PACK ----------------------------------------------------------------------------------------------------------------------------------------------
..\..\nuget.exe pack ..\SecureHttpClient.nuspec -OutputDir ..\ -version %Version%

echo -- DONE !! -------------------------------------------------------------------------------------------------------------------------------------------------
