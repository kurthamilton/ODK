using System.ComponentModel;
using System.Web;
using Umbraco.Core.Models;
using OdkMember = ODK.Core.Members.IMember;

namespace ODK.Umbraco.Members
{
    public class UpdateMemberModel : MemberModel, IMemberPictureUpload
    {
        public UpdateMemberModel()
            : this((IPublishedContent)null)
        {
        }

        public UpdateMemberModel(IPublishedContent member)
            : base(member)
        {
        }

        public UpdateMemberModel(IPublishedContent member, UpdateMemberModel other)
            : base(member)
        {
            CopyFrom(other);
        }

        [DisplayName("Upload new photo")]
        public HttpPostedFileBase UploadedPicture { get; set; }
    }
}
