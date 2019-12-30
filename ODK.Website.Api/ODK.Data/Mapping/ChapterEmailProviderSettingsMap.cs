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
            Property(x => x.EmailProvider);
            Property(x => x.ApiKey);
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
                emailProvider: reader.GetString(1),
                apiKey: reader.GetString(2),
                smtpServer: reader.GetString(3),
                smtpPort: reader.GetInt32(4),
                smtpLogin: reader.GetString(5),
                smtpPassword: reader.GetString(6),
                fromEmailAddress: reader.GetString(7),
                fromName: reader.GetString(8)
            );
        }
    }
}
