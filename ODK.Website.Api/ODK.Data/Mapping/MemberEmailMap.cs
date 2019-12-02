using System.Data;
using ODK.Core.Mail;
using ODK.Data.Sql.Mapping;

namespace ODK.Data.Mapping
{
    public class MemberEmailMap : SqlMap<MemberEmail>
    {
        public MemberEmailMap()
            : base("MemberEmails")
        {
            Property(x => x.Id).HasColumnName("MemberEmailId").IsIdentity();
            Property(x => x.ChapterId);
            Property(x => x.ToAddress);
            Property(x => x.Subject);
            Property(x => x.CreatedDate);
            Property(x => x.Sent);
        }

        public override MemberEmail Read(IDataReader reader)
        {
            return new MemberEmail
            (
                id: reader.GetGuid(0),
                chapterId: reader.GetGuid(1),
                toAddress: reader.GetString(2),
                subject: reader.GetString(3),
                createdDate: reader.GetDateTime(4),
                sent: reader.GetBoolean(5)
            );
        }
    }
}
