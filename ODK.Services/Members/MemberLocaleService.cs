using System.Globalization;
using ODK.Core.Members;
using ODK.Core.Utils;
using ODK.Data.Core;
using ODK.Data.Core.Deferred;

namespace ODK.Services.Members;

public class MemberLocaleService : IMemberLocaleService
{
    private readonly IUnitOfWork _unitOfWork;

    public MemberLocaleService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<CultureInfo> GetCulture(Guid memberId)
    {
        var preferences = await _unitOfWork.MemberPreferencesRepository.GetByMemberIdOrDefault(memberId).Run();
        return LocaleUtils.GetCultureOrDefault(preferences?.Locale);
    }

    public async Task<IReadOnlyDictionary<Guid, CultureInfo>> GetCultures(IReadOnlyCollection<Guid> memberIds)
    {
        var distinctIds = memberIds.Distinct().ToArray();
        if (distinctIds.Length == 0)
        {
            return new Dictionary<Guid, CultureInfo>();
        }

        var preferences = await _unitOfWork.MemberPreferencesRepository.GetByMemberIds(distinctIds).Run();
        var localesByMemberId = preferences.ToDictionary(x => x.MemberId, x => x.Locale);

        return distinctIds.ToDictionary(
            memberId => memberId,
            memberId => LocaleUtils.GetCultureOrDefault(localesByMemberId.GetValueOrDefault(memberId)));
    }

    public async Task UpdateLocale(Guid memberId, string locale)
    {
        var preferences = await _unitOfWork.MemberPreferencesRepository.GetByMemberIdOrDefault(memberId).Run();
        if (preferences?.Locale == locale)
        {
            return;
        }

        preferences ??= new MemberPreferences();
        preferences.Locale = locale;
        _unitOfWork.MemberPreferencesRepository.Upsert(preferences, memberId);

        await _unitOfWork.SaveChanges();
    }
}
