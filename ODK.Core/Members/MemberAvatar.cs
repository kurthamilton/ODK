using ODK.Core.Images;

namespace ODK.Core.Members;

public class MemberAvatar : IVersioned
{
    public int CropX { get; set; }

    public int CropY { get; set; }

    public int CropWidth { get; set; }

    public int CropHeight { get; set; }

    public byte[] ImageData { get; set; } = [];

    public Guid MemberId { get; set; }

    public string MimeType { get; set; } = string.Empty;

    public byte[] Version { get; set; } = [];

    public int VersionInt { get; set; }

    public string ToDataUrl() => ImageHelper.ToDataUrl(ImageData, MimeType);
}