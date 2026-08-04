using System.Globalization;

namespace ODK.Services.Members;

public interface IMemberLocaleService
{
    /// <summary>The member's stored formatting culture, or the default when they have no stored locale.</summary>
    Task<CultureInfo> GetCulture(Guid memberId);

    /// <summary>
    /// The stored formatting culture for each member id (default where none is stored). For formatting
    /// request-independent output (e.g. bulk emails) per recipient without an N+1.
    /// </summary>
    Task<IReadOnlyDictionary<Guid, CultureInfo>> GetCultures(IReadOnlyCollection<Guid> memberIds);

    /// <summary>Stores the member's locale (no-op when already equal). Invoked from a background job.</summary>
    Task UpdateLocale(Guid memberId, string locale);
}
