using ODK.Core.Chapters;

namespace ODK.Web.Razor.Models.SocialMedia
{
    public class InstagramPostViewModel
    {
        public InstagramPostViewModel(Chapter chapter, Guid instagramPostId, string caption, 
            string externalId)
        {
            Caption = caption;
            Chapter = chapter;
            ExternalId = externalId;
            InstagramPostId = instagramPostId;
        }

        public string Caption { get; }

        public Chapter Chapter { get; }

        public string ExternalId { get; }

        public Guid InstagramPostId { get; }
    }
}
