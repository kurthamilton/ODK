using System;

namespace ODK.Web.Api.Chapters
{
    public class ChapterPropertyOptionResponse
    {
        public Guid ChapterPropertyId { get; set; }

        public bool? FreeText { get; set; }

        public string Value { get; set; }
    }
}
