namespace ODK.Web.Razor.Pages.SiteAdmin;

public class MessageModel : SiteAdminPageModel
{
    public MessageModel()
    {
    }

    public Guid MessageId { get; private set; }

    public void OnGet(Guid id)
    {
        MessageId = id;
    }
}