using ODK.Services.Caching;

namespace ODK.Web.Razor.Pages.Chapters.Admin.Chapters;

public class MessageModel : AdminPageModel
{
    public MessageModel(IRequestCache requestCache) 
        : base(requestCache)
    {
    }

    public Guid MessageId { get; private set; }

    public void OnGet(Guid id)
    {
        MessageId = id;
    }
}
