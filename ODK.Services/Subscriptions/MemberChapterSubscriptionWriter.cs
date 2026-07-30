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
        MemberSubscriptionRecord newRecord,
        MemberSubscriptionRecord? existingCurrent)
    {
        newRecord.IsCurrent = true;
        _unitOfWork.MemberSubscriptionRecordRepository.Add(newRecord);

        if (existingCurrent != null && !ReferenceEquals(existingCurrent, newRecord))
        {
            existingCurrent.IsCurrent = false;
            _unitOfWork.MemberSubscriptionRecordRepository.Update(existingCurrent);
        }
    }
}
