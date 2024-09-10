namespace ODK.Core.Members;

public class MemberEmailPreference
{
    public bool Disabled { get; set; }

    public Guid MemberId { get; set; }

    public MemberEmailPreferenceType Type { get; set; }
}
