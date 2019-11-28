using System.Data;
using ODK.Core.Chapters;
using ODK.Data.Sql.Mapping;

namespace ODK.Data.Mapping
{
    public class ContactRequestMap : SqlMap<ContactRequest>
    {
        public ContactRequestMap()
            : base("ContactRequests")
        {
            Property(x => x.Id).HasColumnName("ContactRequestId").IsIdentity();
            Property(x => x.ChapterId);
            Property(x => x.CreatedDate);
            Property(x => x.FromAddress);
            Property(x => x.Message);
            Property(x => x.Sent);
        }

        public override ContactRequest Read(IDataReader reader)
        {
            return new ContactRequest
            (
                id: reader.GetGuid(0),
                chapterId: reader.GetGuid(1),
                createdDate: reader.GetDateTime(2),
                fromAddress: reader.GetString(3),
                message: reader.GetString(4),
                sent: reader.GetBoolean(5)
            );
        }
    }
}
