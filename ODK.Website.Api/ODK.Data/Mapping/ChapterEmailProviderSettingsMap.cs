using System.Data;
using ODK.Core.Chapters;
using ODK.Data.Sql.Mapping;

namespace ODK.Data.Mapping
{
    public class ChapterEmailProviderSettingsMap : SqlMap<ChapterEmailProviderSettings>
    {
        public ChapterEmailProviderSettingsMap()
            : base("ChapterEmailProviderSettings")
        {
            Property(x => x.ChapterId);
            Property(x => x.SmtpServer);
            Property(x => x.SmtpPort);
            Property(x => x.SmtpLogin);
            Property(x => x.SmtpPassword);
            Property(x => x.FromEmailAddress);
            Property(x => x.FromName);
        }

        public override ChapterEmailProviderSettings Read(IDataReader reader)
        {
            return new ChapterEmailProviderSettings
            (
                chapterId: reader.GetGuid(0),
                smtpServer: reader.GetString(1),
                smtpPort: reader.GetInt32(2),
                smtpLogin: reader.GetString(3),
                smtpPassword: reader.GetString(4),
                fromEmailAddress: reader.GetString(5),
                fromName: reader.GetString(6)
            );
        }
    }
}
