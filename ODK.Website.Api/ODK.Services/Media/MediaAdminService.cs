using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ODK.Core.Chapters;
using ODK.Core.Images;
using ODK.Core.Media;
using ODK.Services.Exceptions;

namespace ODK.Services.Media
{
    public class MediaAdminService : OdkAdminServiceBase, IMediaAdminService
    {
        private readonly IMediaFileProvider _mediaFileProvider;

        public MediaAdminService(IChapterRepository chapterRepository, IMediaFileProvider mediaFileProvider)
            : base(chapterRepository)
        {
            _mediaFileProvider = mediaFileProvider;
        }

        public async Task<IReadOnlyCollection<MediaFile>> DeleteMediaFile(Guid currentMemberId, Guid chapterId, string name)
        {
            await AssertMemberIsChapterAdmin(currentMemberId, chapterId);

            MediaFile mediaFile = await _mediaFileProvider.GetMediaFile(chapterId, name);

            File.Delete(mediaFile.FilePath);

            return await _mediaFileProvider.GetMediaFiles(chapterId);
        }

        public async Task<IReadOnlyCollection<MediaFile>> GetMediaFiles(Guid currentMemberId, Guid chapterId)
        {
            await AssertMemberIsChapterAdmin(currentMemberId, chapterId);

            return await _mediaFileProvider.GetMediaFiles(chapterId);
        }

        public async Task<IReadOnlyCollection<MediaFile>> SaveMediaFile(Guid currentMemberId, Guid chapterId, string name, byte[] data)
        {
            await AssertMemberIsChapterAdmin(currentMemberId, chapterId);

            if (!ImageValidator.IsValidData(data))
            {
                throw new OdkServiceException("File must be an image");
            }

            string filePath = await _mediaFileProvider.GetMediaFilePath(chapterId, name);

            int version = 1;
            while (File.Exists(filePath))
            {
                FileInfo file = new FileInfo(filePath);
                string versionedFileName = $"{file.Name.Substring(0, file.Name.Length - file.Extension.Length)}{++version}{file.Extension}";
                filePath = Path.Combine(file.Directory.FullName, versionedFileName);
            }

            await File.WriteAllBytesAsync(filePath, data);

            return await _mediaFileProvider.GetMediaFiles(chapterId);
        }
    }
}
