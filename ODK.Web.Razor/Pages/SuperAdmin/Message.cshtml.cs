using ODK.Services.Caching;

namespace ODK.Web.Razor.Pages.SuperAdmin;

public class MessageModel : SuperAdminPageModel
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