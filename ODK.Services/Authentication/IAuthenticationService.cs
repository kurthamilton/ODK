using System.Security.Claims;
using ODK.Core.Chapters;
using ODK.Core.Members;

namespace ODK.Services.Authentication;

public interface IAuthenticationService
{
    Task<ServiceResult> ActivateChapterAccountAsync(
        IChapterServiceRequest request,
        string activationToken,
        string password);

    Task<ServiceResult> ActivateSiteAccountAsync(
        IServiceRequest request,
        string activationToken,
        string password);

    Task<ServiceResult> ChangePasswordAsync(Guid memberId, string currentPassword, string newPassword);

    Task<IReadOnlyCollection<Claim>> GetClaimsAsync(IMemberServiceRequest request);

    Task<Member?> GetMemberAsync(string username, string password);

    Task<ServiceResult> RequestPasswordResetAsync(
        IServiceRequest request,
        Chapter? chapter,
        string emailAddress);

    Task<ServiceResult> RequestPasswordResetAsync(
        IServiceRequest request,
        string emailAddress);

    Task<ServiceResult> ResetPasswordAsync(string token, string password);
}