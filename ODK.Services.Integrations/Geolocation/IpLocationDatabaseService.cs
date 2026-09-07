using System.Globalization;
using System.IO.Abstractions;
using System.IO.Compression;
using ODK.Data.Core;
using ODK.Services.Geolocation;
using ODK.Services.Geolocation.ViewModels;
using ODK.Services.Logging;

namespace ODK.Services.Integrations.Geolocation;

public class IpLocationDatabaseService : OdkAdminServiceBase, IIpLocationDatabaseService
{
    private const string FilePattern = "dbip-city-lite-*.mmdb";
    private const string FilePrefix = "dbip-city-lite-";
    private const string FileSuffix = ".mmdb";

    /* DB-IP publish one file per month at a dated URL and withdraw the old ones, so an update asks for a
       month rather than for "the latest". The current month is not published on its first days, hence the
       walk backwards - far enough to cover that gap, short enough to fail rather than install a stale
       database. */
    private const int MonthsToTry = 3;

    private readonly IFileSystem _fileSystem;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IIpLocationLookup _ipLocationLookup;
    private readonly ILoggingService _loggingService;
    private readonly GeolocationServiceSettings _settings;

    public IpLocationDatabaseService(
        IUnitOfWork unitOfWork,
        GeolocationServiceSettings settings,
        IFileSystem fileSystem,
        IHttpClientFactory httpClientFactory,
        ILoggingService loggingService,
        IIpLocationLookup ipLocationLookup)
        : base(unitOfWork)
    {
        _fileSystem = fileSystem;
        _httpClientFactory = httpClientFactory;
        _ipLocationLookup = ipLocationLookup;
        _loggingService = loggingService;
        _settings = settings;
    }

    public async Task<ServiceResult> Delete(IMemberServiceRequest request, string fileName)
    {
        AssertMemberIsSiteAdmin(request.CurrentMember);

        // The name arrives from a form, so it must name a database in the directory and nothing else.
        if (_fileSystem.Path.GetFileName(fileName) != fileName
            || !fileName.StartsWith(FilePrefix, StringComparison.OrdinalIgnoreCase)
            || !fileName.EndsWith(FileSuffix, StringComparison.OrdinalIgnoreCase))
        {
            return ServiceResult.Failure("Not an IP location database file name");
        }

        var path = _fileSystem.Path.Combine(_settings.IpDatabaseDirectory, fileName);

        if (string.Equals(path, _ipLocationLookup.CurrentDatabase, StringComparison.OrdinalIgnoreCase))
        {
            return ServiceResult.Failure("That database is the one in use");
        }

        try
        {
            _fileSystem.File.Delete(path);
            return ServiceResult.Successful();
        }
        catch (Exception ex)
        {
            await _loggingService.Error($"Error deleting IP location database {fileName}", ex);
            return ServiceResult.Failure("The file could not be deleted");
        }
    }

    public Task<DirectoryViewModel> GetDirectory(IMemberServiceRequest request, string? path)
    {
        AssertMemberIsSiteAdmin(request.CurrentMember);

        var directory = string.IsNullOrWhiteSpace(path) ? _settings.IpDatabaseDirectory : path.Trim();

        string? parent = null;
        var entries = new List<DirectoryEntryViewModel>();
        string? error = null;
        var exists = false;

        try
        {
            var full = _fileSystem.Path.GetFullPath(directory);
            directory = full;
            parent = _fileSystem.Directory.GetParent(full)?.FullName;
            exists = _fileSystem.Directory.Exists(full);

            if (exists)
            {
                entries.AddRange(_fileSystem.Directory
                    .GetDirectories(full)
                    .Select(x => Entry(x, isDirectory: true)));

                entries.AddRange(_fileSystem.Directory
                    .GetFiles(full)
                    .Select(x => Entry(x, isDirectory: false)));
            }
        }
        catch (Exception ex)
        {
            // The message is the diagnosis - an unreadable directory is what this page exists to show.
            error = ex.Message;
        }

        return Task.FromResult(new DirectoryViewModel
        {
            Entries = entries,
            Error = error,
            Exists = exists,
            Identity = Identity(),
            ParentPath = parent,
            Path = directory
        });
    }

    public Task<IpLocationDatabaseViewModel> GetViewModel(IMemberServiceRequest request)
    {
        AssertMemberIsSiteAdmin(request.CurrentMember);

        var directory = _settings.IpDatabaseDirectory;
        var loaded = _ipLocationLookup.CurrentDatabase;

        var files = !string.IsNullOrEmpty(directory) && _fileSystem.Directory.Exists(directory)
            ? _fileSystem.Directory
                .GetFiles(directory, FilePattern)
                .Select(x => _fileSystem.FileInfo.New(x))
                .OrderByDescending(x => x.Name, StringComparer.OrdinalIgnoreCase)
                .Select(x => new IpLocationDatabaseFileViewModel
                {
                    LastModifiedUtc = x.LastWriteTimeUtc,
                    Loaded = string.Equals(x.FullName, loaded, StringComparison.OrdinalIgnoreCase),
                    Name = x.Name,
                    SizeBytes = x.Length
                })
                .ToArray()
            : [];

        return Task.FromResult(new IpLocationDatabaseViewModel
        {
            Directory = directory,
            Files = files,
            Preloaded = _settings.PreloadIpDatabase
        });
    }

    public async Task<ServiceResult> TestWrite(IMemberServiceRequest request, string path)
    {
        AssertMemberIsSiteAdmin(request.CurrentMember);

        if (string.IsNullOrWhiteSpace(path))
        {
            return ServiceResult.Failure("No directory given");
        }

        var probe = _fileSystem.Path.Combine(path, $"odk-write-test-{Guid.NewGuid():N}.tmp");

        try
        {
            await _fileSystem.File.WriteAllTextAsync(probe, "odk");
        }
        catch (Exception ex)
        {
            return ServiceResult.Failure($"Cannot write to {path} as {Identity()}: {ex.Message}");
        }

        try
        {
            _fileSystem.File.Delete(probe);
        }
        catch (Exception ex)
        {
            return ServiceResult.Failure(
                $"Wrote to {path} as {Identity()} but could not delete the test file: {ex.Message}");
        }

        return ServiceResult.Successful($"Wrote and deleted a file in {path} as {Identity()}");
    }

    public async Task<ServiceResult> Update()
    {
        var directory = _settings.IpDatabaseDirectory;
        if (string.IsNullOrEmpty(directory))
        {
            var message = "IP location database directory is not configured";
            await _loggingService.Error(message);
            return ServiceResult.Failure(message);
        }

        if (!_fileSystem.Directory.Exists(directory))
        {
            var message = $"IP location database directory '{directory}' does not exist";
            await _loggingService.Error(message);
            return ServiceResult.Failure(message);
        }

        var months = Enumerable
            .Range(0, MonthsToTry)
            .Select(x => DateTime.UtcNow.AddMonths(-x).ToString("yyyy-MM", CultureInfo.InvariantCulture));

        string? newest = null;
        ServiceResult? downloadResult = null;

        foreach (var month in months)
        {
            var path = _fileSystem.Path.Combine(directory, $"{FilePrefix}{month}{FileSuffix}");

            if (_fileSystem.File.Exists(path))
            {
                newest = path;
                break;
            }

            var monthDownloadResult = await Download(month, path);
            if (monthDownloadResult.Success)
            {
                newest = path;
                downloadResult = monthDownloadResult;
                break;
            }
        }

        if (newest == null)
        {
            var message = $"Could not download the IP location database: no release found in the last {MonthsToTry} months";
            await _loggingService.Error(message);
            return ServiceResult.Failure(message);
        }

        if (downloadResult?.Success == true)
        {
            _ipLocationLookup.Reload();
        }

        // Runs whether or not anything was downloaded, so a file left behind by a failed delete goes on the
        // next run rather than waiting for the next release.
        var prunedFiles = Prune(directory, newest);

        var successMessage = "GeoIP update Finished. ";

        if (downloadResult?.Success == true)
        {
            successMessage += $"Downloaded {newest}.";
        }
        else
        {
            successMessage += $"No new files found.";
        }

        if (prunedFiles.Count > 0)
        {
            var prunedFilesString = string.Join(", ", prunedFiles);
            successMessage += $" Deleted {prunedFilesString}.";
        }

        await _loggingService.Info(successMessage);
        return ServiceResult.Successful(successMessage);
    }

    private static string Identity()
        => string.IsNullOrEmpty(Environment.UserDomainName)
            ? Environment.UserName
            : $@"{Environment.UserDomainName}\{Environment.UserName}";

    private async Task<ServiceResult> Download(string month, string path)
    {
        var url = $"https://download.db-ip.com/free/{FilePrefix}{month}{FileSuffix}.gz";

        var client = _httpClientFactory.CreateClient();

        using var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
        if (!response.IsSuccessStatusCode)
        {
            var responseContent = await response.Content.ReadAsStringAsync();
            var message = $"Error downloading GeoIP database: {responseContent}";
            await _loggingService.Error(message);
            return ServiceResult.Failure(message);
        }

        // Named per download: both platforms' instances update the same directory.
        var temporaryPath = $"{path}.{Guid.NewGuid():N}.tmp";

        try
        {
            /* Unpacked into a temporary file and moved into place only once the whole response has been
               read. A half-written database is worse than an absent one - a reader opens it and then fails
               on every lookup - and an interrupted download is the ordinary way to produce one. */
            await using (var source = await response.Content.ReadAsStreamAsync())
            await using (var gzip = new GZipStream(source, CompressionMode.Decompress))
            await using (var destination = _fileSystem.File.Create(temporaryPath))
            {
                await gzip.CopyToAsync(destination);
            }

            _fileSystem.File.Move(temporaryPath, path, overwrite: true);
            return ServiceResult.Successful();
        }
        catch (Exception ex)
        {
            _fileSystem.File.Delete(temporaryPath);
            var message = $"Error downloading the GeoIP location database for {month}";
            await _loggingService.Error(message, ex);
            return ServiceResult.Failure(message);
        }
    }

    private DirectoryEntryViewModel Entry(string path, bool isDirectory)
    {
        // One entry's metadata being unreadable must not lose the rest of the listing.
        long? size = null;
        DateTime? modified = null;

        try
        {
            if (isDirectory)
            {
                modified = _fileSystem.DirectoryInfo.New(path).LastWriteTimeUtc;
            }
            else
            {
                var file = _fileSystem.FileInfo.New(path);
                modified = file.LastWriteTimeUtc;
                size = file.Length;
            }
        }
        catch (Exception)
        {
            // Left null, and the row still names the entry.
        }

        return new DirectoryEntryViewModel
        {
            IsDirectory = isDirectory,
            LastModifiedUtc = modified,
            Name = _fileSystem.Path.GetFileName(path.TrimEnd('/', '\\')),
            Path = path,
            SizeBytes = size
        };
    }

    private IReadOnlyCollection<string> Prune(string directory, string keep)
    {
        var prunedFiles = new List<string>();

        foreach (var path in _fileSystem.Directory.GetFiles(directory, FilePattern))
        {
            if (string.Equals(path, keep, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            try
            {
                _fileSystem.File.Delete(path);

                var fileName = _fileSystem.FileInfo.New(path).Name;
                prunedFiles.Add(fileName);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                // Still held by a reader in this process or the other instance. The next run retries.
            }
        }

        return prunedFiles;
    }
}
