using ODK.Services.Members.Models;

namespace ODK.Web.Razor.Services;

/// <summary>
/// Stages parsed member-import rows server-side between the upload/preview step and the confirm step,
/// so the confirm form only has to post a small token rather than round-tripping every row through
/// hidden fields (which is bounded by MVC's model-binding collection limit).
/// </summary>
public interface IMemberImportStagingService
{
    string Stage(IReadOnlyCollection<MemberImportModel> members);

    IReadOnlyCollection<MemberImportModel>? Retrieve(string? token);

    void Remove(string token);
}
