namespace ODK.Services.SocialMedia;

public class SocialMediaService : ISocialMediaService
{
    private readonly SocialMediaServiceSettings _settings;

    public SocialMediaService(SocialMediaServiceSettings settings)
    {
        _settings = settings;
    }

    public string GetWhatsAppLink(string groupId)
    {
        return _settings.WhatsAppUrlFormat.Replace("{groupid}", groupId);
    }
}