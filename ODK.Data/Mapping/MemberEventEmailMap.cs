using System;
using System.Data;
using ODK.Core.Mail;
using ODK.Data.Sql.Mapping;

namespace ODK.Data.Mapping
{
    public class MemberEventEmailMap : SqlMap<MemberEventEmail>
    {
        public MemberEventEmailMap()
            : base("MemberEventEmails")
        {
            Property(x => x.EventId);
            Property(x => x.MemberId);
            Property(x => x.MemberEmailId);
            Property(x => x.ResponseToken);
            Property(x => x.Sent).FromTable("MemberEmails");

            Join<MemberEmail, Guid>(x => x.MemberEmailId, x => x.Id);
        }

        public override MemberEventEmail Read(IDataReader reader)
        {
            return new MemberEventEmail
            (
                eventId: reader.GetGuid(0),
                memberId: reader.GetGuid(1),
                memberEmailId: reader.GetGuid(2),
                responseToken: reader.GetString(3),
                sent: reader.GetBoolean(4)
            );
        }
    }
}
