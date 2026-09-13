namespace ODK.Services.Chapters;

public class ChapterViewModelServiceSettings
{
    /// <summary>
    /// How many days after a move a group's home page still announces it.
    /// </summary>
    public required int MovedBannerDays { get; init; }
}
