@echo off
color 4F
echo ============================================================
echo    WARNING: PRODUCTION DATABASE MIGRATION
echo ============================================================
echo.
set /p "confirm=Type YES to continue: "
if /i not "%confirm%"=="YES" (
    echo Aborted.
    exit /b 1
)
echo.
echo Running migration...
cd ..
rem Reads the prod connection string from the local (gitignored) ODK.Web.Razor/appsettings.Production.json.
dotnet ef database update -p ODK.Data.EntityFramework.Migrations -s ODK.Web.Razor -- --environment Production
pause
