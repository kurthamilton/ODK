@echo off
rem Exposes the local dev app on a public URL. ngrok.yml lives in the repo root and is gitignored, so it is
rem passed by absolute path - resolved from this script's own location rather than the current directory.
ngrok start odk --config "%~dp0..\..\ngrok.yml"
