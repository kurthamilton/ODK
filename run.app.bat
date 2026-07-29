@echo off
rem Hot-reload dev run. Serves BOTH platforms from one process - Group Squirrel (8123) and ODK (8124);
rem PlatformProvider resolves the platform from the request URL (see appsettings.Development.json
rem Platforms) - with .NET hot reload plus SCSS auto-compile.
rem
rem Everything runs in the FOREGROUND under a single `concurrently -k` (see the "dev" npm script):
rem closing this window or pressing Ctrl+C kills dotnet watch AND both sass watchers together, so no
rem orphaned process is left holding a lock on ODK.Web.Razor\bin.
cd /d "%~dp0ODK.Web.Razor"
call npm run dev
