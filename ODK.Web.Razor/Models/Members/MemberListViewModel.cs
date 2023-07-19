using ODK.Core.Chapters;

namespace ODK.Web.Razor.Models.Members
{
    public class MemberListViewModel
    {
        public MemberListViewModel(Chapter chapter, IEnumerable<ListMemberViewModel> members)
        {
            Chapter = chapter;
            Members = members.ToArray();
        }

        public Chapter Chapter { get; }

        public int Cols { get; set; }

        public int ImageHeight => Size == "xs" ? 50 : 150;

        public int MaxWidth => Size == "xs" ? 50 : 0;

        public IReadOnlyCollection<ListMemberViewModel> Members { get; }

        public string? Size { get; set; }
    }
}
