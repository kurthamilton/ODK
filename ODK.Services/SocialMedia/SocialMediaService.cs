using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ODK.Services.Caching;

namespace ODK.Services.SocialMedia
{
    public class SocialMediaService : ISocialMediaService
    {
        private readonly ICacheService _cacheService;
        private readonly IInstagramService _instagramService;

        public SocialMediaService(ICacheService cacheService, IInstagramService instagramService)
        {
            _cacheService = cacheService;
            _instagramService = instagramService;
        }

        public async Task<IReadOnlyCollection<SocialMediaImage>> GetLatestInstagramImages(Guid chapterId)
        {
            return await _cacheService.GetOrSetItem(() => _instagramService.FetchInstagramImages(chapterId),
                chapterId, TimeSpan.FromMinutes(60));
        }
    }
}
