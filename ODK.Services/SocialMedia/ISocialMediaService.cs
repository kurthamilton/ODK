using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ODK.Services.SocialMedia
{
    public interface ISocialMediaService
    {
        Task<IReadOnlyCollection<SocialMediaImage>> GetLatestInstagramImages(Guid chapterId);        
    }
}
