using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ODK.Core.Members;
using ODK.Services.Members;

namespace ODK.Web.Api.Members
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class MembersController : OdkControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IMemberService _memberService;

        public MembersController(IMemberService memberService, IMapper mapper)
        {
            _mapper = mapper;
            _memberService = memberService;
        }

        [HttpGet]
        public async Task<IEnumerable<MemberResponse>> Get(Guid chapterId)
        {
            IReadOnlyCollection<Member> members = await _memberService.GetMembers(GetMemberId(), chapterId);
            return members.Select(_mapper.Map<MemberResponse>);
        }

        [HttpGet("{id}/Profile")]
        public async Task<MemberProfileResponse> Profile(Guid id)
        {
            MemberProfile profile = await _memberService.GetMemberProfile(GetMemberId(), id);
            return _mapper.Map<MemberProfileResponse>(profile);
        }

        [HttpGet("{id}/image")]
        public async Task<IActionResult> MemberImage(Guid id)
        {
            MemberImage image = await _memberService.GetMemberImage(GetMemberId(), id);
            if (image == null)
            {
                return NoContent();
            }

            return File(image.ImageData, image.MimeType);
        }
    }
}
