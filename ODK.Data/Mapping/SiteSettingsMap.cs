using System.Data;
using ODK.Core.Settings;
using ODK.Data.Sql.Mapping;

namespace ODK.Data.Mapping
{
    public class SiteSettingsMap : SqlMap<SiteSettings>
    {
        public SiteSettingsMap()
            : base("Settings")
        {
            Property(x => x.GoogleMapsApiKey);
            Property(x => x.InstagramUsername);
            Property(x => x.InstagramPassword);
            Property(x => x.InstagramScraperUserAgent);
            Property(x => x.ScrapeInstagram);
            Property(x => x.Version).IsRowVersion();
            Property(x => x.RecaptchaSiteKey);
            Property(x => x.RecaptchaSecretKey);
        }

        public override SiteSettings Read(IDataReader reader)
        {
            return new SiteSettings
            (
                googleMapsApiKey: reader.GetString(0),
                instagramUsername: reader.GetString(1),
                instagramPassword: reader.GetString(2),
                instagramScraperUserAgent: reader.GetString(3),
                scrapeInstagram: reader.GetBoolean(4),
                version: reader.GetInt64(5),
                recaptchaSiteKey: reader.GetString(6),
                recaptchaSecretKey: reader.GetString(7)
            );
        }
    }
}
