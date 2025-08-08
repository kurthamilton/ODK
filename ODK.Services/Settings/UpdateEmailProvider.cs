namespace ODK.Services.Settings;

public class UpdateEmailProvider
{
    public required int? BatchSize { get; set; }

    public required int DailyLimit { get; set; }

    public required string Name { get; set; }

    public required string SmtpLogin { get; set; }

    public required string SmtpPassword { get; set; }

    public required int SmtpPort { get; set; }

    public required string SmtpServer { get; set; }
}
