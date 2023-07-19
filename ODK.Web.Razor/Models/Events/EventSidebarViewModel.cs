using ODK.Core.Chapters;
using ODK.Core.Events;
using ODK.Core.Members;

namespace ODK.Web.Razor.Models.Events
{
    public class EventSidebarViewModel
    {
        public EventSidebarViewModel(Chapter chapter, Event @event, Member? member)
        {
            Chapter = chapter;
            Event = @event;
            Member = member;
        }

        public Chapter Chapter { get; }

        public Event Event { get; }

        public Member? Member { get; }
    }
}
