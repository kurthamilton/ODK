namespace ODK.Core.SocialMedia;

public class InstagramImage
{
    public byte[] ImageData { get; set; } = [];

    public Guid InstagramPostId { get; set; }

    public string MimeType { get; set; } = "";
}
