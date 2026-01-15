namespace ODK.Web.Razor.Pages.Chapters.Admin.Chapters;

public class QuestionEditModel : AdminPageModel
{
    public QuestionEditModel()
    {
    }

    public Guid QuestionId { get; private set; }

    public void OnGet(Guid id)
    {
        QuestionId = id;
    }
}