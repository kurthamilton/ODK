using ODK.Services.Security;

namespace ODK.Web.Razor.Pages.Chapters.Admin.Chapters;

public class QuestionsModel : AdminPageModel
{
    public QuestionsModel()
    {
    }

    public override ChapterAdminSecurable Securable => ChapterAdminSecurable.Questions;

    public void OnGet()
    {
    }
}