namespace ODK.Web.Razor.Pages.SiteAdmin;

public class QuestionModel : SiteAdminPageModel
{
    public Guid Id { get; private set; }

    public void OnGet(Guid id)
    {
        Id = id;
    }
}
