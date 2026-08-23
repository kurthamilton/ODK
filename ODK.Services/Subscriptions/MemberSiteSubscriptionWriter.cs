using ODK.Core.Members;
using ODK.Data.Core;

namespace ODK.Services.Subscriptions;

public class MemberSiteSubscriptionWriter : IMemberSiteSubscriptionWriter
{
    private readonly IUnitOfWork _unitOfWork;

    public MemberSiteSubscriptionWriter(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task MakeRecordCurrent(MemberSiteSubscriptionRecord newRecord)
    {
        var existingCurrent = await _unitOfWork.MemberSiteSubscriptionRecordRepository
            .Query()
            .Current()
            .ForMember(newRecord.MemberId)
            .GetSingleOrDefault()
            .Run();

        MakeRecordCurrent(newRecord, existingCurrent);
    }

    public void MakeRecordCurrent(MemberSiteSubscriptionRecord newRecord, MemberSiteSubscriptionRecord? existingCurrent)
    {
        newRecord.IsCurrent = true;
        _unitOfWork.MemberSiteSubscriptionRecordRepository.Add(newRecord);

        if (existingCurrent != null && !ReferenceEquals(existingCurrent, newRecord))
        {
            existingCurrent.IsCurrent = false;
            _unitOfWork.MemberSiteSubscriptionRecordRepository.Update(existingCurrent);
        }
    }
}
