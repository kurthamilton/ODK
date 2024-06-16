namespace ODK.Core.Chapters;

public class Chapter
{
    public Chapter(Guid id, Guid countryId, string name, string? bannerImageUrl, string? welcomeText,
        string? redirectUrl, int displayOrder)
    {
        BannerImageUrl = bannerImageUrl;
        CountryId = countryId;
        DisplayOrder = displayOrder;
        Id = id;
        Name = name;
        RedirectUrl = redirectUrl;
        WelcomeText = welcomeText;
    }

    public string? BannerImageUrl { get; }

    public Guid CountryId { get; }

    public int DisplayOrder { get; }

    public Guid Id { get; }

    public string Name { get; }

    public string? RedirectUrl { get; }

    public string? WelcomeText { get; private set; }

    public void Update(string welcomeText)
    {
        WelcomeText = welcomeText;
    }
}
