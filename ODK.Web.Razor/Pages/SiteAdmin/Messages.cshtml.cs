namespace ODK.Web.Razor.Pages.SiteAdmin;

public class MessagesModel : SiteAdminPageModel
{
    public MessagesModel()
    {
    }

    public bool Spam { get; private set; }

    public void OnGet(bool spam = false)
    {
        Spam = spam;
    }
}