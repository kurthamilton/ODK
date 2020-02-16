using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InstaSharper.API;
using InstaSharper.API.Builder;
using InstaSharper.Classes;
using InstaSharper.Classes.Models;
using InstaSharper.Classes.ResponseWrappers;
using ODK.Core.Chapters;
using ODK.Core.Settings;
using ODK.Services.Exceptions;

namespace ODK.Services.SocialMedia
{
    public class InstagramService : IInstagramService
    {
        private readonly IChapterRepository _chapterRepository;
        private readonly ISettingsRepository _settingsRepository;

        public InstagramService(IChapterRepository chapterRepository, ISettingsRepository settingsRepository)
        {
            _chapterRepository = chapterRepository;
            _settingsRepository = settingsRepository;
        }

        public async Task<IReadOnlyCollection<SocialMediaImage>> FetchInstagramImages(Guid chapterId)
        {
            ChapterLinks links = await _chapterRepository.GetChapterLinks(chapterId);
            if (links == null)
            {
                throw new OdkNotFoundException();
            }

            if (string.IsNullOrEmpty(links.InstagramName))
            {
                return new SocialMediaImage[0];
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

        public async Task Login()
        {
            await OpenApi();
        }

        public async Task SendVerifyCode(string code)
        {
            IInstaApi api = await GetApi();

            await api.LoginAsync();

            await api.GetVerifyStep();

            await api.ChooseVerifyMethod(0);

            await api.GetVerifyStep();

            IResult<InstaResetChallenge> result = await api.SendVerifyCode(code);

            if (!result.Succeeded)
            {
                throw new OdkServiceException(result.Info.Message);
            }
        }

        public async Task TriggerVerifyCode()
        {
            IInstaApi api = await GetApi();

            await api.LoginAsync();

            await api.ChooseVerifyMethod(0);
        }

        private async Task<IInstaApi> OpenApi()
        {
            IInstaApi api = await GetApi();

            IResult<InstaLoginResult> loginResult = await api.LoginAsync();
            if (!loginResult.Succeeded)
            {
                throw new OdkServiceException($"Instagram login failed");
            }

            return api;
        }

        private async Task<IInstaApi> GetApi()
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

            return api;
        }
    }
}
