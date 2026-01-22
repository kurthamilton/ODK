namespace ODK.Web.Razor.Pages.SiteAdmin;

public class TopicModel : SiteAdminPageModel
{
    public Guid TopicId { get; private set; }

    public void OnGet(Guid id)
    {
        TopicId = id;
    }
}