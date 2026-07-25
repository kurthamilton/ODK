@echo off
rem Runs the app in the dedicated "e2e" environment bound to BOTH platform ports, so a single instance
rem serves Default (8125) and DrunkenKnitwits (8126) at once - PlatformProvider resolves the platform
rem from the request URL (see appsettings.e2e.json Platforms). appsettings.e2e.json also turns off
rem external services (console email, HIBP off, in-memory Hangfire, local dev DB).
cd ..
cd ODK.Web.Razor
set ASPNETCORE_ENVIRONMENT=e2e
set ASPNETCORE_URLS=http://localhost:8125;http://localhost:8126
dotnet run --no-launch-profile
