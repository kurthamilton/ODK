using ODK.Core.Countries;
using ODK.Core.Members;

namespace ODK.Services.Localization;

public interface ILocaleService
{
    /// <summary>
    /// The effective .NET short-date pattern (e.g. "dd/MM/yyyy") for a viewer, resolved as member
    /// preference -> the member's country default -> app default. Callers pass the request-loaded member
    /// preferences and country (both null for an anonymous request).
    /// </summary>
    string GetShortDatePattern(MemberPreferences? preferences, Country? country);
}
