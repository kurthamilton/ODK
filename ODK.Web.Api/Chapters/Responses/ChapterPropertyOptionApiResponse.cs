using System;

namespace ODK.Web.Api.Chapters.Responses
{
    public class ChapterPropertyOptionApiResponse
    {
        public Guid ChapterPropertyId { get; set; }

        public bool? FreeText { get; set; }

        public string Value { get; set; }
    }
}
