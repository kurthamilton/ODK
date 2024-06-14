using System;

namespace ODK.Core.Members
{
    public class MemberPasswordResetRequest
    {
        public MemberPasswordResetRequest(Guid id, Guid memberId, DateTime created, DateTime expires, string token)
        {
            Created = created;
            Expires = expires;
            Id = id;
            MemberId = memberId;
            Token = token;
        }

        public DateTime Created { get; }

        public DateTime Expires { get; }

        public Guid Id { get; set; }

        public Guid MemberId { get; }        

        public string Token { get; }
    }
}
