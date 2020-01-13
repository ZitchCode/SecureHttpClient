@echo off

echo -- INIT ----------------------------------------------------------------------------------------------------------------------------------------------------
set Z_LibName=SecureHttpClient
echo LibName: %Z_LibName%
set /p Z_Version=<..\version.txt
echo Version: %Z_Version%

echo -- CLEAN ---------------------------------------------------------------------------------------------------------------------------------------------------
msbuild /v:m ../%Z_LibName%/%Z_LibName%.csproj /p:Configuration=Release /t:Clean

echo -- RESTORE -------------------------------------------------------------------------------------------------------------------------------------------------
msbuild /v:m ../%Z_LibName%/%Z_LibName%.csproj /p:Configuration=Release /t:Restore

echo -- BUILD ---------------------------------------------------------------------------------------------------------------------------------------------------
msbuild /v:m ../%Z_LibName%/%Z_LibName%.csproj /p:Configuration=Release /t:Build /p:Version=%Z_Version%;AssemblyVersion=%Z_Version%;AssemblyFileVersion=%Z_Version%

echo -- PACK ----------------------------------------------------------------------------------------------------------------------------------------------------
msbuild /v:m ../%Z_LibName%/%Z_LibName%.csproj /p:Configuration=Release /t:Pack /p:NoBuild=true /p:NuspecFile=..\SecureHttpClient.nuspec /p:NuspecProperties=version=%Z_Version%
move ..\%Z_LibName%\bin\Release\%Z_LibName%.%Z_Version%.nupkg ..\ >nul

echo -- DONE !! -------------------------------------------------------------------------------------------------------------------------------------------------
