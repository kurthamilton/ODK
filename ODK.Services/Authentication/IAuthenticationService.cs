using System.Security.Claims;
using ODK.Core.Chapters;
using ODK.Core.Members;

namespace ODK.Services.Authentication;

public interface IAuthenticationService
{
    Task<ServiceResult> ActivateChapterAccount(
        IChapterServiceRequest request,
        string activationToken,
        string password);

    Task<ServiceResult> ActivateSiteAccount(
        IServiceRequest request,
        string activationToken,
        string password);

    Task<ServiceResult> ChangePassword(Guid memberId, string currentPassword, string newPassword);

    Task<IReadOnlyCollection<Claim>> GetClaims(IMemberServiceRequest request);

    Task<Member?> GetMember(string username, string password);

    Task<ServiceResult> RequestPasswordReset(
        IServiceRequest request,
        Chapter? chapter,
        string emailAddress);

    Task<ServiceResult> RequestPasswordReset(
        IServiceRequest request,
        string emailAddress);

    Task<ServiceResult> ResetPassword(string token, string password);
}