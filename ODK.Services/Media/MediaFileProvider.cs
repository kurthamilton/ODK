using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ODK.Core.Chapters;
using ODK.Core.Media;
using ODK.Core.Utils;
using ODK.Services.Caching;
using ODK.Services.Exceptions;

namespace ODK.Services.Media
{
    public class MediaFileProvider : IMediaFileProvider
    {
        private readonly ICacheService _cacheService;
        private readonly IChapterRepository _chapterRepository;
        private readonly MediaFileProviderSettings _settings;

        public MediaFileProvider(IChapterRepository chapterRepository, ICacheService cacheService,
            MediaFileProviderSettings settings)
        {
            _cacheService = cacheService;
            _chapterRepository = chapterRepository;
            _settings = settings;
        }

        public async Task<MediaFile> GetMediaFile(Guid chapterId, string name)
        {
            Chapter chapter = await GetChapter(chapterId);

            return await GetMediaFile(chapter.Name, name);
        }

        public async Task<MediaFile> GetMediaFile(string chapter, string name)
        {
            string filePath = GetMediaFilePath(chapter, name);

            if (!File.Exists(filePath))
            {
                throw new OdkNotFoundException();
            }

            return await GetMediaFile(chapter, new FileInfo(filePath));
        }

        public async Task<string> GetMediaFilePath(Guid chapterId, string name)
        {
            Chapter chapter = await GetChapter(chapterId);

            return GetMediaFilePath(chapter.Name, name);
        }

        public async Task<IReadOnlyCollection<MediaFile>> GetMediaFiles(Guid chapterId)
        {
            Chapter chapter = await GetChapter(chapterId);

            string path = GetMediaPath(chapter.Name);

            List<MediaFile> mediaFiles = new List<MediaFile>();

            foreach (FileInfo file in new DirectoryInfo(path).GetFiles())
            {
                MediaFile mediaFile = await GetMediaFile(chapter.Name, file);
                mediaFiles.Add(mediaFile);
            }

            return mediaFiles.ToArray();
        }

        private async Task<Chapter> GetChapter(Guid chapterId)
        {
            return await _cacheService.GetOrSetItem(
                () => _chapterRepository.GetChapter(chapterId),
                chapterId);
        }

        private Task<MediaFile> GetMediaFile(string chapter, FileInfo file)
        {
            string url = GetMediaUrl(chapter, file.Name);
            MediaFile mediaFile = new MediaFile(file.FullName, file.Name, url, file.CreationTime);
            return Task.FromResult(mediaFile);
        }

        private string GetMediaFilePath(string chapter, string name)
        {
            chapter = chapter.AlphaNumeric();
            name = name.AlphaNumericImageFileName();

            string path = GetMediaPath(chapter);
            string filePath = Path.Combine(path, name);

            if (!string.Equals(new FileInfo(filePath).Directory?.FullName, path, StringComparison.OrdinalIgnoreCase))
            {
                throw new OdkServiceException("Bad file name");
            }

            return filePath;
        }

        private string GetMediaPath(string chapter)
        {
            string path = Path.Combine(_settings.RootMediaPath, chapter.ToLowerInvariant());
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            return path;
        }

        private string GetMediaUrl(string chapter, string name)
        {
            return _settings.RootMediaUrl.Interpolate(new Dictionary<string, string>
            {
                { "name", name.ToLowerInvariant() },
                { "chapter.name", chapter.ToLowerInvariant() }
            });
        }
    }
}
