using System;

namespace ODK.Core.Members
{
    public class MemberActivationToken
    {
        public MemberActivationToken(Guid memberId, string activationToken)
        {
            ActivationToken = activationToken;
            MemberId = memberId;
        }

        public string ActivationToken { get; }

        public Guid MemberId { get; }
    }
}
