using System.Threading.Tasks;
using ODK.Core.Media;

namespace ODK.Services.Media
{
    public interface IMediaService
    {
        Task<(MediaFile? MediaFile, byte[]? Data)> GetMediaFile(string chapter, string name);
    }
}
