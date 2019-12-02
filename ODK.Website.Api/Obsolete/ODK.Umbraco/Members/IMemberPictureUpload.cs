using System.Web;

namespace ODK.Umbraco.Members
{
    public interface IMemberPictureUpload
    {
        HttpPostedFileBase UploadedPicture { get; }
    }
}
