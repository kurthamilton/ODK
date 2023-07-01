using ODK.Core.Members;

namespace ODK.Web.Razor.Models.Members
{
    public class MemberProfileViewModel
    {
        public MemberProfileViewModel(Member member, Member currentMember)
        {
            CurrentMember = currentMember;
            Member = member;
        }

        public Member CurrentMember { get; }

        public Member Member { get; }
    }
}
