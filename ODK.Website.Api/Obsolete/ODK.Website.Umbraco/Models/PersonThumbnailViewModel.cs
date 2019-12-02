using Umbraco.Core.Models;
using Umbraco.Web;
using OdkMember = ODK.Core.Members.IMember;

namespace ODK.Website.Models
{
    public class PersonThumbnailViewModel
    {
        public PersonThumbnailViewModel(OdkMember member, IPublishedContent chapter, bool showFullName = true)
        {
            FullName = member.FullName;
            MemberId = member.Id;
            PersonPageUrl = chapter.GetPropertyValue<IPublishedContent>("personPage").Url;
            PictureUrl = member.PictureThumbnailUrl;
            ShowFullName = showFullName;
        }

        public string FullName { get; }

        public int MemberId { get; }

        public string PersonPageUrl { get; }

        public string PictureUrl { get; }

        public bool ShowFullName { get; }
    }
}