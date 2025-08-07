﻿namespace ODK.Core.Emails;

public class EmailProvider : IDatabaseEntity
{
    public int? BatchSize { get; set; }

    public int DailyLimit { get; set; }

    public Guid Id { get; set; }

    public int Order { get; set; }

    public string SmtpLogin { get; set; } = "";

    public string SmtpPassword { get; set; } = "";

    public int SmtpPort { get; set; }

    public string SmtpServer { get; set; } = "";   
    
    public bool IsValid()
    {
        if (string.IsNullOrWhiteSpace(SmtpLogin) ||
            string.IsNullOrWhiteSpace(SmtpPassword) ||
            SmtpPort == 0 ||
            DailyLimit <= 0 ||
            BatchSize <= 0)
        {
            return false;
        }

        return true;
    }
}
