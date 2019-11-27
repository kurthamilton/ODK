using System;
using System.Collections.Generic;
using System.Text;

namespace ODK.Core.Mail
{
    public class MemberEventEmail
    {
        public MemberEventEmail(Guid eventId, Guid memberId, Guid memberEmailId, string responseToken)
        {
            EventId = eventId;
            MemberEmailId = memberEmailId;
            MemberId = memberId;
            ResponseToken = responseToken;
        }

        public Guid EventId { get; }

        public Guid MemberEmailId { get; }

        public Guid MemberId { get; }

        public string ResponseToken { get; }
    }
}
