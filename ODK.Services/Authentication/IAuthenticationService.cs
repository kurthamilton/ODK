using ODK.Core.Members;
using ODK.Core.Web;
using System.Security.Claims;

namespace ODK.Services.Authentication;

public interface IAuthenticationService
{
    Task<ServiceResult> ActivateChapterAccountAsync(
        IHttpRequestContext httpRequestContext, 
        Guid chapterId, 
        string activationToken, 
        string password);

    Task<ServiceResult> ActivateSiteAccountAsync(
        IHttpRequestContext httpRequestContext, 
        string activationToken, 
        string password);    

    Task<ServiceResult> ChangePasswordAsync(Guid memberId, string currentPassword, string newPassword);
    
    Task<IReadOnlyCollection<Claim>> GetClaimsAsync(Member member);

    Task<Member?> GetMemberAsync(string username, string password);
    
    Task<ServiceResult> RequestPasswordResetAsync(
        IHttpRequestContext httpRequestContext, 
        Guid chapterId, 
        string emailAddress);
    
    Task<ServiceResult> RequestPasswordResetAsync(
        IHttpRequestContext httpRequestContext, 
        string emailAddress);

    Task<ServiceResult> ResetPasswordAsync(string token, string password);
}
