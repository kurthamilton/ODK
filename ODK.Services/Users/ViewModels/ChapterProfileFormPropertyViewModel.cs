namespace ODK.Services.Users.ViewModels;

public class ChapterProfileFormPropertyViewModel
{
    public Guid ChapterPropertyId { get; set; }

    public string? OtherValue { get; set; } = string.Empty;

    public string? Value { get; set; } = string.Empty;
}
