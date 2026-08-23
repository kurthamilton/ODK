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

rem An ODK.Web.Razor.exe left over from an earlier run holds the DLLs under bin and the two ports below,
rem so this run dies on a locked file or a failed bind rather than on anything naming a stale process as
rem the cause. One process serves both platforms, so whatever is already running is stale - stop it.
rem
rem Match this image name only. A broad `dotnet.exe` sweep would also take down unrelated builds, other
rem repos' watchers and IDE language servers.
tasklist /fi "imagename eq ODK.Web.Razor.exe" | find /i "ODK.Web.Razor.exe" >nul
if not errorlevel 1 (
    echo Stopping an ODK.Web.Razor.exe that is already running...
    taskkill /f /im ODK.Web.Razor.exe >nul 2>&1
)

rem npm holds both the SCSS compiler and the browser libraries the app serves, so a clone with no
rem node_modules has neither. `npm ci` installs exactly what package-lock.json pins.
if not exist node_modules (
    echo Installing npm packages...
    call npm ci
    if errorlevel 1 (
        echo.
        echo npm ci failed - fix the error above and run again.
        pause
        exit /b 1
    )
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
