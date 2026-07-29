@echo off
rem Plain run for manual testing. Compiles the current SCSS once, then runs BOTH platforms from one
rem process - Group Squirrel (8123) and ODK (8124). No hot reload and no file watchers: the app is a
rem single foreground process, so closing this window (or Ctrl+C) exits it cleanly with nothing left
rem running. Edit code/SCSS? Stop and re-run to pick up changes (or use run.app.bat for hot reload).
cd /d "%~dp0ODK.Web.Razor"
call npm run build:css && dotnet run -- --urls=http://localhost:8123;http://localhost:8124 --environment=Development
