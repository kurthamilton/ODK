using ODK.Core.Chapters;
using ODK.Core.Events;
using ODK.Core.Members;
using ODK.Services.Chapters;

namespace ODK.Web.Razor.Models.Admin.Events;

public class EventViewModel
{
    public EventViewModel(
        Chapter chapter, 
        Member currentMember, 
        Event @event,
        ChapterAdminMembersDto? chapterAdminMembers = null,
        IReadOnlyCollection<EventHost>? hosts = null)
    {
        Chapter = chapter;
        ChapterAdminMembers = chapterAdminMembers;
        CurrentMember = currentMember;
        Event = @event;
        Hosts = hosts;
    }

    public Chapter Chapter { get; }

    public ChapterAdminMembersDto? ChapterAdminMembers { get; }

    public Member CurrentMember { get; }

    public Event Event { get; }

    public IReadOnlyCollection<EventHost>? Hosts { get; }
}
