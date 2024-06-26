using Microsoft.AspNetCore.Mvc;
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
        Member? member = await _memberService.GetMember(id);
        if (member == null)
        {
            return NotFound();
        }

        Member = member;

        return Page();
    }
}
