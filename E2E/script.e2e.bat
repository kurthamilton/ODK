@echo off
rem Generic E2E test runner: waits for an already-starting app to be ready on a port, runs its E2E
rem tests, then stops the app (kills whatever is listening on the port). Exits with the test exit code.
rem
rem Writes an html run report to <test project>\TestResults\e2e.html and opens it when anything failed.
rem Failure traces and screenshots land alongside it in TestResults\artifacts (written by OdkPageTest).
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

rem A bare category name (E2E, Default, DrunkenKnitwits, Stripe, ...) is wrapped as TestCategory=<name>.
rem Anything already mentioning TestCategory is used verbatim, so sets can be composed - most usefully
rem "TestCategory!=Stripe" to skip the slow payment tests. Quote it when passing one. A filter using & (AND)
rem has to go to dotnet test directly: & is a command separator, so it can't survive this command line.
set "FILTER=TestCategory=%CATEGORY%"
if not "%CATEGORY:TestCategory=%"=="%CATEGORY%" set "FILTER=%CATEGORY%"

rem Alias for the inverse filter. It has to be an alias rather than the filter itself, because cmd treats =
rem as an argument delimiter: "TestCategory!=Stripe" passed as an argument arrives split in two.
if /i "%CATEGORY%"=="NoStripe" set "FILTER=TestCategory!=Stripe"

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
echo App is ready. Running E2E tests (%FILTER%) ...
rem console logger streams per-test results; the fixtures also print live START/PASS/FAIL + timing lines
rem via TestContext.Progress. Fixtures run in parallel (see AssemblyInfo.cs).
rem The html logger writes a run summary to review afterwards - it ships with Microsoft.NET.Test.Sdk, so
rem no extra package. A fixed LogFileName keeps the path predictable (the default is timestamped).
dotnet test "%TEST_PROJECT%" --filter "%FILTER%" --logger "console;verbosity=normal" --logger "html;LogFileName=e2e.html"
set TEST_EXIT=%errorlevel%

rem Report paths, so a failed run says where to look rather than leaving them to be hunted for. Failure
rem traces and screenshots are written per-test by OdkPageTest.
for %%d in ("%TEST_PROJECT%") do set "TEST_DIR=%%~dpd"
set "REPORT=%TEST_DIR%TestResults\e2e.html"
echo.
echo Report:    %REPORT%
echo Artifacts: %TEST_DIR%TestResults\artifacts  ^(traces + screenshots, failures only^)

rem Open the report when something failed - that's when it's worth reading, and a passing run shouldn't
rem steal focus. Flat gotos rather than a parenthesised if block: a stray ) or & inside one closes it early
rem and the script dies with a parse error. `start` returns immediately, so teardown still runs below.
if "%TEST_EXIT%"=="0" goto reported
if not exist "%REPORT%" goto reported
echo Opening the report ...
start "" "%REPORT%"
:reported

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
