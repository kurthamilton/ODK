﻿namespace ODK.Core.SocialMedia;

public class InstagramPost : IDatabaseEntity
{
    public string? Caption { get; set; }

    public Guid ChapterId { get; set; }

    public DateTime Date { get; set; }

    public string ExternalId { get; set; } = "";

    public Guid Id { get; set; }

    public string Url { get; set; } = "";
}
