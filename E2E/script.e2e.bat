@echo off
rem Generic E2E test runner: waits for an already-starting app to be ready on a port, runs its E2E
rem tests, then stops the app (kills whatever is listening on the port). Exits with the test exit code.
rem
rem Usage: script.e2e.bat <port> <path-to-test-csproj> [category]
rem   category defaults to E2E (all platforms). Use Default or DrunkenKnitwits to target one platform.
rem   e.g. script.e2e.bat 8125 ODK.E2E.Tests\ODK.E2E.Tests.csproj Default
rem
rem One-time prerequisite: install the Playwright browsers (see E2E/README.md):
rem   powershell -File ODK.E2E.Tests\bin\Debug\net10.0\playwright.ps1 install
setlocal

set PORT=%~1
set TEST_PROJECT=%~2
set CATEGORY=%~3
if "%CATEGORY%"=="" set CATEGORY=E2E

if "%PORT%"=="" (echo Usage: script.e2e.bat ^<port^> ^<test-csproj^> [category] & exit /b 2)
if "%TEST_PROJECT%"=="" (echo Usage: script.e2e.bat ^<port^> ^<test-csproj^> [category] & exit /b 2)

echo Waiting for the app to be ready on http://localhost:%PORT% ...
set /a TRIES=0
:waitloop
set /a TRIES+=1
powershell -NoProfile -Command "try { Invoke-WebRequest -UseBasicParsing -TimeoutSec 3 'http://localhost:%PORT%/' | Out-Null; exit 0 } catch { if ($_.Exception.Response) { exit 0 } else { exit 1 } }" >nul 2>&1
if not errorlevel 1 goto ready
if %TRIES% geq 60 (
    echo App did not become ready within ~2 minutes - aborting.
    goto teardown
)
timeout /t 2 >nul
goto waitloop

:ready
echo App is ready. Running E2E tests (TestCategory=%CATEGORY%) ...
rem console logger streams per-test results; the fixtures also print live START/PASS/FAIL + timing lines
rem via TestContext.Progress. Fixtures run in parallel (see AssemblyInfo.cs).
dotnet test "%TEST_PROJECT%" --filter "TestCategory=%CATEGORY%" --logger "console;verbosity=normal"
set TEST_EXIT=%errorlevel%

:teardown
echo Stopping the app ...
call :killport

if not defined TEST_EXIT set TEST_EXIT=1
echo Done ^(exit %TEST_EXIT%^).
pause
exit /b %TEST_EXIT%

:killport
for /f "tokens=5" %%p in ('netstat -ano ^| findstr ":%PORT% " ^| findstr "LISTENING"') do taskkill /F /T /PID %%p >nul 2>&1
exit /b 0

rem Stop only the ngrok agent serving this endpoint (matched on its command line), so a separately
rem started tunnel - e.g. the dev "odk" one, whose command line doesn't contain this name - keeps
rem running. Also kills the cmd hosting it (the agent's parent), which is what closes its terminal tab.
exit /b 0
