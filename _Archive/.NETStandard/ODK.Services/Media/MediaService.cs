using System.IO;
using System.Threading.Tasks;
using ODK.Core.Media;

namespace ODK.Services.Media
{
    public class MediaService : IMediaService
    {
        private readonly IMediaFileProvider _mediaFileProvider;

        public MediaService(IMediaFileProvider mediaFileProvider)
        {
            _mediaFileProvider = mediaFileProvider;
        }

        public async Task<(MediaFile? MediaFile, byte[]? Data)> GetMediaFile(string chapter, string name)
        {
            MediaFile? file = await _mediaFileProvider.GetMediaFile(chapter, name);
            byte[] data = await File.ReadAllBytesAsync(file?.FilePath);

            return (file, data);
        }
    }
}
