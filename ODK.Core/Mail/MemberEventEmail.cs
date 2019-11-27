using System;

namespace ODK.Core.Mail
{
    public class MemberEventEmail
    {
        public MemberEventEmail(Guid eventId, Guid memberId, Guid memberEmailId, string responseToken, bool sent)
        {
            EventId = eventId;
            MemberEmailId = memberEmailId;
            MemberId = memberId;
            ResponseToken = responseToken;
            Sent = sent;
        }

        public Guid EventId { get; }

        public Guid MemberEmailId { get; }

        public Guid MemberId { get; }

        public string ResponseToken { get; }

        public bool Sent { get; }
    }
}
