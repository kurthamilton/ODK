using System;

namespace ODK.Web.Common.Routes;

public class MemberRoutes
{

    public string Avatar(Guid memberId) => $"/members/{memberId}/avatar";

    public string Image(Guid memberId) => $"/members/{memberId}/image";
}
