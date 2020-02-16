using System;
using System.Threading.Tasks;

namespace ODK.Services.SocialMedia
{
    public interface ISocialMediaAdminService
    {
        Task InstagramLogin(Guid currentUserId, Guid chapterId);

        Task InstagramTriggerAccountVerification(Guid currentUserId, Guid chapterId);

        Task InstagramVerifyAccount(Guid currentUserId, Guid chapterId, string code);
    }
}
