using ODK.Core.Media;

namespace ODK.Services.Media;

public class MediaService : IMediaService
{
    private readonly IMediaFileProvider _mediaFileProvider;

    public MediaService(IMediaFileProvider mediaFileProvider)
    {
        _mediaFileProvider = mediaFileProvider;
    }

    public async Task<(MediaFile? MediaFile, byte[]? Data)> GetMediaFile(Guid chapterId, string name)
    {
        var file = await _mediaFileProvider.GetMediaFile(chapterId, name);
        if (file == null)
        {
            return (file, null);
        }

        var data = await File.ReadAllBytesAsync(file.FilePath);

        return (file, data);
    }
}
