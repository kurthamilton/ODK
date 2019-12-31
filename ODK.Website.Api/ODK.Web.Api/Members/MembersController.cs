using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ODK.Services.Members;
using ODK.Web.Api.Members.Responses;

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
        public async Task<ActionResult<IEnumerable<MemberApiResponse>>> Get(Guid chapterId)
        {
            return await HandleVersionedRequest(
                version => _memberService.GetMembers(version, GetMemberId(), chapterId),
                x => x.Select(_mapper.Map<MemberApiResponse>));
        }

        [AllowAnonymous]
        [HttpGet("{id}/Image")]
        public async Task<IActionResult> MemberImage(Guid id, int? size)
        {
            return await HandleVersionedRequest(
                version => _memberService.GetMemberImage(version, id, size),
                MemberImageResult);
        }

        [HttpGet("Latest")]
        public async Task<ActionResult<IEnumerable<MemberApiResponse>>> Latest(Guid chapterId)
        {
            return await HandleVersionedRequest(
                version => _memberService.GetLatestMembers(version, GetMemberId(), chapterId),
                x => x.Select(_mapper.Map<MemberApiResponse>));
        }

        [HttpGet("{id}/Profile")]
        public async Task<MemberProfileApiResponse> Profile(Guid id)
        {
            MemberProfile profile = await _memberService.GetMemberProfile(GetMemberId(), id);
            return _mapper.Map<MemberProfileApiResponse>(profile);
        }
    }
}
