using System;

namespace ODK.Web.Api.Chapters.Responses
{
    public class ChapterApiResponse
    {
        public string BannerImageUrl { get; set; }

        public Guid CountryId { get; set; }

        public Guid Id { get; set; }

        public string Name { get; set; }

        public string RedirectUrl { get; set; }
    }
}
