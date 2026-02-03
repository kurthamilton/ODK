using ODK.Services.Security;

namespace ODK.Web.Common.Routes;

public class AdminRoute
{
    public required string Path { get; init; }

    public required ChapterAdminSecurable Securable { get; init; }
}
