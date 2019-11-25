using System.Collections.Generic;

namespace ODK.Umbraco.Settings
{
    public class SiteSettings
    {
        public IEnumerable<KeyValuePair<string, string>> FooterLinks { get; set; }

        public string SiteUrl { get; set; }
    }
}
