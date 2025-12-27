namespace ODK.Core.SocialMedia;

public class InstagramImage : IVersioned
{
    public byte[] ImageData { get; set; } = [];

    public Guid InstagramPostId { get; set; }

    public string MimeType { get; set; } = string.Empty;

    public byte[] Version { get; set; } = [];
}
