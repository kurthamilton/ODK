﻿using ODK.Core.Chapters;
using ODK.Core.Media;
using ODK.Core.Utils;
using ODK.Data.Core;

namespace ODK.Services.Media;

public class MediaFileProvider : IMediaFileProvider
{
    private readonly MediaFileProviderSettings _settings;
    private readonly IUnitOfWork _unitOfWork;

    public MediaFileProvider(IUnitOfWork unitOfWork,
        MediaFileProviderSettings settings)
    {        
        _settings = settings;
        _unitOfWork = unitOfWork;
    }

    public async Task<MediaFile?> GetMediaFile(Guid chapterId, string name)
    {
        Chapter? chapter = await GetChapter(chapterId);
        if (chapter == null)
        {
            return null;
        }

        return await GetMediaFile(chapter.Name, name);
    }

    public async Task<MediaFile?> GetMediaFile(string chapter, string name)
    {
        string? filePath = GetMediaFilePath(chapter, name);

        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
        {
            return null;
        }

        return await GetMediaFile(chapter, new FileInfo(filePath));
    }

    public async Task<string?> GetMediaFilePath(Guid chapterId, string name)
    {
        Chapter? chapter = await GetChapter(chapterId);
        if (chapter == null)
        {
            return null;
        }

        return GetMediaFilePath(chapter.Name, name);
    }

    public async Task<IReadOnlyCollection<MediaFile>> GetMediaFiles(Guid chapterId)
    {
        Chapter? chapter = await GetChapter(chapterId);
        if (chapter == null)
        {
            return Array.Empty<MediaFile>();
        }

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
        return await _unitOfWork.ChapterRepository.GetById(chapterId).Run();
    }

    private Task<MediaFile> GetMediaFile(string chapter, FileInfo file)
    {
        string url = GetMediaUrl(chapter, file.Name);
        MediaFile mediaFile = new MediaFile(file.FullName, file.Name, url, file.CreationTime);
        return Task.FromResult(mediaFile);
    }

    private string? GetMediaFilePath(string chapter, string name)
    {
        chapter = chapter.AlphaNumeric();
        name = name.AlphaNumericImageFileName();

        string path = GetMediaPath(chapter);
        string filePath = Path.Combine(path, name);

        if (!string.Equals(new FileInfo(filePath).Directory?.FullName, path, StringComparison.OrdinalIgnoreCase))
        {
            return null;
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
        throw new NotImplementedException();
    }
}
