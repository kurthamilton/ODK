@echo off
rem Runs an E2E suite for one platform (or all). Opens a single Windows Terminal window with three tabs:
rem   - "e2e app"   : the app in the "e2e" environment on both ports (script.run.app.e2e.bat)
rem   - "e2e tests" : waits for the app, runs the tests for the given category, then stops the app.
rem   - "e2e ngrok" : public tunnel to the e2e app, for testing integrations that call back in
rem                   (see the ngrok section in the root README for the gitignored ngrok.yml).
rem
rem Usage: script.run.tests.bat [category]
rem   Pass a category to skip the prompt; run with no argument (or double-click) to be asked.
setlocal

set CATEGORY=%~1

rem Prompt when nothing was passed, so the useful subsets don't have to be memorised - double-clicking the
rem file then works too, which it didn't when the only way in was an argument.
if "%CATEGORY%"=="" (
    echo Which tests?
    echo.
    echo   E2E                  - everything [default]
    echo   Default              - Group Squirrel only
    echo   DrunkenKnitwits      - Drunken Knitwits only
    echo   Stripe               - payments only; slow, needs the ngrok tunnel up
    echo   NoStripe             - everything except payments
    echo.
    set /p "CATEGORY=Category [E2E]: "
)

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

wt new-tab --title "e2e app" -d "%ROOT%" cmd /k "script.run.app.e2e.bat" ; new-tab --title "e2e tests (%CATEGORY%)" -d "%ROOT%" cmd /k "script.e2e.bat %PORT% ODK.E2E.Tests\ODK.E2E.Tests.csproj %CATEGORY%" ; new-tab --title "e2e ngrok" -d "%ROOT%" cmd /k "script.run.ngrok.e2e.bat"

rem This launcher window closing immediately is normal - it just hands off to Windows Terminal. If the
rem terminal never appears, wt itself failed, so surface that instead of vanishing silently.
if errorlevel 1 (
    echo.
    echo Failed to launch Windows Terminal ^(wt exit code %errorlevel%^).
    pause
)
