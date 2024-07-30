namespace ODK.Services.Settings;

public class UpdateEmailProvider
{
    public int? BatchSize { get; set; }

    public int DailyLimit { get; set; }

    public string SmtpLogin { get; set; } = "";

    public string SmtpPassword { get; set; } = "";

    public int SmtpPort { get; set; }

    public string SmtpServer { get; set; } = "";
}
