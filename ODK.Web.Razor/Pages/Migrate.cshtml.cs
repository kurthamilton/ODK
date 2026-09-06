using ODK.Core.Exceptions;
using ODK.Core.Platforms;

namespace ODK.Web.Razor.Pages;

/// <summary>
/// How an organiser brings a group here from another platform. Not available on the DrunkenKnitwits
/// platform, which has no member-owned groups to move one into, so reaching it there is a typed URL.
/// </summary>
public class MigrateModel : OdkPageModel
{
    public void OnGet()
    {
        if (Platform == PlatformType.DrunkenKnitwits)
        {
            throw new OdkNotFoundException();
        }
    }
}
