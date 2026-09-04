@echo off
rem Runs an E2E suite for one platform (or all). Opens a single Windows Terminal window with four tabs:
rem   - "e2e app gs" : the app on 8125, e2e-gs launch profile (run.app.bat gs)
rem   - "e2e app dk" : the app on 8126, e2e-dk launch profile (run.app.bat dk)
rem   - "e2e tests"  : waits for both, runs the tests for the given category, then stops them.
rem   - "e2e ngrok"  : public tunnel to the DrunkenKnitwits e2e app, for testing integrations that call
rem                    back in (see the ngrok section in the root README for the gitignored ngrok.yml).
rem
rem An instance serves the one platform its config states, so both start whatever the category: a fixture
rem takes its platform from its base class, and starting only one leaves the other's tests unable to load
rem a page.
rem
rem Both tabs build at the same time, and neither is built here first: each runs under an --artifacts-path of
rem its own, which moves bin *and* obj, so the two share no build output and cannot collide.
rem
rem Usage: run.tests.bat [category[,category...]]
rem   Pass a category to skip the prompt; run with no argument (or double-click) to be asked.
rem   Comma-separate to run several at once - a test in more than one of them still runs once.
setlocal

set CATEGORY=%~1

rem Prompt when nothing was passed, so the useful subsets don't have to be memorised - double-clicking the
rem file then works too, which it didn't when the only way in was an argument.
if "%CATEGORY%"=="" (
    echo Which tests?
    echo.
    echo   E2E                           - everything [default]
    echo   Default                       - Group Squirrel only
    echo   DrunkenKnitwits               - Drunken Knitwits only
    echo   Stripe                        - payments only; slow, needs the ngrok tunnel up
    echo   NoStripe                      - everything except payments
    echo.
    echo   One state machine each:
    echo   AccountWorkflows              - every route to an account: sign-up, activation, imported members
    echo   ChapterMembershipWorkflows    - every route into a group: sign-up, invitation, join, questions
    echo   ChapterPublicationWorkflows   - a group becoming findable: admin approves, then owner publishes
    echo.
    echo   Venues                        - venue admin; creation, name normalising, slugs, events filter
    echo   SiteQuestions                 - site FAQ admin and the About page; Group Squirrel only
    echo   EmailAdmin                    - a group customising its email templates; Group Squirrel only
    echo.
    echo   Comma-separate to combine, e.g. AccountWorkflows,ChapterMembershipWorkflows
    echo.
    set /p "CATEGORY=Category [E2E]: "
)

if "%CATEGORY%"=="" set CATEGORY=E2E

rem Spaces would split the value when it is passed on as an argument below, so "A, B" becomes "A,B".
set "CATEGORY=%CATEGORY: =%"

rem One port per instance. e2e.bat waits for every one of them and stops them all afterwards.
set PORTS=8125+8126

rem Repo-relative root (this scripts folder) without the trailing backslash (a trailing "\" before a
rem closing quote is read by Windows Terminal as an escaped quote, which breaks the -d argument).
set "ROOT=%~dp0"
set "ROOT=%ROOT:~0,-1%"

rem Free both ports in case a previous run left the app behind (before the app tabs start).
for %%q in (8125 8126) do call :killport %%q

wt new-tab --title "e2e app gs" -d "%ROOT%" cmd /k "run.app.bat gs" ; new-tab --title "e2e app dk" -d "%ROOT%" cmd /k "run.app.bat dk" ; new-tab --title "e2e tests (%CATEGORY%)" -d "%ROOT%" cmd /k "e2e.bat %PORTS% ODK.E2E.Tests\ODK.E2E.Tests.csproj %CATEGORY%" ; new-tab --title "e2e ngrok" -d "%ROOT%" cmd /k "run.ngrok.bat"

rem This launcher window closing immediately is normal - it just hands off to Windows Terminal. If the
rem terminal never appears, wt itself failed, so surface that instead of vanishing silently.
if errorlevel 1 (
    echo.
    echo Failed to launch Windows Terminal ^(wt exit code %errorlevel%^).
    pause
)

exit /b 0

:killport
for /f "tokens=5" %%p in ('netstat -ano ^| findstr ":%~1 " ^| findstr "LISTENING"') do taskkill /F /T /PID %%p >nul 2>&1
exit /b 0
