using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ODK.Core.Chapters;
using ODK.Core.Media;
using ODK.Core.Utils;
using ODK.Services.Exceptions;

namespace ODK.Services.Media
{
    public class MediaAdminService : OdkAdminServiceBase, IMediaAdminService
    {
        private readonly MediaServiceSettings _settings;

        public MediaAdminService(IChapterRepository chapterRepository, MediaServiceSettings settings)
            : base(chapterRepository)
        {
            _settings = settings;
        }

        public async Task<IReadOnlyCollection<MediaFile>> GetMediaFiles(Guid currentMemberId, Guid chapterId)
        {
            await AssertMemberIsChapterAdmin(currentMemberId, chapterId);

            Chapter chapter = await ChapterRepository.GetChapter(chapterId);

            string path = GetMediaPath(chapter);

            List<MediaFile> mediaFiles = new List<MediaFile>();

            foreach (FileInfo file in new DirectoryInfo(path).GetFiles())
            {
                MediaFile mediaFile = await GetMediaFile(file, chapter);
                mediaFiles.Add(mediaFile);
            }

            return mediaFiles.ToArray();
        }

        public async Task<MediaFile> SaveMediaFile(Guid currentMemberId, Guid chapterId, string name, byte[] data)
        {
            await AssertMemberIsChapterAdmin(currentMemberId, chapterId);

            Chapter chapter = await ChapterRepository.GetChapter(chapterId);

            string path = GetMediaPath(chapter);

            string filePath = Path.Combine(path, name);

            if (File.Exists(filePath))
            {
                throw new OdkServiceException("File already exists");
            }

            await File.WriteAllBytesAsync(filePath, data);

            return await GetMediaFile(new FileInfo(filePath), chapter);
        }

        private async Task<MediaFile> GetMediaFile(FileInfo file, Chapter chapter)
        {
            byte[] data = await File.ReadAllBytesAsync(file.FullName);
            string url = GetMediaUrl(chapter, file.Name);
            return new MediaFile(file.Name, data, url);
        }

        private string GetMediaPath(Chapter chapter)
        {
            return Path.Combine(_settings.RootMediaPath, chapter.Name.ToLowerInvariant());
        }

        private string GetMediaUrl(Chapter chapter, string fileName)
        {
            return $"{_settings.RootMediaUrl}/{fileName}".Interpolate(new Dictionary<string, string>
            {
                { "chapter.name", chapter.Name.ToLowerInvariant() }
            });
        }
    }
}
