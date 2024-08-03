using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ODK.Core.Members;
using ODK.Services;
using ODK.Services.Members;

namespace ODK.Web.Razor.Controllers;

[ApiController]
public class MembersController : ControllerBase
{
    private static readonly Regex VersionRegex = new(@"^""(?<version>-?\d+)""$");

    private readonly IMemberService _memberService;
    
    public MembersController(IMemberService memberService)
    {
        _memberService = memberService;
    }

    [Authorize]
    [HttpGet("{Chapter}/Members/{Id}/Avatar")]
    public Task<IActionResult> Avatar(Guid id, string chapter) 
        => HandleVersionedRequest(version => _memberService.GetMemberAvatar(version, id), MemberAvatarResult);

    [Authorize]
    [HttpGet("{Chapter}/Members/{Id}/Image")]
    public Task<IActionResult> Image(Guid id, string chapter)
        => HandleVersionedRequest(version => _memberService.GetMemberImage(version, id), MemberImageResult);

    private void AddVersionHeader(long version)
    {
        Response.Headers.Append("ETag", $"\"{version}\"");
    }

    private long? GetRequestVersion()
    {
        string? requestETag = Request.Headers["If-None-Match"].FirstOrDefault();
        if (requestETag == null)
        {
            return null;
        }

        Match match = VersionRegex.Match(requestETag);
        return match.Success ? long.Parse(match.Groups["version"].Value) : new long?();
    }

    protected async Task<IActionResult> HandleVersionedRequest<T>(Func<long?, Task<VersionedServiceResult<T>>> getter, Func<T?, IActionResult> map) where T : class
    {
        long? version = GetRequestVersion();

        VersionedServiceResult<T> result = await getter(version);

        AddVersionHeader(result.Version);

        if (version == result.Version)
        {
            return new StatusCodeResult(304);
        }

        return map(result.Value);
    }

    protected IActionResult MemberAvatarResult(MemberAvatar? image)
    {
        if (image == null)
        {
            return NoContent();
        }

        return File(image.ImageData, image.MimeType);
    }

    protected IActionResult MemberImageResult(MemberImage? image)
    {
        if (image == null)
        {
            return NoContent();
        }

        return File(image.ImageData, image.MimeType);
    }    
}
