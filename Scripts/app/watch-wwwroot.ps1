#Requires -Version 5.1
<#
    Logs every file that appears, is renamed, or is removed under ODK.Web.Razor\wwwroot, to name what takes
    dotnet watch down rather than guess at it.

    dotnet watch crashes when a file is CREATED under wwwroot while it is running - an open SDK bug
    (dotnet/roslyn#84062, "Unexpected true - file HotReloadMSBuildWorkspace.cs line 158"). Editing a file
    that already exists is fine, as is creating one anywhere outside wwwroot, and item metadata cannot help:
    the crash comes before the watcher decides whether the file belongs to the project, so Watch="false"
    protects nothing.

    What creates those files is the editor. Visual Studio saves by writing the new content to a temp file
    beside the target and renaming over the original, so one save produces a trace like:

        Created   scss\w0t3v23j.nmk~
        Created   scss\_bands.scss~RFb5a5a5e.TMP
        Renamed   scss\_bands.scss -> scss\_bands.scss~RFb5a5a5e.TMP
        Renamed   scss\w0t3v23j.nmk~ -> scss\_bands.scss

    Two creations, and the watcher is gone. The SCSS sources were moved out of wwwroot for exactly this
    reason. Anything still hand-edited under wwwroot - wwwroot\js\odk.*.js above all - has the same problem,
    so run this to confirm before assuming a crash is something new; dotnet watch --no-hot-reload avoids the
    whole class, at the cost of C# hot reload.

    This uses the same FileSystemWatcher dotnet watch does, so it sees the same events. Its log goes to TEMP
    rather than anywhere under the repository, because a logger writing into the tree it watches would be
    the very thing it is looking for.
#>
[CmdletBinding()]
param(
    # Also log Changed events. Off by default: a save produces several per file and they are not what
    # crashes the watcher, so they only bury the events that do.
    [switch]$IncludeChanges
)

$ErrorActionPreference = 'Stop'

$root = Join-Path (Split-Path -Parent (Split-Path -Parent $PSScriptRoot)) 'ODK.Web.Razor\wwwroot'
if (-not (Test-Path $root)) {
    Write-Error "Could not find $root."
    return
}

$logPath = Join-Path $env:TEMP 'odk-wwwroot-events.log'

Write-Host "Watching $root"
Write-Host "Logging to $logPath"
Write-Host 'Reproduce the crash, then read the last Created/Renamed line. Ctrl+C to stop.'
Write-Host ''

$watcher = New-Object System.IO.FileSystemWatcher
$watcher.Path = $root
$watcher.IncludeSubdirectories = $true
$watcher.EnableRaisingEvents = $true

# The event handler runs in its own scope, so what it needs is passed through MessageData rather than
# captured. Colour makes the creations - the only events that crash the watcher - findable at a glance.
$context = @{
    Root = $root
    LogPath = $logPath
    Colours = @{ Created = 'Red'; Renamed = 'Yellow'; Deleted = 'DarkGray'; Changed = 'DarkGray' }
}

$action = {
    $eventArgs = $Event.SourceEventArgs
    $data = $Event.MessageData

    $relative = $eventArgs.FullPath
    if ($relative.StartsWith($data.Root, [StringComparison]::OrdinalIgnoreCase)) {
        $relative = $relative.Substring($data.Root.Length).TrimStart('\')
    }

    $name = $eventArgs.ChangeType.ToString()
    if ($name -eq 'Renamed') {
        $old = $eventArgs.OldFullPath
        if ($old.StartsWith($data.Root, [StringComparison]::OrdinalIgnoreCase)) {
            $old = $old.Substring($data.Root.Length).TrimStart('\')
        }

        $relative = "$old -> $relative"
    }

    $line = '{0}  {1,-8}  {2}' -f (Get-Date).ToString('HH:mm:ss.fff'), $name, $relative

    $colour = $data.Colours[$name]
    if (-not $colour) { $colour = 'Gray' }
    Write-Host $line -ForegroundColor $colour

    Add-Content -Path $data.LogPath -Value $line -Encoding utf8
}

$types = @('Created', 'Renamed', 'Deleted')
if ($IncludeChanges) { $types += 'Changed' }

$subscriptions = foreach ($type in $types) {
    Register-ObjectEvent -InputObject $watcher -EventName $type -Action $action -MessageData $context
}

try {
    while ($true) { Start-Sleep -Seconds 1 }
}
finally {
    $subscriptions | Unregister-Event -ErrorAction SilentlyContinue
    $watcher.EnableRaisingEvents = $false
    $watcher.Dispose()
    Write-Host ''
    Write-Host "Stopped. Log: $logPath"
}
