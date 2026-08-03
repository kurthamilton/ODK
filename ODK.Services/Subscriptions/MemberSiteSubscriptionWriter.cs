using ODK.Core.Members;
using ODK.Core.Platforms;
using ODK.Data.Core;

namespace ODK.Services.Subscriptions;

public class MemberSiteSubscriptionWriter : IMemberSiteSubscriptionWriter
{
    private readonly IUnitOfWork _unitOfWork;

    public MemberSiteSubscriptionWriter(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task MakeRecordCurrent(MemberSiteSubscriptionRecord newRecord, PlatformType platform)
    {
        var memberId = newRecord.MemberId!.Value;

        var (existingCurrent, existingSnapshot) = await _unitOfWork.RunAsync(
            x => x.MemberSiteSubscriptionRecordRepository.Query().Current().ForMember(memberId).GetSingleOrDefault(),
            x => x.MemberSiteSubscriptionRepository.GetByMemberId(memberId, platform));

        MakeRecordCurrent(newRecord, existingCurrent, existingSnapshot);
    }

    public void MakeRecordCurrent(
        MemberSiteSubscriptionRecord newRecord,
        MemberSiteSubscriptionRecord? existingCurrent,
        MemberSiteSubscription? existingSnapshot)
    {
        newRecord.IsCurrent = true;
        _unitOfWork.MemberSiteSubscriptionRecordRepository.Add(newRecord);

        if (existingCurrent != null && !ReferenceEquals(existingCurrent, newRecord))
        {
            existingCurrent.IsCurrent = false;
            _unitOfWork.MemberSiteSubscriptionRecordRepository.Update(existingCurrent);
        }

        var snapshot = existingSnapshot ?? new MemberSiteSubscription
        {
            MemberId = newRecord.MemberId!.Value
        };
        snapshot.ExpiresUtc = newRecord.ExpiresUtc;
        snapshot.ExternalId = newRecord.ExternalId;
        snapshot.SiteSubscriptionId = newRecord.SiteSubscriptionId;
        snapshot.SiteSubscriptionPriceId = newRecord.SiteSubscriptionPriceId;

        if (existingSnapshot == null)
        {
            _unitOfWork.MemberSiteSubscriptionRepository.Add(snapshot);
        }
        else
        {
            _unitOfWork.MemberSiteSubscriptionRepository.Update(snapshot);
        }
    }
}
