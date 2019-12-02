using System.Data;
using ODK.Core.Chapters;
using ODK.Data.Sql.Mapping;

namespace ODK.Data.Mapping
{
    public class ChapterEmailSettingsMap : SqlMap<ChapterEmailSettings>
    {
        public ChapterEmailSettingsMap()
            : base("ChapterAdmin")
        {
            Property(x => x.ChapterId);
            Property(x => x.ContactEmailAddress);
            Property(x => x.FromEmailAddress);
        }

        public override ChapterEmailSettings Read(IDataReader reader)
        {
            return new ChapterEmailSettings
            (
                chapterId: reader.GetGuid(0),
                contactEmailAddress: reader.GetString(1),
                fromEmailAddress: reader.GetString(2)
            );
        }
    }
}
