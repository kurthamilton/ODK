cd ..
set /p name=Enter migration name:
dotnet ef migrations add %name% -p ODK.Data.EntityFramework.Migrations -s ODK.Web.Razor
pause