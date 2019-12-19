using System.Data;
using ODK.Core.Chapters;
using ODK.Data.Sql;
using ODK.Data.Sql.Mapping;

namespace ODK.Data.Mapping
{
    public class ChapterEmailSettingsMap : SqlMap<ChapterEmailSettings>
    {
        public ChapterEmailSettingsMap()
            : base("ChapterSettings")
        {
            Property(x => x.ChapterId);
            Property(x => x.AdminEmailAddress);
            Property(x => x.ContactEmailAddress);
            Property(x => x.EmailApiKey);
            Property(x => x.EmailProvider);
            Property(x => x.FromEmailAddress);
            Property(x => x.FromEmailName);
        }

        public override ChapterEmailSettings Read(IDataReader reader)
        {
            return new ChapterEmailSettings
            (
                chapterId: reader.GetGuid(0),
                adminEmailAddress: reader.GetString(1),
                contactEmailAddress: reader.GetString(2),
                emailApiKey: reader.GetStringOrDefault(3),
                emailProvider: reader.GetStringOrDefault(4),
                fromEmailAddress: reader.GetString(5),
                fromEmailName: reader.GetStringOrDefault(6)
            );
        }
    }
}
