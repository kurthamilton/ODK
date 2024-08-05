using System;

namespace ODK.Web.Common.Routes;

public class MemberRoutes : RoutesBase
{
    public string Avatar(Guid memberId, string? chapterName = null) =>
        MemberPath(chapterName, $"{memberId}/Avatar");

    public string Image(Guid memberId, string? chapterName = null) =>
        MemberPath(chapterName, $"{memberId}/Image");

    private string MemberPath(string? chapterName, string path) 
        => GetRoute(chapterName, $"Members/{path}");
}
