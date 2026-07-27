using Microsoft.EntityFrameworkCore;
using ODK.Core.Chapters;
using ODK.Core.Countries;
using ODK.Core.Members;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Extensions;

namespace ODK.Data.EntityFramework.Repositories;

public class CurrencyRepository : ReadWriteRepositoryBase<Currency>, ICurrencyRepository
{
    public CurrencyRepository(DbContext context)
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
        => ChapterQuery(chapterId).DeferredSingle();

    public IDeferredQuerySingleOrDefault<Currency> GetByChapterIdOrDefault(Guid chapterId)
        => ChapterQuery(chapterId).DeferredSingleOrDefault();

    public IDeferredQuerySingle<Currency> GetByCountryId(Guid countryId)
    {
        var query =
            from currency in Set()
            from country in Set<Country>()
                .Where(x => x.CurrencyId == currency.Id)
            where country.Id == countryId
            select currency;

        return query.DeferredSingle();
    }

    public IDeferredQuerySingleOrDefault<Currency> GetByMemberIdOrDefault(Guid memberId)
        => MemberQuery(memberId).DeferredSingleOrDefault();

    private IQueryable<Currency> ChapterQuery(Guid chapterId)
        =>
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
            where chapter.Id == chapterId
            select chapterPaymentSettingsCurrency ?? countryCurrency;

    private IQueryable<Currency> MemberQuery(Guid memberId)
        =>
            // Anchor on the member and left-join everything else: the member's chosen currency
            // (MemberPaymentSettings.CurrencyId) must be returned even when they have no location/country,
            // falling back to the location country's currency when they haven't chosen one.
            from member in Set<Member>()
                .Where(x => x.Id == memberId)
            from memberPaymentSettings in Set<MemberPaymentSettings>()
                .Where(x => x.MemberId == memberId)
                .DefaultIfEmpty()
            from memberPaymentSettingsCurrency in Set()
                .Where(x => x.Id == memberPaymentSettings.CurrencyId)
                .DefaultIfEmpty()
            from memberLocation in Set<MemberLocation>()
                .Where(x => x.MemberId == memberId)
                .DefaultIfEmpty()
            from country in Set<Country>()
                .Where(x => x.Id == memberLocation.CountryId)
                .DefaultIfEmpty()
            from countryCurrency in Set()
                .Where(x => x.Id == country.CurrencyId)
                .DefaultIfEmpty()
            select memberPaymentSettingsCurrency ?? countryCurrency;
}