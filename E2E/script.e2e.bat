@echo off
rem Generic E2E test runner: waits for an already-starting app to be ready on a port, runs its E2E
rem tests, then stops the app (kills whatever is listening on the port). Exits with the test exit code.
rem
rem Usage: run.e2e.tests.bat <port> <path-to-test-csproj>
rem   e.g. run.e2e.tests.bat 8125 ODK.E2ETests\ODK.E2ETests.csproj
rem
rem One-time prerequisite: install the Playwright browsers (see ODK.E2ETests/README.md):
rem   powershell -File ODK.E2ETests\bin\Debug\net10.0\playwright.ps1 install
setlocal

set PORT=%~1
set TEST_PROJECT=%~2

if "%PORT%"=="" (echo Usage: script.run.tests.gs.bat ^<port^> ^<test-csproj^> & exit /b 2)
if "%TEST_PROJECT%"=="" (echo Usage: script.run.tests.gs.bat ^<port^> ^<test-csproj^> & exit /b 2)

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
echo App is ready. Running E2E tests ...
dotnet test "%TEST_PROJECT%" --filter "TestCategory=E2E"
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
