namespace ODK.Core.SocialMedia;

public class InstagramImage : IDatabaseEntity, IVersioned
{
    public string? Alt { get; set; }

    public int? DisplayOrder { get; set; }

    public string? ExternalId { get; set; }

    public int? Height { get; set; }

    public Guid Id { get; set; }

    public byte[] ImageData { get; set; } = [];

    public Guid InstagramPostId { get; set; }

    public bool IsVideo { get; set; }

    public string MimeType { get; set; } = string.Empty;

    public byte[] Version { get; set; } = [];

    public int VersionInt { get; set; }

    public int? Width { get; set; }
}