﻿using System.Collections.Generic;

namespace ODK.Web.Common.Platforms;

public class PlatformProviderSettings
{
    public required IReadOnlyCollection<string> DefaultBaseUrls { get; set; }

    public required IReadOnlyCollection<string> DrunkenKnitwitsBaseUrls { get; init; }
}
