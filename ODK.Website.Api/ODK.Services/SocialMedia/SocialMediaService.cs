using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InstaSharper.API;
using InstaSharper.API.Builder;
using InstaSharper.Classes;
using InstaSharper.Classes.Models;
using ODK.Core.Chapters;
using ODK.Core.Settings;
using ODK.Services.Exceptions;

namespace ODK.Services.SocialMedia
{
    public class SocialMediaService : ISocialMediaService
    {
        private readonly IChapterRepository _chapterRepository;
        private readonly ISettingsRepository _settingsRepository;

        public SocialMediaService(IChapterRepository chapterRepository, ISettingsRepository settingsRepository)
        {
            _chapterRepository = chapterRepository;
            _settingsRepository = settingsRepository;
        }

        public async Task<IReadOnlyCollection<SocialMediaImage>> GetLatestInstagramImages(Guid chapterId)
        {
            ChapterLinks links = await _chapterRepository.GetChapterLinks(chapterId);
            if (links == null)
            {
                throw new OdkNotFoundException();
            }

            IInstaApi api = await OpenApi();

            PaginationParameters paginationParameters = PaginationParameters.MaxPagesToLoad(1);
            IResult<InstaMediaList> media = await api.GetUserMediaAsync(links.InstagramName, paginationParameters);

            return media.Value
                .OrderByDescending(x => x.DeviceTimeStamp)
                .Where(x => x.Images.Count > 0)
                .Select(x => new SocialMediaImage
                {
                    Caption = x.Caption.Text,
                    ImageUrl = x.Images.OrderBy(img => img.Width).First().URI,
                    Url = $"https://www.instagram.com/p/{x.Code}"
                })
                .ToArray();
        }

        private async Task<IInstaApi> OpenApi()
        {
            SiteSettings siteSettings = await _settingsRepository.GetSiteSettings();

            UserSessionData user = new UserSessionData
            {
                UserName = siteSettings.InstagramUsername,
                Password = siteSettings.InstagramPassword
            };

            IInstaApi api = InstaApiBuilder.CreateBuilder()
                .SetUser(user)
                .Build();

            IResult<InstaLoginResult> loginResult = await api.LoginAsync();
            if (!loginResult.Succeeded)
            {
                throw new OdkServiceException($"Instagram login failed for user {user.UserName}");
            }

            return api;
        }
    }
}
