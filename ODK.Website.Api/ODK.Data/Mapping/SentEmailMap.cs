using System.Data;
using ODK.Core.Emails;
using ODK.Data.Sql.Mapping;

namespace ODK.Data.Mapping
{
    public class SentEmailMap : SqlMap<SentEmail>
    {
        public SentEmailMap()
            : base("SentEmails")
        {
            Property(x => x.ChapterEmailProviderId);
            Property(x => x.SentDate);
            Property(x => x.To);
            Property(x => x.Subject);
        }

        public override SentEmail Read(IDataReader reader)
        {
            return new SentEmail
            (
                chapterEmailProviderId: reader.GetGuid(0),
                sentDate: reader.GetDateTime(1),
                to: reader.GetString(2),
                subject: reader.GetString(3)
            );
        }
    }
}
