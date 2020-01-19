using System;

namespace ODK.Web.Common.Chapters.Responses
{
    public class ChapterPropertyOptionApiResponse
    {
        public Guid ChapterPropertyId { get; set; }

        public bool? FreeText { get; set; }

        public string Value { get; set; }
    }
}
