using System;

namespace ODK.Web.Common.Account.Requests
{
    public class UpdateMemberPropertyApiRequest
    {
        public Guid ChapterPropertyId { get; set; }

        public string Value { get; set; }
    }
}
