namespace ODK.Services.Localization;

public interface ILocaleService
{
    /// <summary>
    /// The effective .NET short-date pattern (e.g. "dd/MM/yyyy") for the given member, resolved as
    /// member preference -> member's country default -> app default. Pass null for an anonymous request.
    /// </summary>
    Task<string> GetShortDatePattern(Guid? memberId);
}
