using ODK.Core.Chapters;
using ODK.Core.Events;
using ODK.Core.Members;

namespace ODK.Web.Razor.Models.Admin.Events
{
    public class AdminEventViewModel
    {
        public AdminEventViewModel(Chapter chapter, Member currentMember, Event @event)
        {
            Chapter = chapter;
            CurrentMember = currentMember;
            Event = @event;
        }

        public Chapter Chapter { get; }

        public Member CurrentMember { get; }

        public Event Event { get; }
    }
}
