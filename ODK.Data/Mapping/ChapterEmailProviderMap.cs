using System.Data;
using ODK.Core.Chapters;
using ODK.Data.Sql;
using ODK.Data.Sql.Mapping;

namespace ODK.Data.Mapping;

public class ChapterEmailProviderMap : SqlMap<ChapterEmailProvider>
{
    public ChapterEmailProviderMap()
        : base("ChapterEmailProviders")
    {
        Property(x => x.Id).HasColumnName("ChapterEmailProviderId").IsIdentity();
        Property(x => x.ChapterId);
        Property(x => x.SmtpServer);
        Property(x => x.SmtpPort);
        Property(x => x.SmtpLogin);
        Property(x => x.SmtpPassword);
        Property(x => x.FromEmailAddress);
        Property(x => x.FromName);
        Property(x => x.BatchSize);
        Property(x => x.DailyLimit);
        Property(x => x.Order);
    }

    public override ChapterEmailProvider Read(IDataReader reader)
    {
        return new ChapterEmailProvider
        (
            id: reader.GetGuid(0),
            chapterId: reader.GetGuid(1),
            smtpServer: reader.GetString(2),
            smtpPort: reader.GetInt32(3),
            smtpLogin: reader.GetString(4),
            smtpPassword: reader.GetString(5),
            fromEmailAddress: reader.GetString(6),
            fromName: reader.GetString(7),
            batchSize: reader.GetNullableInt(8),
            dailyLimit: reader.GetInt32(9),
            order: reader.GetInt32(10)
        );
    }
}
