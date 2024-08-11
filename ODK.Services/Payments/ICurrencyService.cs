using ODK.Core.Countries;

namespace ODK.Services.Payments;

public interface ICurrencyService
{
    Task<IReadOnlyCollection<Currency>> GetCurrencies();
}
