using ODK.Core.Media;
using ODK.Core.Utils;

namespace ODK.Services.Media;

public class MediaFileProvider : IMediaFileProvider
{
    private readonly MediaFileProviderSettings _settings;

    public MediaFileProvider(MediaFileProviderSettings settings)
    {
        _settings = settings;
    }

    public async Task<MediaFile?> GetMediaFile(Guid chapterId, string name)
    {
        string? filePath = await GetMediaFilePath(chapterId, name);

        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
        {
            return null;
        }

        return await GetMediaFile(chapterId, new FileInfo(filePath));
    }

    public async Task<string?> GetMediaFilePath(Guid chapterId, string name)
    {
        name = name.AlphaNumericImageFileName();

        var path = GetMediaPath(chapterId);
        var filePath = Path.Combine(path, name);

        if (!string.Equals(new FileInfo(filePath).Directory?.FullName, path, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return filePath;
    }

    public async Task<IReadOnlyCollection<MediaFile>> GetMediaFiles(Guid chapterId)
    {
        var path = GetMediaPath(chapterId);

        var mediaFiles = new List<MediaFile>();

        foreach (var file in new DirectoryInfo(path).GetFiles())
        {
            var mediaFile = await GetMediaFile(chapterId, file);
            mediaFiles.Add(mediaFile);
        }

        return mediaFiles.ToArray();
    }

    private Task<MediaFile> GetMediaFile(Guid chapterId, FileInfo file)
    {
        var url = GetMediaUrl(chapterId, file.Name);
        var mediaFile = new MediaFile(file.FullName, file.Name, url, file.CreationTime);
        return Task.FromResult(mediaFile);
    }

    private string GetMediaPath(Guid chapterId)
    {
        var chapterIdPart = chapterId.ToString()
            .AlphaNumeric()
            .ToLowerInvariant();

        var path = Path.Combine(_settings.RootMediaPath, chapterIdPart);
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        return path;
    }

    private string GetMediaUrl(Guid chapterId, string name)
    {
        throw new NotImplementedException();
    }
}
