using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Core.Venues;

namespace ODK.Web.Razor.Models.Admin.Venues
{
    public class VenueViewModel
    {
        public VenueViewModel(Chapter chapter, Member currentMember, Venue venue)
        {
            Chapter = chapter;
            CurrentMember = currentMember;
            Venue = venue;
        }

        public Chapter Chapter { get; }

        public Member CurrentMember { get; }

        public Venue Venue { get; }
    }
}
