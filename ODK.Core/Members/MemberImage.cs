namespace ODK.Core.Members;

public class MemberImage : IVersioned
{
    public MemberImage(Guid memberId, byte[] imageData, string mimeType, long version)
    {
        ImageData = imageData;
        MemberId = memberId;
        MimeType = mimeType;
        Version = version;
    }

    public byte[] ImageData { get; }

    public Guid MemberId { get; }

    public string MimeType { get; }

    public long Version { get; }
}
