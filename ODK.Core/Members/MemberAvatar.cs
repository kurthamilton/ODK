using ODK.Core.Images;
using System.Reflection;

namespace ODK.Core.Members;

public class MemberAvatar : IVersioned
{
    public int CropHeight { get; set; }

    public byte[] ImageData { get; set; } = [];

    public Guid MemberId { get; set; }

    public string MimeType { get; set; } = "";

    public byte[] Version { get; set; } = [];

    public int CropWidth { get; set; }

    public int CropX { get; set; }
    public int CropY { get; set; }

    public string ToDataUrl() => ImageHelper.ToDataUrl(ImageData, MimeType);
}
