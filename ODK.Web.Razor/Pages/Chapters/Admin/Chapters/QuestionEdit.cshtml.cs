using ODK.Services.Security;

namespace ODK.Web.Razor.Pages.Chapters.Admin.Chapters;

public class QuestionEditModel : AdminPageModel
{
    public QuestionEditModel()
    {
    }

    public Guid QuestionId { get; private set; }

    public override ChapterAdminSecurable Securable => ChapterAdminSecurable.Questions;

    public void OnGet(Guid id)
    {
        QuestionId = id;
    }
}