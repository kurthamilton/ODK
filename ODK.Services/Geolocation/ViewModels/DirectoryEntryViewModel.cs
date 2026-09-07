namespace ODK.Services.Geolocation.ViewModels;

public class DirectoryEntryViewModel
{
    public required bool IsDirectory { get; init; }

    public required DateTime? LastModifiedUtc { get; init; }

    public required string Name { get; init; }

    public required string Path { get; init; }

    public required long? SizeBytes { get; init; }
}
