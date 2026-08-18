@echo off
rem Hot-reload dev run. Serves BOTH platforms from one process - Group Squirrel (8123) and ODK (8124);
rem PlatformProvider resolves the platform from the request URL (see appsettings.Development.json
rem Platforms).
rem
rem Compiles the SCSS once, then runs dotnet watch in this window.
rem
rem Do NOT add a sass --watch alongside dotnet watch. MSBuild enumerates and hashes every file under
rem wwwroot as a static web asset on each rebuild, so a sass watcher rewriting wwwroot\css while dotnet
rem watch is evaluating the project takes dotnet watch down with it - it exits on the spot and leaves the
rem console at a prompt. Excluding those files from the watch list (see the csproj) does not help, because
rem the build reads them whether they are watched or not.
rem
rem So CSS is compiled on demand: after editing a .scss, run Scripts\run.build.css.bat and hard-refresh the
rem browser. .cs and .cshtml hot reload is unaffected.
rem
rem dotnet watch owns this console's stdin, so its shortcuts work - notably Ctrl+R to force a restart when
rem a change isn't picked up, or when the app fails to come back up after an error.
setlocal

rem Paths are resolved from this script's own location (%~dp0 ends with a backslash), so it works whatever
rem the current directory is. Never `cd ..` - that is relative to the caller, not to the script.
cd /d "%~dp0..\ODK.Web.Razor" || (
    echo Could not find ODK.Web.Razor next to "%~dp0..".
    pause
    exit /b 1
)

echo Compiling SCSS...
call npm run build:css
if errorlevel 1 (
    echo.
    echo SCSS compilation failed - fix the error above and run again.
    pause
    exit /b 1
)

dotnet watch run "--urls=http://localhost:8123;http://localhost:8124" --environment=Development
