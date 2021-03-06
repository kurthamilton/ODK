﻿using System.Data;
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
            Property(x => x.Version).IsRowVersion();
        }

        public override SiteSettings Read(IDataReader reader)
        {
            return new SiteSettings
            (
                googleMapsApiKey: reader.GetString(0),
                instagramUsername: reader.GetString(1),
                instagramPassword: reader.GetString(2),
                version: reader.GetInt64(3)
            );
        }
    }
}
