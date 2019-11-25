using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace ODK.Umbraco.Members
{
    public class RegisterMemberModel : MemberModel, IMemberPictureUpload
    {
        public RegisterMemberModel()
        {
        }

        public RegisterMemberModel(RegisterMemberModel other)
        {
            if (other != null)
            {
                CopyFrom(other);

                Password = other.Password;
                PasswordConfirm = other.PasswordConfirm;
            }
        }

        [Required]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters long")]
        public string Password { get; set; }

        [DisplayName("Confirm password")]
        [Compare(nameof(Password), ErrorMessage = "Passwords must match")]
        public string PasswordConfirm { get; set; }

        [Required(ErrorMessage = "Photo required")]
        [DisplayName("Please upload your photo")]
        public HttpPostedFileBase UploadedPicture { get; set; }
    }
}
