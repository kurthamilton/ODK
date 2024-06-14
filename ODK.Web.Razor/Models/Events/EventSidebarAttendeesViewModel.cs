using ODK.Core.Chapters;
using ODK.Core.Members;

namespace ODK.Web.Razor.Models.Events;

public class EventSidebarAttendeesViewModel
{
    public EventSidebarAttendeesViewModel(Chapter chapter, IEnumerable<Member> members)
    {
        Chapter = chapter;
        Members = members.ToArray();
    }

    public Chapter Chapter { get; }

    public IReadOnlyCollection<Member> Members { get; }

    public string? Title { get; set; }
}
