using System;

namespace ODK.Web.Api.Account
{
    public class UpdateMemberPropertyRequest
    {
        public Guid ChapterPropertyId { get; set; }

        public string Value { get; set; }
    }
}
