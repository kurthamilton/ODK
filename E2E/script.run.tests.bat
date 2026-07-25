@echo off
rem Runs an E2E suite for one platform (or all). Opens a single Windows Terminal window with two tabs:
rem   - "e2e app"   : the app in the "e2e" environment on both ports (script.run.app.e2e.bat)
rem   - "e2e tests" : waits for the app, runs the tests for the given category, then stops the app.
rem
rem Usage: script.run.tests.bat [category]   (category: Default | DrunkenKnitwits | E2E; default E2E)
setlocal

set CATEGORY=%~1
if "%CATEGORY%"=="" set CATEGORY=E2E

rem The two platform ports bind together in one process, so waiting on 8125 is enough.
set PORT=8125

rem Repo-relative root (this scripts folder) without the trailing backslash (a trailing "\" before a
rem closing quote is read by Windows Terminal as an escaped quote, which breaks the -d argument).
set "ROOT=%~dp0"
set "ROOT=%ROOT:~0,-1%"

rem Free both ports in case a previous run left the app behind (before the app tab starts).
for /f "tokens=5" %%p in ('netstat -ano ^| findstr ":8125 " ^| findstr "LISTENING"') do taskkill /F /T /PID %%p >nul 2>&1
for /f "tokens=5" %%p in ('netstat -ano ^| findstr ":8126 " ^| findstr "LISTENING"') do taskkill /F /T /PID %%p >nul 2>&1

wt new-tab --title "e2e app" -d "%ROOT%" cmd /k "script.run.app.e2e.bat" ; new-tab --title "e2e tests (%CATEGORY%)" -d "%ROOT%" cmd /k "script.e2e.bat %PORT% ODK.E2E.Tests\ODK.E2E.Tests.csproj %CATEGORY%"
