namespace ODK.Services.Geolocation.ViewModels;

public class IpLocationDatabaseViewModel
{
    public required string Directory { get; init; }

    public required IReadOnlyCollection<IpLocationDatabaseFileViewModel> Files { get; init; }

    public required bool Preloaded { get; init; }
}
