using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ODK.Services.SocialMedia
{
    public interface IInstagramService
    {
        Task<IReadOnlyCollection<SocialMediaImage>> FetchInstagramImages(Guid chapterId);

        Task Login();

        Task SendVerifyCode(string code);

        Task TriggerVerifyCode();
    }
}
