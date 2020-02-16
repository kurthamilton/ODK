using System;
using System.Threading.Tasks;
using ODK.Core.Chapters;

namespace ODK.Services.SocialMedia
{
    public class SocialMediaAdminService : OdkAdminServiceBase, ISocialMediaAdminService
    {
        private readonly IInstagramService _instagramService;

        public SocialMediaAdminService(IChapterRepository chapterRepository, IInstagramService instagramService)
            : base(chapterRepository)
        {
            _instagramService = instagramService;
        }

        public async Task InstagramLogin(Guid currentUserId, Guid chapterId)
        {
            await AssertMemberIsChapterSuperAdmin(currentUserId, chapterId);

            await _instagramService.Login();
        }

        public async Task InstagramTriggerAccountVerification(Guid currentUserId, Guid chapterId)
        {
            await AssertMemberIsChapterSuperAdmin(currentUserId, chapterId);

            await _instagramService.TriggerVerifyCode();
        }

        public async Task InstagramVerifyAccount(Guid currentUserId, Guid chapterId, string code)
        {
            await AssertMemberIsChapterSuperAdmin(currentUserId, chapterId);

            await _instagramService.SendVerifyCode(code);
        }
    }
}
