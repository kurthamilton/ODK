@echo off
rem Hot-reload dev run. Serves BOTH platforms from one process - Group Squirrel (8123) and ODK (8124);
rem PlatformProvider resolves the platform from the request URL (see appsettings.Development.json
rem Platforms) - with .NET hot reload plus SCSS auto-compile.
rem
rem Opens a single Windows Terminal window with two tabs:
rem   - "app"  : dotnet watch (run.app.watch.bat)
rem   - "sass" : the SCSS watchers, expanded + minified
rem
rem Separate tabs rather than one `concurrently`: concurrently redirects stdin, which silently disables
rem dotnet watch's keyboard shortcuts. In its own tab it owns the console, so Ctrl+R force-restarts the
rem app - the escape hatch when a change isn't picked up or it doesn't recover from an error.
rem
rem Closing the window stops both tabs. Ctrl+C in a tab stops just that watcher.
setlocal

rem Repo root without the trailing backslash (a trailing "\" before a closing quote is read by Windows
rem Terminal as an escaped quote, which breaks the -d argument).
set "ROOT=%~dp0"
set "ROOT=%ROOT:~0,-1%"

wt new-tab --title "app" -d "%ROOT%" cmd /k "run.app.watch.bat" ; new-tab --title "sass" -d "%ROOT%\ODK.Web.Razor" cmd /k "npm run watch:sass"

rem This launcher window closing immediately is normal - it just hands off to Windows Terminal. If the
rem terminal never appears, wt itself failed, so surface that instead of vanishing silently.
if errorlevel 1 (
    echo.
    echo Failed to launch Windows Terminal ^(wt exit code %errorlevel%^).
    pause
)
