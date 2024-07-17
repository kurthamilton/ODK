﻿using ODK.Core.Media;
using ODK.Data.Core;
using ODK.Services.Exceptions;
using ODK.Services.Imaging;

namespace ODK.Services.Media;

public class MediaAdminService : OdkAdminServiceBase, IMediaAdminService
{
    private readonly IImageService _imageService;
    private readonly IMediaFileProvider _mediaFileProvider;

    public MediaAdminService(IUnitOfWork unitOfWork, 
        IMediaFileProvider mediaFileProvider,
        IImageService imageService)
        : base(unitOfWork)
    {
        _imageService = imageService;
        _mediaFileProvider = mediaFileProvider;
    }

    public async Task<IReadOnlyCollection<MediaFile>> DeleteMediaFile(Guid currentMemberId, Guid chapterId, string name)
    {
        await AssertMemberIsChapterAdmin(currentMemberId, chapterId);

        MediaFile? mediaFile = await _mediaFileProvider.GetMediaFile(chapterId, name);
        if (mediaFile == null)
        {
            throw new OdkServiceException("File not found");
        }

        File.Delete(mediaFile.FilePath);

        return await _mediaFileProvider.GetMediaFiles(chapterId);
    }

    public async Task<IReadOnlyCollection<MediaFile>> GetMediaFiles(Guid currentMemberId, Guid chapterId)
    {
        await AssertMemberIsChapterAdmin(currentMemberId, chapterId);

        return await _mediaFileProvider.GetMediaFiles(chapterId);
    }

    public async Task<MediaFile?> SaveMediaFile(Guid currentMemberId, Guid chapterId, string name, byte[] data)
    {
        await AssertMemberIsChapterAdmin(currentMemberId, chapterId);

        if (!_imageService.IsImage(data))
        {
            throw new OdkServiceException("File must be an image");
        }

        var filePath = await _mediaFileProvider.GetMediaFilePath(chapterId, name);
        if (string.IsNullOrEmpty(filePath))
        {
            throw new OdkServiceException("File not found");
        }

        int version = 1;
        while (File.Exists(filePath))
        {
            var file = new FileInfo(filePath);
            var versionedFileName = $"{file.Name.Substring(0, file.Name.Length - file.Extension.Length)}{++version}{file.Extension}";
            filePath = Path.Combine(file.Directory!.FullName, versionedFileName);
        }

        await File.WriteAllBytesAsync(filePath, data);

        return await _mediaFileProvider.GetMediaFile(chapterId, new FileInfo(filePath).Name);
    }
}
