using System.IO.Abstractions;
using System.Net;
using MaxMind.Db;
using MaxMind.GeoIP2;
using ODK.Core.Countries;

namespace ODK.Services.Integrations.Geolocation;

public class IpLocationLookup : IIpLocationLookup, IDisposable
{
    private const string FilePattern = "dbip-city-lite-*.mmdb";

    private readonly string _directory;
    private readonly IFileSystem _fileSystem;
    private readonly Lock _reloadLock = new();
    private DatabaseReader? _reader;
    private string? _readerPath;
    private DatabaseReader? _superseded;

    public IpLocationLookup(GeolocationServiceSettings settings, IFileSystem fileSystem)
    {
        _directory = settings.IpDatabaseDirectory;
        _fileSystem = fileSystem;

        if (settings.PreloadIpDatabase)
        {
            Reload();
        }
    }

    public string? CurrentDatabase => Volatile.Read(ref _reader) != null ? _readerPath : null;

    public bool DatabaseAvailable => EnsureLoaded() != null;

    public void Dispose()
    {
        Volatile.Read(ref _reader)?.Dispose();
        Volatile.Read(ref _superseded)?.Dispose();
        GC.SuppressFinalize(this);
    }

    public Location? Find(string ipAddress)
    {
        var reader = EnsureLoaded();

        // TryCity throws on a string that is not an address.
        if (reader == null || !IPAddress.TryParse(ipAddress, out var address))
        {
            return null;
        }

        if (!reader.TryCity(address, out var city) || city == null)
        {
            return null;
        }

        var (latitude, longitude) = (city.Location.Latitude, city.Location.Longitude);
        if (latitude == null || longitude == null)
        {
            return null;
        }

        var name = string.Join(
            ", ",
            new[] { city.City.Name, city.Country.IsoCode }.Where(x => !string.IsNullOrEmpty(x)));

        return !string.IsNullOrEmpty(name)
            ? new Location
            {
                LatLong = new LatLong(latitude.Value, longitude.Value),
                Name = name
            }
            : null;
    }

    public void Reload()
    {
        lock (_reloadLock)
        {
            var path = NewestDatabase();
            if (path == null || path == _readerPath)
            {
                return;
            }

            var reader = Open(path);
            if (reader == null)
            {
                return;
            }

            var replaced = Volatile.Read(ref _reader);

            Volatile.Write(ref _reader, reader);
            _readerPath = path;

            /* The replaced reader is disposed one reload later, never here: a lookup already running holds
               it, and disposing it under that thread faults on the unmapped memory. Reloads are a month
               apart, so waiting for the next one is long enough for any in-flight call to have returned. */
            Volatile.Read(ref _superseded)?.Dispose();
            Volatile.Write(ref _superseded, replaced);
        }
    }

    // MaxMind opens the path itself, so this one call stays outside IFileSystem.
    private static DatabaseReader? Open(string path)
    {
        try
        {
            return new DatabaseReader(path, FileAccessMode.MemoryMapped);
        }
        catch (Exception)
        {
            return null;
        }
    }

    private DatabaseReader? EnsureLoaded()
    {
        var reader = Volatile.Read(ref _reader);
        if (reader != null)
        {
            return reader;
        }

        /* Nothing open, either because this platform does not preload or because no database was on disk
           when it tried. Retried per call rather than remembered as failed, so a database that arrives
           later is picked up without a restart; the cost of a miss is a directory listing. */
        Reload();
        return Volatile.Read(ref _reader);
    }

    private string? NewestDatabase()
    {
        if (string.IsNullOrEmpty(_directory) || !_fileSystem.Directory.Exists(_directory))
        {
            return null;
        }

        // Dated yyyy-MM, so the newest sorts last.
        return _fileSystem.Directory
            .GetFiles(_directory, FilePattern)
            .OrderByDescending(x => x, StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault();
    }
}
