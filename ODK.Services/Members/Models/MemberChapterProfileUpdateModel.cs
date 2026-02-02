namespace ODK.Services.Members.Models;

public class MemberChapterProfileUpdateModel
{
    public IEnumerable<MemberPropertyUpdateModel> Properties { get; set; } = Enumerable.Empty<MemberPropertyUpdateModel>();
}
