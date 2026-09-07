namespace ODK.Services.Geolocation.ViewModels;

public class IpLocationDatabaseFileViewModel
{
    public required DateTime LastModifiedUtc { get; init; }

    public required bool Loaded { get; init; }

    public required string Name { get; init; }

    public required long SizeBytes { get; init; }
}
