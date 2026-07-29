using ODK.Core.Members;
using ODK.Data.Core;

namespace ODK.Services.Subscriptions;

public class MemberChapterSubscriptionWriter : IMemberChapterSubscriptionWriter
{
    private readonly IUnitOfWork _unitOfWork;

    public MemberChapterSubscriptionWriter(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public void MakeRecordCurrent(
        MemberChapter memberChapter,
        MemberSubscriptionRecord newRecord,
        MemberSubscriptionRecord? existingCurrent,
        MemberSubscription? existingSnapshot)
    {
        newRecord.IsCurrent = true;
        _unitOfWork.MemberSubscriptionRecordRepository.Add(newRecord);

        if (existingCurrent != null && !ReferenceEquals(existingCurrent, newRecord))
        {
            existingCurrent.IsCurrent = false;
            _unitOfWork.MemberSubscriptionRecordRepository.Update(existingCurrent);
        }

        var snapshot = existingSnapshot ?? new MemberSubscription
        {
            MemberChapterId = memberChapter.Id
        };
        snapshot.ExpiresUtc = newRecord.ExpiresUtc;
        snapshot.Type = newRecord.Type;

        if (existingSnapshot == null)
        {
            _unitOfWork.MemberSubscriptionRepository.Add(snapshot);
        }
        else
        {
            _unitOfWork.MemberSubscriptionRepository.Update(snapshot);
        }
    }
}
