using System;

namespace ODK.Web.Common.Routes;

public class MemberRoutes : RoutesBase
{
    public string Avatar(Guid memberId, string? chapterName = null) =>
        MemberPath(chapterName, $"/{memberId}/avatar");

    public string Image(Guid memberId, string? chapterName = null) =>
        MemberPath(chapterName, $"/{memberId}/image");

    private string MemberPath(string? chapterName, string path) 
        => GetRoute(chapterName, $"/members{path}");
}
