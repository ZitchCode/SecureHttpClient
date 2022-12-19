@echo off

echo -- INIT ----------------------------------------------------------------------------------------------------------------------------------------------------
set Z_LibName=SecureHttpClient
echo LibName: %Z_LibName%
set /p Z_Version=<..\version.txt
echo Version: %Z_Version%
set Z_AndroidSdkDirectory=D:\android\sdk
echo AndroidSdkDirectory: %Z_AndroidSdkDirectory%

echo -- CLEAN ---------------------------------------------------------------------------------------------------------------------------------------------------
dotnet clean -v m ../%Z_LibName%/%Z_LibName%.csproj -c Release

echo -- BUILD ---------------------------------------------------------------------------------------------------------------------------------------------------
dotnet build -v m ../%Z_LibName%/%Z_LibName%.csproj -c Release -p:AndroidSdkDirectory=%Z_AndroidSdkDirectory% -p:Version=%Z_Version% -p:AssemblyVersion=%Z_Version% -p:AssemblyFileVersion=%Z_Version%

echo -- PACK ----------------------------------------------------------------------------------------------------------------------------------------------------
dotnet pack -v m ../%Z_LibName%/%Z_LibName%.csproj -c Release -p:AndroidSdkDirectory=%Z_AndroidSdkDirectory% --no-build -o ..\ -p:PackageVersion=%Z_Version% -p:NuspecFile=..\build\SecureHttpClient.nuspec -p:NuspecProperties=version=%Z_Version%

echo -- DONE !! -------------------------------------------------------------------------------------------------------------------------------------------------
