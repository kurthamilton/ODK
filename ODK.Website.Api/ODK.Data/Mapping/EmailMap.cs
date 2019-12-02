using System.Data;
using ODK.Core.Mail;
using ODK.Data.Sql.Mapping;

namespace ODK.Data.Mapping
{
    public class EmailMap : SqlMap<Email>
    {
        public EmailMap()
            : base("Emails")
        {
            Property(x => x.Type).HasColumnName("EmailTypeId");
            Property(X => X.Subject);
            Property(x => x.Body);
        }

        public override Email Read(IDataReader reader)
        {
            return new Email
            (
                type: (EmailType)reader.GetInt32(0),
                subject: reader.GetString(1),
                body: reader.GetString(2)
            );
        }
    }
}
