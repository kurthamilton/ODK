@echo off
rem Launch dotnet (serving BOTH platforms at once) and sass as two tabs in a single Windows Terminal
rem window. One instance binds both platform ports - Group Squirrel (8123) and ODK (8124) - and
rem PlatformProvider resolves the platform from the request URL (see appsettings.Development.json
rem Platforms). The ports' ; separator is escaped as \; so Windows Terminal doesn't read it as a
rem command delimiter.
wt -d "%~dp0ODK.Web.Razor" --title "ODK dotnet (both)" --suppressApplicationTitle cmd /k "set DOTNET_WATCH_SUPPRESS_STATIC_FILE_HANDLING=true && dotnet watch run --urls=http://localhost:8123\;http://localhost:8124 --environment=Development" ; new-tab -d "%~dp0ODK.Web.Razor" --title "ODK sass" --suppressApplicationTitle cmd /k "npm run watch:sass" ; focus-tab -t 0
