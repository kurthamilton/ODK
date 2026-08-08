namespace ODK.Services.Referrals;

public interface IReferralService
{
    Task<ServiceResult> CreateReferral(IMemberServiceRequest request, string emailAddress);

}
