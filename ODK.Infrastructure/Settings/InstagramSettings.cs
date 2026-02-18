using System.Collections.Generic;

namespace ODK.Infrastructure.Settings;

public class InstagramSettings
{
    public required string BaseUrl { get; init; }    

    public required InstagramClientAppSettings Client { get; init; }    

    public required int FetchWaitSeconds { get; init; }    

    public required InstagramPathsAppSettings Paths { get; init; }
}