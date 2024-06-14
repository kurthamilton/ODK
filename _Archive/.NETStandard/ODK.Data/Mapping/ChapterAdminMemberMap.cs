using System;
using System.Data;
using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Data.Sql;
using ODK.Data.Sql.Mapping;

namespace ODK.Data.Mapping
{
    public class ChapterAdminMemberMap : SqlMap<ChapterAdminMember>
    {
        public ChapterAdminMemberMap()
            : base("ChapterAdminMembers")
        {
            Property(x => x.ChapterId);
            Property(x => x.MemberId);
            Property(x => x.FirstName).From<Member>();
            Property(x => x.LastName).From<Member>();
            Property(x => x.AdminEmailAddress);
            Property(x => x.SuperAdmin).From<Member>();
            Property(x => x.ReceiveContactEmails);
            Property(x => x.ReceiveNewMemberEmails);
            Property(x => x.SendNewMemberEmails);

            Join<Member, Guid>(x => x.MemberId, x => x.Id);
        }

        public override ChapterAdminMember Read(IDataReader reader)
        {
            return new ChapterAdminMember
            (
                chapterId: reader.GetGuid(0),
                memberId: reader.GetGuid(1),
                firstName: reader.GetString(2),
                lastName: reader.GetString(3),
                adminEmailAddress: reader.GetStringOrDefault(4),
                superAdmin: reader.GetBoolean(5),
                receiveContactEmails: reader.GetBoolean(6),
                receiveNewMemberEmails: reader.GetBoolean(7),
                sendNewMemberEmails: reader.GetBoolean(8)
            );
        }
    }
}
