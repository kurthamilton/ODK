@echo off
rem The app tab of run.app.bat: hot-reload run of BOTH platforms from one process - Group Squirrel (8123)
rem and ODK (8124). Kept as its own script so the wt command line in run.app.bat needs no semicolon - wt
rem reads ";" as a tab separator, and the two URLs are semicolon-delimited.
rem
rem Interactive on purpose (no --non-interactive): dotnet watch owns this console's stdin, so its
rem shortcuts work - notably Ctrl+R to force a restart when a change isn't picked up, or when the app
rem fails to come back up after an error.
cd /d "%~dp0ODK.Web.Razor"
dotnet watch run "--urls=http://localhost:8123;http://localhost:8124" --environment=Development
