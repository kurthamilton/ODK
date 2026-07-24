@echo off
rem Launch dotnet and sass as two tabs in a single Windows Terminal window.
wt -d "%~dp0ODK.Web.Razor" --title "ODK dotnet (odk)" --suppressApplicationTitle cmd /k "set DOTNET_WATCH_SUPPRESS_STATIC_FILE_HANDLING=true && dotnet watch --urls=http://localhost:8124" ; new-tab -d "%~dp0ODK.Web.Razor" --title "ODK sass" --suppressApplicationTitle cmd /k "npm run watch:sass" ; focus-tab -t 0
