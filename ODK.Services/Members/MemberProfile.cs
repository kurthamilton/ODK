using ODK.Core.Chapters;
using ODK.Core.Members;

namespace ODK.Services.Members;

public class MemberProfile
{
    public MemberProfile(Member member, IEnumerable<MemberProperty> memberProperties, 
        IEnumerable<ChapterProperty> chapterProperties)
        : this(member.EmailAddress, member.EmailOptIn, member.FirstName, member.LastName, 
            member.CreatedDate.Date, memberProperties, chapterProperties)
    {
    }

    public MemberProfile(string emailAddress, bool emailOptIn, string firstName, string lastName, DateTime joined, 
        IEnumerable<MemberProperty> memberProperties, IEnumerable<ChapterProperty> chapterProperties)
    {
        ChapterProperties = chapterProperties.ToDictionary(x => x.Id);
        EmailAddress = emailAddress;
        EmailOptIn = emailOptIn;
        FirstName = firstName;
        Joined = joined;
        LastName = lastName;
        MemberProperties = memberProperties.ToArray();
    }

    public IDictionary<Guid, ChapterProperty> ChapterProperties { get; }

    public string EmailAddress { get; }

    public bool EmailOptIn { get; set; }

    public string FirstName { get; set; }

    public DateTime Joined { get; }

    public string LastName { get; set; }

    public IReadOnlyCollection<MemberProperty> MemberProperties { get; }
}
