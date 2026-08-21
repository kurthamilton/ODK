using ODK.Core.Countries;

namespace ODK.Data.Core.Countries;

/// <summary>
/// A currency together with the country it belongs to: the country referencing it, or the codes the currency
/// carries itself where no country does. A currency no country owns has no name to show, only its codes.
/// </summary>
public class CurrencyDto
{
    public required string? CountryIsoCode2 { get; init; }

    public required string? CountryIsoCode3 { get; init; }

    public required string? CountryName { get; init; }

    public required Currency Currency { get; init; }
}
