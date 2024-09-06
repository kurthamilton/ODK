using ODK.Core.Chapters;

namespace ODK.Services.Users.ViewModels;

public class MemberChapterPaymentsPageViewModel : MemberPaymentsPageViewModel
{
    public required Chapter Chapter { get; set; }    
}
