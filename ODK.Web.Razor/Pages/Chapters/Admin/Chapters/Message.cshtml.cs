namespace ODK.Web.Razor.Pages.Chapters.Admin.Chapters;

public class MessageModel : AdminPageModel
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