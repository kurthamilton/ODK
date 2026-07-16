cd ..
set /p name=Enter migration name:
dotnet ef database update %name% -p ODK.Data.EntityFramework.Migrations -s ODK.Web.Razor
pause