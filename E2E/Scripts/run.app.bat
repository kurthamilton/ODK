@echo off
rem Runs the app in the "e2e" environment for ONE platform - Group Squirrel (gs, 8125) or Drunken Knitwits
rem (dk, 8126). A process serves the one platform its config states, so a suite covering both platforms
rem needs one of these per platform; run.tests.bat starts both.
rem
rem Usage: run.app.bat <gs|dk>
rem
rem Everything that differs between the two - the environment, the port, the platform - is in the e2e-gs and
rem e2e-dk launch profiles (ODK.Web.Razor\Properties\launchSettings.json). appsettings.e2e.json is what turns
rem the outside world off: console email, HIBP off, in-memory Hangfire, the local dev DB.
rem
rem The --artifacts-path is what lets the instances coexist. It moves bin *and* obj for every project in the
rem graph, so nothing is shared: without it a running instance holds the .exe in its bin and the next one to
rem build never starts, and a shared obj makes two builds at once fail on the same intermediate file. A tree
rem per instance also keeps an E2E run clear of a dev instance's output, so neither rebuilds the other's.
rem No dotnet watch: the tests run against a fixed build.
setlocal

set ODK_PLATFORM=%~1

if not "%ODK_PLATFORM%"=="gs" if not "%ODK_PLATFORM%"=="dk" (
    echo Usage: run.app.bat ^<gs^|dk^>
    pause
    exit /b 2
)

cd /d "%~dp0..\..\ODK.Web.Razor" || (
    echo Could not find ODK.Web.Razor next to "%~dp0..".
    pause
    exit /b 1
)

dotnet run --artifacts-path .\artifacts\e2e-%ODK_PLATFORM% --launch-profile e2e-%ODK_PLATFORM%
