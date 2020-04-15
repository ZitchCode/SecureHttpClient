@echo off
echo List of folders to remove:
for /d /r .. %%d in (bin,obj) do @if exist "%%d" echo "%%d"
set /p temp= Hit enter to continue
for /d /r .. %%d in (bin,obj) do @if exist "%%d" rd /s/q "%%d"
echo Done !
