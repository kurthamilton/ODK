using System;
using System.Collections.Generic;

namespace ODK.Web.Api.Chapters
{
    public class ChapterPropertyResponse
    {
        public Guid DataTypeId { get; set; }

        public string HelpText { get; set; }

        public Guid Id { get; set; }

        public string Name { get; set; }

        public IEnumerable<string> Options { get; set; }

        public bool? Required { get; set; }

        public string Subtitle { get; set; }
    }
}
