using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using ODK.Core.Chapters;
using ODK.Core.Settings;
using ODK.Core.SocialMedia;

namespace ODK.Services.SocialMedia
{
    public class InstagramService : IInstagramService
    {
        private readonly IChapterRepository _chapterRepository;
        private readonly IInstagramRepository _instagramRepository;
        private readonly ISettingsRepository _settingsRepository;

        public InstagramService(IChapterRepository chapterRepository, ISettingsRepository settingsRepository,
            IInstagramRepository instagramRepository)
        {
            _chapterRepository = chapterRepository;
            _instagramRepository = instagramRepository;
            _settingsRepository = settingsRepository;
        }

        public async Task<IReadOnlyCollection<SocialMediaImage>> FetchInstagramImages(Guid chapterId)
        {
            ChapterLinks links = await _chapterRepository.GetChapterLinks(chapterId);
            if (links == null)
            {
                return Array.Empty<SocialMediaImage>();
            }

            if (string.IsNullOrEmpty(links.InstagramName))
            {
                return Array.Empty<SocialMediaImage>();
            }

            Task<SiteSettings> settingsTask = _settingsRepository.GetSiteSettings();
            Task<DateTime?> mostRecentPostDateTask = _instagramRepository.GetLastPostDate(chapterId);

            await Task.WhenAll(settingsTask, mostRecentPostDateTask);

            DateTime? after = mostRecentPostDateTask.Result;

            if (settingsTask.Result.ScrapeInstagram)
            {
                await DownloadNewImages(chapterId, settingsTask.Result.InstagramScraperUserAgent,
                    links.InstagramName, after);
            }

            IReadOnlyCollection<InstagramPost> posts = await _instagramRepository.GetPosts(chapterId, 8);

            return posts
                .Select(x => new SocialMediaImage
                {
                    Caption = x.Caption,
                    Url = x.Url
                })
                .ToArray();
        }

        public async Task<InstagramImage> GetInstagramImage(Guid instagramPostId)
        {
            return await _instagramRepository.GetImage(instagramPostId);
        }

        public async Task<IReadOnlyCollection<InstagramPost>> GetInstagramPosts(Guid chapterId, int pageSize)
        {
            ChapterLinks links = await _chapterRepository.GetChapterLinks(chapterId);
            if (links == null)
            {
                return Array.Empty<InstagramPost>();
            }

            if (string.IsNullOrEmpty(links.InstagramName))
            {
                return Array.Empty<InstagramPost>();
            }

            Task<SiteSettings> settingsTask = _settingsRepository.GetSiteSettings();
            Task<DateTime?> latestPostDateTask = _instagramRepository.GetLastPostDate(chapterId);

            await Task.WhenAll(settingsTask, latestPostDateTask);

            DateTime? latest = latestPostDateTask.Result;

            if (settingsTask.Result.ScrapeInstagram)
            {
                await DownloadNewImages(chapterId, settingsTask.Result.InstagramScraperUserAgent,
                    links.InstagramName, latest);
            }

            return await _instagramRepository.GetPosts(chapterId, pageSize);
        }
        
        private async Task DownloadNewImages(Guid chapterId, string userAgent, string username, DateTime? after)
        {
            string json;

            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("User-Agent", userAgent);
                string url = $"https://www.instagram.com/api/v1/users/web_profile_info/?username={username}";
                HttpResponseMessage response = await httpClient.GetAsync(url);
                json = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                {
                    return;
                }
            }

            JArray edges;
            try
            {
                JObject data = JObject.Parse(json);
                edges = data["data"]?["user"]?["edge_owner_to_timeline_media"]?["edges"] as JArray;
                if (edges == null)
                {
                    return;
                }
            }
            catch
            {
                return;
            }

            foreach (JToken edge in edges)
            {
                JToken node = edge["node"];

                InstagramPost post = ParsePost(chapterId, node);
                if (post == null || post.Date <= after)
                {
                    continue;
                }

                InstagramImage image = await ParseImage(post, userAgent, node);
                if (image == null)
                {
                    continue;
                } 

                await _instagramRepository.AddPost(post);
                await _instagramRepository.AddImage(image);
            }
        }

        private async Task<InstagramImage> ParseImage(InstagramPost post, string userAgent, JToken node)
        {
            try
            {
                string thumbnailUrl = node["thumbnail_src"]?.ToString();
                if (string.IsNullOrEmpty(thumbnailUrl))
                {
                    return null;
                }

                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("User-Agent", userAgent);

                    HttpResponseMessage response = await client.GetAsync(thumbnailUrl);
                    if (!response.IsSuccessStatusCode)
                    {
                        return null;
                    }

                    byte[] imageData = await response.Content.ReadAsByteArrayAsync();
                    string mimeType = response.Content.Headers.ContentType.MediaType;

                    return new InstagramImage(
                        instagramPostId: post.Id,
                        imageData: imageData,
                        mimeType: mimeType);
                }
            }
            catch
            {
                return null;
            }
        }

        private static InstagramPost ParsePost(Guid chapterId, JToken node)
        {
            try
            {
                int timestamp = node["taken_at_timestamp"].ToObject<int>();
                DateTime date = new DateTime(1970, 1, 1).AddSeconds(timestamp);
                string shortcode = node["shortcode"].ToString();

                return new InstagramPost(
                    id: Guid.NewGuid(),
                    chapterId: chapterId,
                    externalId: shortcode,
                    date: date,
                    caption: node["edge_media_to_caption"]["edges"].FirstOrDefault()?["node"]["text"].ToString(),
                    url: $"https://www.instagram.com/p/{shortcode}/?img_index=1"
                );
            }
            catch
            {
                return null;
            }
        }
    }
}
