namespace ODK.Web.Razor.Pages.My.Groups.Group.Questions;

public class QuestionModel : OdkGroupAdminPageModel
{
    public Guid QuestionId { get; private set; }

    public void OnGet(Guid questionId)
    {
        QuestionId = questionId;
    }
}