using Microsoft.AspNetCore.Mvc;
using ODK.Core.Exceptions;
using ODK.Core.Members;
using ODK.Services.Caching;
using ODK.Services.Members;

namespace ODK.Web.Razor.Pages.Chapters.Members;

public class MemberModel : ChapterPageModel
{
    private readonly IMemberService _memberService;

    public MemberModel(IRequestCache requestCache, IMemberService memberService)
        : base(requestCache)
    {
        _memberService = memberService;
    }

    public Member Member { get; private set; } = null!;

    public async Task<IActionResult> OnGet(Guid id)
    {
        var member = await _memberService.GetMember(id, Chapter.Id);
        if (member == null)
        {
            throw new OdkNotFoundException();
        }

        Member = member;

        return Page();
    }
}
