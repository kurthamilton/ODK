using System;

namespace ODK.Web.Api.Chapters
{
    public class ChapterResponse
    {
        public Guid CountryId { get; set; }

        public Guid Id { get; set; }

        public string Name { get; set; }

        public string RedirectUrl { get; set; }
    }
}
