using System.Collections.Generic;
using ODK.Core.Platforms;

namespace ODK.Web.Common.Routes;

public class OdkRoutesFactory : IOdkRoutesFactory
{
    // Kept per platform, not one instance: a scope asks for both platforms' trees whenever it builds a URL
    // about another platform's group, and a single cached instance would answer every ask with the first.
    private readonly Dictionary<PlatformType, IOdkRoutes> _odkRoutes = [];

    public IOdkRoutes Get(PlatformType platform)
    {
        if (!_odkRoutes.TryGetValue(platform, out var odkRoutes))
        {
            odkRoutes = new OdkRoutes(platform);
            _odkRoutes.Add(platform, odkRoutes);
        }

        return odkRoutes;
    }
}
