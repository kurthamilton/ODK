# Getting started

## Installation
1. Install the latest version of .NET
2. Install the latest version of SQL Server
3. Take a backup of the prod DB and restore locally

## Apps
The project runs two different platforms based on the base URL.

### ODK
The Drunken Knitwits platform, specifically for Drunken Knitwits groups around the world.

### Group Squirrel
A Meetup-style platform currently under development.

## Running locally
Run `run-odk.bat` to run the ODK platform or `run-gs.bat` to run the Group Squirrel platform.

The batch file spawns two tabs: the `dotnet` process in the main tab and a sass builder in the other.

## CSS
`.css` files are compiled into `wwwroot/css` from the `.scss` files in `wwwroot/scss`.

To compile, run `npm run build:css`. The compilation script also runs when the app is run from one of the batch files.