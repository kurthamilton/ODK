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
            Property(x => x.Version).IsRowVersion();
        }

        public override SiteSettings Read(IDataReader reader)
        {
            return new SiteSettings
            (
                reader.GetString(0),
                reader.GetInt64(1)
            );
        }
    }
}
