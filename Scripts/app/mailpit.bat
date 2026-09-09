@echo off
rem Runs the local mail interceptor: SMTP on 1025, web UI on http://localhost:8025.
rem
rem Both dev instances send here - appsettings.Development.json sets Emails:Client to Smtp - so this is one
rem process for both platforms rather than one per platform. The From address says which sent what, since
rem the dev config gives GS and DK a +tag each.
rem
rem Usage: mailpit.bat [--ui]
rem   --ui  also open the web UI in a browser.
rem
rem BOUND TO 127.0.0.1 DELIBERATELY. Mailpit's own default is [::], which serves every captured message to
rem the whole network with no authentication.
rem
rem Started only if nothing holds 1025 already, so re-running run.bat leaves a sink that is already up
rem alone rather than tearing it down - unlike the app instances, whose ports run.bat clears.
setlocal

set "OPENUI="
if /i "%~1"=="--ui" set "OPENUI=1"

rem An unrecognised argument is refused rather than ignored, so a mistyped flag does not look like it worked.
if not "%~1"=="" if not defined OPENUI (
    echo Usage: mailpit.bat [--ui]
    exit /b 1
)

netstat -ano | findstr ":1025 " | findstr "LISTENING" >nul 2>&1
if not errorlevel 1 (
    echo Mailpit is already running - http://localhost:8025
    if defined OPENUI start "" http://localhost:8025
    exit /b 0
)

rem THE BINARY IS RESOLVED RATHER THAN CALLED BY NAME, and PATH is only the first place looked. winget
rem installs Mailpit as a portable package and adds its directory to the persisted user PATH, but a process
rem keeps the environment it started with - including the Windows Terminal process that opens this tab - so
rem a fresh install stays invisible to a new tab of an old terminal until everything holding a stale
rem environment has been restarted. Looking the exe up here means the sink runs as soon as it is installed.
set "MAILPIT="
for /f "delims=" %%f in ('where mailpit 2^>nul') do set "MAILPIT=%%f"

rem Any winget version, whatever the package directory's hash suffix. A scoop or hand-unzipped install is on
rem PATH instead, which is why that is tried first.
if not defined MAILPIT (
    for /f "delims=" %%f in ('dir /b /s "%LOCALAPPDATA%\Microsoft\WinGet\Packages\mailpit.exe" 2^>nul') do set "MAILPIT=%%f"
)

if not defined MAILPIT (
    echo.
    echo Could not find mailpit.exe, on PATH or under %LOCALAPPDATA%\Microsoft\WinGet\Packages.
    echo Install it with: winget install axllent.mailpit
    pause
    exit /b 1
)

rem GATED ON readyz RATHER THAN A SLEEP, and detached: the server holds this tab's foreground from the next
rem line on, so the only thing that can open the UI once it is listening is another process waiting for it.
rem readyz --wait blocks until the health endpoint answers, so the browser is never pointed at a port that
rem nothing has bound yet.
if defined OPENUI start "Mailpit UI" /min cmd /c ""%MAILPIT%" readyz -l 127.0.0.1:8025 --wait --timeout 30s >nul 2>&1 && start "" http://localhost:8025"

rem The message store is resolved from this script's own location, never the current directory, and is
rem gitignored. It keeps captured mail across restarts of the sink; Mailpit's default is memory only.
"%MAILPIT%" --listen 127.0.0.1:8025 --smtp 127.0.0.1:1025 --database "%~dp0..\..\mailpit.db"

rem Reached when mailpit exits. The window closing on its own would give no clue why, so say so and wait.
if errorlevel 1 (
    echo.
    echo Mailpit exited with code %errorlevel%.
    pause
)

exit /b 0
