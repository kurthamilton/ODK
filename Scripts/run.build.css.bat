@echo off
rem Compiles wwwroot\scss to wwwroot\css. Run this after editing a .scss - nothing watches them while the
rem app is running (see run.app.bat for why), then hard-refresh the browser.
setlocal

rem Resolved from this script's own location, so it works whatever the current directory is.
cd /d "%~dp0..\ODK.Web.Razor" || (
    echo Could not find ODK.Web.Razor next to "%~dp0..".
    pause
    exit /b 1
)

call npm run build:css
if errorlevel 1 (
    echo.
    echo SCSS compilation failed.
)

pause
