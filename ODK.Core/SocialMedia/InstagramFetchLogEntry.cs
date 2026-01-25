namespace ODK.Core.SocialMedia;

public class InstagramFetchLogEntry : IDatabaseEntity
{
    public DateTime CreatedUtc { get; set; }

    public int DelaySeconds { get; set; }

    public Guid Id { get; set; }

    public bool Success { get; set; }

    public required string Username { get; set; }
}