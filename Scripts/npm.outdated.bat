@echo off
setlocal
pushd "%~dp0..\ODK.Web.Razor"

rem npm resolves to npm.cmd, and a batch file invoking another batch file without `call`
rem transfers control for good - the pause below would never be reached.
call npm outdated

popd
pause
