namespace ODK.Core.Members;

public class MemberImage : IVersioned
{
    public const string DefaultMimeType = "image/webp";

    public byte[] ImageData { get; set; } = [];

    public Guid MemberId { get; set; }

    public string MimeType { get; set; } = string.Empty;

    public byte[] Version { get; set; } = [];
}
