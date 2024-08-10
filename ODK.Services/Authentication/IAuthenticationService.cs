using System.Security.Claims;
using ODK.Core.Members;

namespace ODK.Services.Authentication;

public interface IAuthenticationService
{
    Task<ServiceResult> ActivateAccountAsync(string activationToken, string password);

    Task<ServiceResult> ActivateChapterAccountAsync(Guid chapterId, string activationToken, string password);

    Task<ServiceResult> ChangePasswordAsync(Guid memberId, string currentPassword, string newPassword);
    
    Task<IReadOnlyCollection<Claim>> GetClaimsAsync(Member member);

    Task<Member?> GetMemberAsync(string username, string password);
    
    Task<ServiceResult> RequestPasswordResetAsync(Guid chapterId, string emailAddress);
    
    Task<ServiceResult> RequestPasswordResetAsync(string emailAddress);

    Task<ServiceResult> ResetPasswordAsync(string token, string password);
}
