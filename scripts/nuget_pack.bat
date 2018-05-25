@echo off

echo -- INIT ----------------------------------------------------------------------------------------------------------------------------------------------------
set LibName=SecureHttpClient
echo LibName: %LibName%
set /p Version=<..\version.txt
echo Version: %Version%

echo -- CLEAN ---------------------------------------------------------------------------------------------------------------------------------------------------
msbuild /v:m ../%LibName%/%LibName%.csproj /p:Configuration=Release /t:Clean

echo -- RESTORE -------------------------------------------------------------------------------------------------------------------------------------------------
msbuild /v:m ../%LibName%/%LibName%.csproj /p:Configuration=Release /t:Restore

echo -- BUILD ---------------------------------------------------------------------------------------------------------------------------------------------------
msbuild /v:m ../%LibName%/%LibName%.csproj /p:Configuration=Release /t:Build /p:Version=%Version%;AssemblyVersion=%Version%;AssemblyFileVersion=%Version%

echo -- PACK ----------------------------------------------------------------------------------------------------------------------------------------------------
msbuild /v:m ../%LibName%/%LibName%.csproj /p:Configuration=Release /t:Pack /p:NoBuild=true
move ..\%LibName%\bin\Release\%LibName%.%Version%.nupkg ..\ >nul

echo -- DONE !! -------------------------------------------------------------------------------------------------------------------------------------------------
