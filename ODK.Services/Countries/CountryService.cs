﻿using ODK.Core.Countries;
using ODK.Data.Core;

namespace ODK.Services.Countries;

public class CountryService : ICountryService
{
    private readonly IUnitOfWork _unitOfWork;

    public CountryService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public Task<Country> GetCountry(Guid countryId) =>
        _unitOfWork.CountryRepository.GetById(countryId).RunAsync();
}
