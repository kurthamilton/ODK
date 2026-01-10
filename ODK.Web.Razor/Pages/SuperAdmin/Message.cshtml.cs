namespace ODK.Web.Razor.Pages.SuperAdmin;

public class MessageModel : SuperAdminPageModel
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