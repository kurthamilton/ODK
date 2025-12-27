namespace ODK.Core.Messages;

public class SiteContactMessage : IDatabaseEntity
{
    public DateTime CreatedUtc { get; set; }

    public string FromAddress { get; set; } = string.Empty;

    public Guid Id { get; set; }

    public string Message { get; set; } = string.Empty;

    public double? RecaptchaScore { get; set; }

    public DateTime? RepliedUtc { get; set; }
}
