using ODK.Core.Chapters;
using ODK.Core.Members;

namespace ODK.Web.Razor.Models.Events;

public class EventsContentViewModel
{
    public EventsContentViewModel(Chapter chapter, Member? member)
    {
        Chapter = chapter;
        Member = member;
    }

    public Chapter Chapter { get; set; }

    public Member? Member { get; set; }
}
