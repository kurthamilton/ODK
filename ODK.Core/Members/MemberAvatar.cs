namespace ODK.Core.Members;

public class MemberAvatar : IVersioned
{
    public int Height { get; set; }

    public byte[] ImageData { get; set; } = [];

    public Guid MemberId { get; set; }

    public string MimeType { get; set; } = "";

    public byte[] Version { get; set; } = [];

    public int Width { get; set; }

    public int X { get; set; }

    public int Y { get; set; }
}
