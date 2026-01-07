using ODK.Core.Countries;
using ODK.Data.Core;

namespace ODK.Services.Payments;

public class CurrencyService : ICurrencyService
{
    private readonly IUnitOfWork _unitOfWork;

    public CurrencyService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyCollection<Currency>> GetCurrencies()
    {
        return await _unitOfWork.CurrencyRepository.GetAll().Run();
    }
}
