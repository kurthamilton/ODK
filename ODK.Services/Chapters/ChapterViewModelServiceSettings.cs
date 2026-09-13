namespace ODK.Services.Chapters;

public class ChapterViewModelServiceSettings
{
    /// <summary>
    /// How long a group counts as newly arrived (Groups:MigrationWindowDays) - here, how many days after a
    /// move its home page still announces it.
    /// </summary>
    public required int MigrationWindowDays { get; init; }
}
