using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ODK.Core.Members;
using ODK.Services.Files;
using ODK.Services.Members;
using ODK.Web.Api.Admin.Members.Requests;
using ODK.Web.Common;
using ODK.Web.Common.Account.Requests;
using ODK.Web.Common.Account.Responses;
using ODK.Web.Common.Members.Responses;

namespace ODK.Web.Api.Admin.Members
{
    [Authorize]
    [ApiController]
    [Route("Admin/Members")]
    public class MembersController : OdkControllerBase
    {
        private readonly ICsvService _csvService;
        private readonly IMapper _mapper;
        private readonly IMemberAdminService _memberAdminService;

        public MembersController(IMemberAdminService memberAdminService, IMapper mapper, ICsvService csvService)
        {
            _csvService = csvService;
            _mapper = mapper;
            _memberAdminService = memberAdminService;
        }

        [HttpGet]
        public async Task<IEnumerable<MemberApiResponse>> Get(Guid chapterId)
        {
            IReadOnlyCollection<Member> members = await _memberAdminService.GetMembers(GetMemberId(), chapterId);
            return members.Select(_mapper.Map<MemberApiResponse>);
        }

        [HttpGet("Subscriptions")]
        public async Task<IEnumerable<SubscriptionApiResponse>> GetSubscriptions(Guid chapterId)
        {
            IReadOnlyCollection<MemberSubscription> subscriptions = await _memberAdminService.GetMemberSubscriptions(GetMemberId(), chapterId);
            return subscriptions.Select(_mapper.Map<SubscriptionApiResponse>);
        }

        [HttpGet("{id}")]
        public async Task<MemberApiResponse> GetMember(Guid id)
        {
            Member member = await _memberAdminService.GetMember(GetMemberId(), id);
            return _mapper.Map<MemberApiResponse>(member);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMember(Guid id)
        {
            await _memberAdminService.DeleteMember(GetMemberId(), id);
            return NoContent();
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

        [HttpPut("{id}/Image/Rotate/Right")]
        public async Task<IActionResult> RotateImageRight(Guid id)
        {
            MemberImage rotated = await _memberAdminService.RotateMemberImage(GetMemberId(), id, 90);
            return MemberImageResult(rotated);
        }

        [HttpPut("{id}/Image/Rotate/Left")]
        public async Task<IActionResult> RotateImageLeft(Guid id)
        {
            MemberImage rotated = await _memberAdminService.RotateMemberImage(GetMemberId(), id, -90);
            return MemberImageResult(rotated);
        }

        [HttpGet("{id}/Subscription")]
        public async Task<SubscriptionApiResponse> GetMemberSubscription(Guid id)
        {
            MemberSubscription subscription = await _memberAdminService.GetMemberSubscription(GetMemberId(), id);
            return _mapper.Map<SubscriptionApiResponse>(subscription);
        }

        [HttpPut("{id}/Subscription")]
        public async Task<SubscriptionApiResponse> UpdateMemberSubscription(Guid id,
            [FromForm] UpdateMemberSubscriptionApiRequest request)
        {
            UpdateMemberSubscription update = _mapper.Map<UpdateMemberSubscription>(request);

            MemberSubscription updated = await _memberAdminService.UpdateMemberSubscription(GetMemberId(), id, update);
            return _mapper.Map<SubscriptionApiResponse>(updated);
        }

        [HttpPost("Import")]
        public async Task<IActionResult> ImportMembers(Guid chapterId, [FromForm] IFormFile file)
        {
            using Stream stream = file.OpenReadStream();

            CsvFile csvFile = _csvService.ParseCsvFile(stream);

            await _memberAdminService.ImportMembers(GetMemberId(), chapterId, csvFile);
            return NoContent();
        }

        [HttpGet("Import/Template")]
        public async Task<IActionResult> GetMemberImportTemplate(Guid chapterId)
        {
            CsvFile file = await _memberAdminService.GetMemberImportFile(GetMemberId(), chapterId);
            return CsvResult(file, "Members.csv");
        }
    }
}
