using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ODK.Core.Members;
using ODK.Services.Members;
using ODK.Web.Api.Account.Requests;
using ODK.Web.Api.Members.Responses;

namespace ODK.Web.Api.Admin.Members
{
    [Authorize]
    [ApiController]
    [Route("admin/members")]
    public class AdminMembersController : OdkControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IMemberAdminService _memberAdminService;

        public AdminMembersController(IMemberAdminService memberAdminService, IMapper mapper)
        {
            _mapper = mapper;
            _memberAdminService = memberAdminService;
        }

        [HttpGet]
        public async Task<IEnumerable<MemberApiResponse>> Get(Guid chapterId)
        {
            IReadOnlyCollection<Member> members = await _memberAdminService.GetMembers(GetMemberId(), chapterId);
            return members.Select(_mapper.Map<MemberApiResponse>);
        }

        [HttpPut("{id}/Disable")]
        public async Task<IActionResult> DisableMember(Guid id)
        {
            await _memberAdminService.DisableMember(GetMemberId(), id);
            return Ok();
        }

        [HttpPut("{id}/Enable")]
        public async Task<IActionResult> EnableMember(Guid id)
        {
            await _memberAdminService.EnableMember(GetMemberId(), id);
            return Ok();
        }

        [HttpPut("{id}/Image")]
        public async Task<IActionResult> UpdateImage(Guid id, [FromForm] IFormFile file)
        {
            UpdateMemberImageApiRequest request = await FileToApiRequest(file);
            UpdateMemberImage image = _mapper.Map<UpdateMemberImage>(request);

            MemberImage updated = await _memberAdminService.UpdateMemberImage(GetMemberId(), id, image);
            return MemberImageResult(updated);
        }

        [HttpPut("{id}/Image/RotateRight")]
        public async Task<IActionResult> RotateImageRight(Guid id)
        {
            MemberImage rotated = await _memberAdminService.RotateMemberImage(GetMemberId(), id, 90);
            return MemberImageResult(rotated);
        }

        [HttpPut("{id}/Image/RotateLeft")]
        public async Task<IActionResult> RotateImageLeft(Guid id)
        {
            MemberImage rotated = await _memberAdminService.RotateMemberImage(GetMemberId(), id, -90);
            return MemberImageResult(rotated);
        }
    }
}
