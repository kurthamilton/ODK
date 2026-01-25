using System.Collections.Generic;

namespace ODK.Web.Common.Config.Settings;

public class InstagramClientAppSettings
{
    public required Dictionary<string, string> Cookies { get; init; }

    public required InstagramClientGraphQLSettings GraphQL { get; init; }

    public required Dictionary<string, string> Headers { get; init; }
}
