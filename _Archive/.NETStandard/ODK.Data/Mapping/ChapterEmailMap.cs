using System.Data;
using ODK.Core.Emails;
using ODK.Data.Sql.Mapping;

namespace ODK.Data.Mapping
{
    public class ChapterEmailMap : SqlMap<ChapterEmail>
    {
        public ChapterEmailMap()
            : base("ChapterEmails")
        {
            Property(x => x.Id).HasColumnName("ChapterEmailId").IsIdentity();
            Property(x => x.ChapterId);
            Property(x => x.Type).HasColumnName("EmailTypeId");
            Property(x => x.Subject);
            Property(x => x.HtmlContent);
        }

        public override ChapterEmail Read(IDataReader reader)
        {
            return new ChapterEmail
            (
                id: reader.GetGuid(0),
                chapterId: reader.GetGuid(1),
                type: (EmailType)reader.GetInt32(2),
                subject: reader.GetString(3),
                htmlContent: reader.GetString(4)
            );
        }
    }
}
