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

    /// <summary>
    /// The claims for a member's auth cookie. <paramref name="signedInMemberIds"/> carries the members
    /// already signed in on that cookie, so switching between them keeps the list intact; a fresh sign-in
    /// passes none and the cookie ends up holding that member alone.
    /// </summary>
    Task<IReadOnlyCollection<Claim>> GetClaims(
        IMemberServiceRequest request,
        IReadOnlyCollection<Guid> signedInMemberIds);

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