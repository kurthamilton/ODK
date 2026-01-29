using System.Security.Claims;
using ODK.Core.Chapters;
using ODK.Core.Members;

namespace ODK.Services.Authentication;

public interface IAuthenticationService
{
    Task<ServiceResult> ActivateChapterAccountAsync(
        ServiceRequest request,
        Chapter chapter,
        string activationToken,
        string password);

    Task<ServiceResult> ActivateSiteAccountAsync(
        ServiceRequest request,
        string activationToken,
        string password);

    Task<ServiceResult> ChangePasswordAsync(Guid memberId, string currentPassword, string newPassword);

    Task<IReadOnlyCollection<Claim>> GetClaimsAsync(Member member);

    Task<Member?> GetMemberAsync(string username, string password);

    Task<ServiceResult> RequestPasswordResetAsync(
        ServiceRequest request,
        Guid chapterId,
        string emailAddress);

    Task<ServiceResult> RequestPasswordResetAsync(
        ServiceRequest request,
        string emailAddress);

    Task<ServiceResult> ResetPasswordAsync(string token, string password);
}
