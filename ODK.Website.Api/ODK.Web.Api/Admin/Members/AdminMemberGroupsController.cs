using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ODK.Core.Members;
using ODK.Services.Members;
using ODK.Web.Api.Admin.Members.Requests;
using ODK.Web.Api.Admin.Members.Responses;

namespace ODK.Web.Api.Admin.Members
{
    [Authorize]
    [ApiController]
    [Route("admin/membergroups")]
    public class AdminMemberGroupsController : OdkControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IMemberAdminService _memberAdminService;

        public AdminMemberGroupsController(IMemberAdminService memberAdminService, IMapper mapper)
        {
            _mapper = mapper;
            _memberAdminService = memberAdminService;
        }

        [HttpPut("{id}/add")]
        public async Task<IActionResult> AddMember(Guid id, Guid memberId)
        {
            await _memberAdminService.AddMemberToGroup(GetMemberId(), memberId, id);
            return NoContent();
        }

        [HttpPost]
        public async Task<ActionResult<MemberGroupApiResponse>> Create([FromForm] CreateMemberGroupApiRequest request)
        {
            CreateMemberGroup create = _mapper.Map<CreateMemberGroup>(request);
            MemberGroup created = await _memberAdminService.CreateMemberGroup(GetMemberId(), create);
            MemberGroupApiResponse response = _mapper.Map<MemberGroupApiResponse>(created);
            return Created(response);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _memberAdminService.DeleteMemberGroup(GetMemberId(), id);
            return NoContent();
        }

        [HttpGet]
        public async Task<IEnumerable<MemberGroupApiResponse>> Get(Guid chapterId)
        {
            IReadOnlyCollection<MemberGroup> memberGroups = await _memberAdminService.GetMemberGroups(GetMemberId(), chapterId);
            return memberGroups.Select(_mapper.Map<MemberGroupApiResponse>);
        }

        [HttpGet("members")]
        public async Task<IEnumerable<MemberGroupMemberApiResponse>> GetMembers(Guid chapterId)
        {
            IReadOnlyCollection<MemberGroupMember> members = await _memberAdminService.GetMemberGroupMembers(GetMemberId(), chapterId);
            return members.Select(_mapper.Map<MemberGroupMemberApiResponse>);
        }

        [HttpPut("{id}/remove")]
        public async Task<IActionResult> RemoveMember(Guid id, Guid memberId)
        {
            await _memberAdminService.RemoveMemberFromGroup(GetMemberId(), memberId, id);
            return NoContent();
        }

        [HttpPut("{id}")]
        public async Task<MemberGroupApiResponse> Update(Guid id, [FromForm] CreateMemberGroupApiRequest request)
        {
            CreateMemberGroup update = _mapper.Map<CreateMemberGroup>(request);
            MemberGroup updated = await _memberAdminService.UpdateMemberGroup(GetMemberId(), id, update);
            return _mapper.Map<MemberGroupApiResponse>(updated);
        }
    }
}
