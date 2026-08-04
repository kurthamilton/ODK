namespace ODK.Services.Localization;

public interface ILocaleService
{
    /// <summary>
    /// The effective .NET short-date pattern (e.g. "dd/MM/yyyy") for a viewer,
    /// resolved as member preference -> request preference -> app default
    /// </summary>
    string GetShortDatePattern(IServiceRequest request);
}
