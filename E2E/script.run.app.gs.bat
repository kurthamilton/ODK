@echo off
rem Runs Group Squirrel in the dedicated "e2e" environment for the end-to-end tests (ODK.E2ETests).
rem appsettings.e2e.json overrides the base config: console email client (no real email sent), HIBP
rem breach check disabled, in-memory Hangfire, and the local dev database. Served on port 8125 so it
rem can run alongside a normal dev instance (run.app.gs.bat on 8123).
cd ..
cd ODK.Web.Razor
set ASPNETCORE_ENVIRONMENT=e2e
set ASPNETCORE_URLS=http://localhost:8125
dotnet run --no-launch-profile
