namespace ODK.Core.Messages;

public class SiteContactMessage : IDatabaseEntity
{
    public DateTime CreatedUtc { get; set; }

    public string FromAddress { get; set; } = "";

    public Guid Id { get; set; }

    public string Message { get; set; } = "";

    public double? RecaptchaScore { get; set; }

    public DateTime? RepliedUtc { get; set; }
}
