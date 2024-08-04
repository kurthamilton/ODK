namespace ODK.Services.Users.ViewModels;

public class ProfileFormPropertyViewModel
{
    public Guid ChapterPropertyId { get; set; }

    public string? OtherValue { get; set; } = "";

    public string? Value { get; set; } = "";
}
