namespace ODK.Core.SocialMedia;

public class InstagramImage : IDatabaseEntity, IVersioned
{
    public int? DisplayOrder { get; set; }

    public string? ExternalId { get; set; }

    public Guid Id { get; set; }

    public byte[] ImageData { get; set; } = [];

    public Guid InstagramPostId { get; set; }

    public string MimeType { get; set; } = string.Empty;

    public byte[] Version { get; set; } = [];
}
