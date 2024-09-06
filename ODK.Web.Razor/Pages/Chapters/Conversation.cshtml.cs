using ODK.Services.Caching;

namespace ODK.Web.Razor.Pages.Chapters;

public class ConversationModel : ChapterPageModel
{
    public ConversationModel(IRequestCache requestCache) 
        : base(requestCache)
    {
    }

    public Guid ConversationId { get; private set; }

    public void OnGet(Guid id)
    {
        ConversationId = id;
    }
}
