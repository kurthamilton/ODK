namespace ODK.Services.Members.Models;

public class UpdateMemberChapterProfile
{
    public IEnumerable<UpdateMemberProperty> Properties { get; set; } = Enumerable.Empty<UpdateMemberProperty>();
}
