using ODK.Core.Chapters;
using ODK.Core.Countries;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.Repositories;

public class CurrencyRepository : ReadWriteRepositoryBase<Currency>, ICurrencyRepository
{
    public CurrencyRepository(OdkContext context)
        : base(context)
    {
    }

    public IDeferredQueryMultiple<Currency> GetAll() => Set()
        .DeferredMultiple();

    public IDeferredQuerySingleOrDefault<Currency> GetByCode(string code)
        => Set()
            .Where(x => x.Code == code)
            .DeferredSingleOrDefault();

    public IDeferredQuerySingle<Currency> GetByChapterId(Guid chapterId)
    {
        var query =
            from chapter in Set<Chapter>()
            from chapterPaymentSettings in Set<ChapterPaymentSettings>()
                .Where(x => x.ChapterId == chapter.Id)
                .DefaultIfEmpty()
            from chapterPaymentSettingsCurrency in Set()
                .Where(x => x.Id == chapterPaymentSettings.CurrencyId)
                .DefaultIfEmpty()
            from country in Set<Country>()
                .Where(x => x.Id == chapter.CountryId)
            from countryCurrency in Set()
                .Where(x => x.Id == country.CurrencyId)
            select chapterPaymentSettingsCurrency ?? countryCurrency;

        return query.DeferredSingle();
    }

    public IDeferredQuerySingle<Currency> GetByCountryId(Guid countryId)
    {
        var query =
            from currency in Set()
            from country in Set<Country>()
                .Where(x => x.CurrencyId == currency.Id)
            select currency;

        return query.DeferredSingle();
    }
}