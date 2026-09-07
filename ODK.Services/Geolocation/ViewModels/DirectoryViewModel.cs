namespace ODK.Services.Geolocation.ViewModels;

public class DirectoryViewModel
{
    public required IReadOnlyCollection<DirectoryEntryViewModel> Entries { get; init; }

    /// <summary>
    /// Why the directory could not be listed, which for this page is the answer rather than a fault.
    /// </summary>
    public required string? Error { get; init; }

    public required bool Exists { get; init; }

    /// <summary>
    /// The account the application runs as - the app pool identity under IIS, which is what a folder's
    /// permissions have to name.
    /// </summary>
    public required string Identity { get; init; }

    public required string? ParentPath { get; init; }

    public required string Path { get; init; }
}
