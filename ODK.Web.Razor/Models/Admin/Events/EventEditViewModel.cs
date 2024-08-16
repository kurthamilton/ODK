using ODK.Core.Chapters;
using ODK.Core.Countries;
using ODK.Core.Events;
using ODK.Core.Members;

namespace ODK.Web.Razor.Models.Admin.Events;

public class EventEditViewModel : EventViewModel
{
    public EventEditViewModel(
        Chapter chapter, 
        Member currentMember, 
        Event @event,
        Currency? currency,
        IReadOnlyCollection<ChapterAdminMember>? chapterAdminMembers,
        IReadOnlyCollection<EventHost>? hosts)
        : base(chapter, currentMember, @event)
    {
        ChapterAdminMembers = chapterAdminMembers;
        Currency = currency;
        Hosts = hosts;
    }

    public IReadOnlyCollection<ChapterAdminMember>? ChapterAdminMembers { get; }    

    public Currency? Currency { get; }

    public IReadOnlyCollection<EventHost>? Hosts { get; }
}
