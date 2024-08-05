namespace ODK.Services.Members;

public class UpdateMemberChapterProfile
{
    public bool? EmailOptIn { get; set; }

    public string FirstName { get; set; } = "";

    public string LastName { get; set; } = "";

    public IEnumerable<UpdateMemberProperty> Properties { get; set; } = Enumerable.Empty<UpdateMemberProperty>();
}
