using ODK.Core.Countries;

namespace ODK.Services.Integrations.Geolocation;

public interface IIpLocationLookup
{
    string? CurrentDatabase { get; }

    bool DatabaseAvailable { get; }

    Location? Find(string ipAddress);

    void Reload();
}
