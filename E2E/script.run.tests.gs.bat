@echo off
rem Runs the full Group Squirrel E2E suite. Opens a single Windows Terminal window with two tabs:
rem   - "GS e2e app"   : the app in the dedicated "e2e" environment on port 8125 (run.app.gs.e2e.bat)
rem   - "GS e2e tests" : waits for the app, runs the tests, then stops the app (run.e2e.tests.bat)
setlocal

set PORT=8125

rem Repo root without the trailing backslash (a trailing "\" before a closing quote is read by
rem Windows Terminal as an escaped quote, which breaks the -d argument).
set "ROOT=%~dp0"
set "ROOT=%ROOT:~0,-1%"

rem Free the port in case a previous run left the app behind (before the app tab starts).
for /f "tokens=5" %%p in ('netstat -ano ^| findstr ":%PORT% " ^| findstr "LISTENING"') do taskkill /F /T /PID %%p >nul 2>&1

wt new-tab --title "GS e2e app" -d "%ROOT%" cmd /k "script.run.app.gs.bat" ; new-tab --title "GS e2e tests" -d "%ROOT%" cmd /k "script.e2e.bat %PORT% ODK.E2E.Tests\ODK.E2E.Tests.csproj"
