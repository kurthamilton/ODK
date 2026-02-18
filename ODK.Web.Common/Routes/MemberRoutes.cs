using System;

namespace ODK.Web.Common.Routes;

public class MemberRoutes
{
    public string Avatar(Guid memberId, int version) => $"/members/{memberId}/avatar?v={version}";
}