@echo off
rem Opens one Windows Terminal window with a tab per platform:
rem   - "gs" : Group Squirrel   on http://localhost:8123
rem   - "dk" : Drunken Knitwits on http://localhost:8124
rem
rem A process serves the one platform its config states, so working on both at once means two of them. Both
rem tabs run dotnet watch, so an edit reloads both.
rem
rem Usage: run.bat
rem
rem AN ARTIFACTS PATH PER PLATFORM is what lets the two coexist. --artifacts-path moves bin *and* obj for
rem every project in the graph, so the two instances share nothing: without it they collide twice over - a
rem running instance holds the .exe in its bin, so the second one to build never starts, and a shared obj
rem means two builds at once fail on the same intermediate file. With a tree each, both tabs build whenever
rem they like.
rem
rem Each platform's config comes from its launch profile (Properties\launchSettings.json): the environment,
rem the port, and Platform - which is safe as an environment variable there and nowhere else, because a
rem profile's environment is applied to the launched app rather than to the build. MSBuild reads its own
rem Platform property from the environment, so `set Platform=Default` in a shell would silently move the
rem build to bin\Default\...
setlocal

cd /d "%~dp0..\..\ODK.Web.Razor" || (
    echo Could not find ODK.Web.Razor next to "%~dp0..".
    pause
    exit /b 1
)

rem Both instances from an earlier run are stopped before the tabs open, so neither new instance dies on a
rem port already bound. Only what is listening on these two ports is stale - killing by image name would
rem also take down an E2E instance, which runs the same .exe on ports of its own.
for %%q in (8123 8124) do call :killport %%q

rem Each tab starts in the PROJECT directory, not this scripts folder: the tabs run dotnet directly, so a
rem tab starting anywhere else fails with "Could not find a MSBuild project file", and its --artifacts-path
rem would resolve somewhere else again. %CD% is the directory cd'd to above, and carries no trailing
rem backslash - a trailing "\" before a closing quote is read by Windows Terminal as an escaped quote, which
rem breaks the -d argument.
set "ROOT=%CD%"

rem Windows Terminal focuses the tab it opened most recently, so DK is the one that comes up in front. That
rem matters for dotnet watch's shortcuts - Ctrl+R to force a restart - which only reach the tab with focus;
rem swap the two if you would rather land on GS.
wt new-tab --title "GS" -d "%ROOT%" cmd /k "dotnet watch --artifacts-path .\artifacts\gs run --launch-profile gs" ; new-tab --title "DK" -d "%ROOT%" cmd /k "dotnet watch --artifacts-path .\artifacts\dk run --launch-profile dk"

rem This launcher window closing immediately is normal - it just hands off to Windows Terminal. If the
rem terminal never appears, wt itself failed, so surface that instead of vanishing silently.
if errorlevel 1 (
    echo.
    echo Failed to launch Windows Terminal ^(wt exit code %errorlevel%^).
    pause
)

exit /b 0

:killport
for /f "tokens=5" %%p in ('netstat -ano ^| findstr ":%~1 " ^| findstr "LISTENING"') do (
    echo Stopping the process already listening on %~1 ...
    taskkill /F /T /PID %%p >nul 2>&1
)
exit /b 0
