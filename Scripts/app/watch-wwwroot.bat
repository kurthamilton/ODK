@echo off
rem Logs what appears under ODK.Web.Razor\wwwroot, to find the file creation that takes dotnet watch down -
rem see watch-wwwroot.ps1 for the bug and for how to read the output. Run it in a window of its own,
rem alongside the app, and reproduce the crash.
setlocal

rem Resolved from this script's own location, so it works whatever the current directory is.
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0watch-wwwroot.ps1" %*

pause
