using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ODK.Core.Chapters;
using ODK.Services.Chapters;
using ODK.Web.Api.Admin.Chapters.Requests;
using ODK.Web.Api.Admin.Chapters.Responses;
using ODK.Web.Common;
using ODK.Web.Common.Chapters.Responses;

namespace ODK.Web.Api.Admin.Chapters
{
    [Authorize]
    [ApiController]
    [Route("Admin/Chapters")]
    public class ChaptersController : OdkControllerBase
    {
        private readonly IChapterAdminService _chapterAdminService;
        private readonly IMapper _mapper;

        public ChaptersController(IChapterAdminService chapterAdminService, IMapper mapper)
        {
            _chapterAdminService = chapterAdminService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IEnumerable<ChapterApiResponse>> Get()
        {
            IReadOnlyCollection<Chapter> chapters = await _chapterAdminService.GetChapters(GetMemberId());
            return chapters.Select(_mapper.Map<ChapterApiResponse>);
        }

        [HttpPost("{id}/AdminMembers")]
        public async Task<IActionResult> AddChapterAdminMember(Guid id, [FromForm] AddChapterAdminMemberApiRequest request)
        {
            await _chapterAdminService.AddChapterAdminMember(GetMemberId(), id, request.MemberId);
            return Created();
        }

        [HttpGet("{id}/AdminMembers")]
        public async Task<IEnumerable<ChapterAdminMemberApiResponse>> GetChapterAdminMembers(Guid id)
        {
            IReadOnlyCollection<ChapterAdminMember> adminMembers = await _chapterAdminService.GetChapterAdminMembers(GetMemberId(), id);
            return adminMembers.Select(_mapper.Map<ChapterAdminMemberApiResponse>);
        }

        [HttpGet("{id}/AdminMembers/{memberId}")]
        public async Task<ChapterAdminMemberApiResponse> GetChapterAdminMember(Guid id, Guid memberId)
        {
            ChapterAdminMember adminMember = await _chapterAdminService.GetChapterAdminMember(GetMemberId(), id, memberId);
            return _mapper.Map<ChapterAdminMemberApiResponse>(adminMember);
        }

        [HttpPut("{id}/AdminMembers/{memberId}")]
        public async Task<IActionResult> UpdateChapterAdminMember(Guid id, Guid memberId,
            [FromForm] UpdateChapterAdminMemberApiRequest request)
        {
            UpdateChapterAdminMember adminMember = _mapper.Map<UpdateChapterAdminMember>(request);
            await _chapterAdminService.UpdateChapterAdminMember(GetMemberId(), id, memberId, adminMember);
            return NoContent();
        }

        [HttpDelete("{id}/AdminMembers/{memberId}")]
        public async Task<IActionResult> DeleteChapterAdminMember(Guid id, Guid memberId)
        {
            await _chapterAdminService.DeleteChapterAdminMember(GetMemberId(), id, memberId);
            return NoContent();
        }

        [HttpPut("{id}/Links")]
        public async Task<IActionResult> UpdateLinks(Guid id, [FromForm] UpdateChapterLinksApiRequest request)
        {
            UpdateChapterLinks links = _mapper.Map<UpdateChapterLinks>(request);
            await _chapterAdminService.UpdateChapterLinks(GetMemberId(), id, links);
            return NoContent();
        }

        [HttpGet("{id}/Membership/Settings")]
        public async Task<ChapterAdminMembershipSettingsApiResponse> GetMembershipSettings(Guid id)
        {
            ChapterMembershipSettings settings = await _chapterAdminService.GetChapterMembershipSettings(GetMemberId(), id);
            return _mapper.Map<ChapterAdminMembershipSettingsApiResponse>(settings);
        }

        [HttpPut("{id}/Membership/Settings")]
        public async Task<IActionResult> UpdateMembershipSettings(Guid id, [FromForm] UpdateChapterMembershipSettingsApiRequest request)
        {
            UpdateChapterMembershipSettings settings = _mapper.Map<UpdateChapterMembershipSettings>(request);
            await _chapterAdminService.UpdateChapterMembershipSettings(GetMemberId(), id, settings);
            return NoContent();
        }

        [HttpGet("{id}/Payments/Settings")]
        public async Task<ChapterAdminPaymentSettingsApiResponse> PaymentSettings(Guid id)
        {
            ChapterPaymentSettings settings = await _chapterAdminService.GetChapterPaymentSettings(GetMemberId(), id);
            return _mapper.Map<ChapterAdminPaymentSettingsApiResponse>(settings);
        }

        [HttpPut("{id}/Payments/Settings")]
        public async Task<ChapterAdminPaymentSettingsApiResponse> UpdatePaymentSettings(Guid id,
            [FromForm] UpdateChapterPaymentSettingsApiRequest request)
        {
            UpdateChapterPaymentSettings update = _mapper.Map<UpdateChapterPaymentSettings>(request);
            ChapterPaymentSettings settings = await _chapterAdminService.UpdateChapterPaymentSettings(GetMemberId(), id, update);
            return _mapper.Map<ChapterAdminPaymentSettingsApiResponse>(settings);
        }

        [HttpGet("{id}/Properties")]
        public async Task<IEnumerable<ChapterPropertyApiResponse>> GetProperties(Guid id)
        {
            IReadOnlyCollection<ChapterProperty> properties = await _chapterAdminService.GetChapterProperties(GetMemberId(), id);
            return properties.Select(_mapper.Map<ChapterPropertyApiResponse>);
        }

        [HttpPost("{id}/Properties")]
        public async Task<IActionResult> CreateProperty(Guid id, [FromForm] CreateChapterPropertyApiRequest request)
        {
            CreateChapterProperty property = _mapper.Map<CreateChapterProperty>(request);
            await _chapterAdminService.CreateChapterProperty(GetMemberId(), id, property);
            return Created();
        }

        [HttpPost("{id}/Questions")]
        public async Task<IActionResult> CreateQuestion(Guid id, [FromForm] CreateChapterQuestionApiRequest request)
        {
            CreateChapterQuestion question = _mapper.Map<CreateChapterQuestion>(request);
            await _chapterAdminService.CreateChapterQuestion(GetMemberId(), id, question);
            return Created();
        }

        [HttpPost("{id}/Subscriptions")]
        public async Task<IActionResult> CreateSubscription(Guid id, [FromForm] CreateChapterSubscriptionApiRequest request)
        {
            CreateChapterSubscription subscription = _mapper.Map<CreateChapterSubscription>(request);
            await _chapterAdminService.CreateChapterSubscription(GetMemberId(), id, subscription);
            return Created();
        }

        [HttpPut("{id}/Texts")]
        public async Task<ActionResult<ChapterTextsApiResponse>> UpdateChapterTexts(Guid id, [FromForm] UpdateChapterTextsApiRequest request)
        {
            UpdateChapterTexts texts = _mapper.Map<UpdateChapterTexts>(request);
            ChapterTexts updated = await _chapterAdminService.UpdateChapterTexts(GetMemberId(), id, texts);
            return _mapper.Map<ChapterTextsApiResponse>(updated);
        }

        [HttpGet("Properties/{id}")]
        public async Task<ChapterPropertyApiResponse> GetProperty(Guid id)
        {
            ChapterProperty property = await _chapterAdminService.GetChapterProperty(GetMemberId(), id);
            return _mapper.Map<ChapterPropertyApiResponse>(property);
        }

        [HttpPut("Properties/{id}")]
        public async Task<IActionResult> UpdateProperty(Guid id, [FromForm] UpdateChapterPropertyApiRequest request)
        {
            UpdateChapterProperty property = _mapper.Map<UpdateChapterProperty>(request);
            await _chapterAdminService.UpdateChapterProperty(GetMemberId(), id, property);
            return NoContent();
        }

        [HttpPut("Properties/{id}/MoveDown")]
        public async Task<IEnumerable<ChapterPropertyApiResponse>> MovePropertyDown(Guid id)
        {
            IReadOnlyCollection<ChapterProperty> properties =
                await _chapterAdminService.UpdateChapterPropertyDisplayOrder(GetMemberId(), id, 1);
            return properties.Select(_mapper.Map<ChapterPropertyApiResponse>);
        }

        [HttpPut("Properties/{id}/MoveUp")]
        public async Task<IEnumerable<ChapterPropertyApiResponse>> MovePropertyUp(Guid id)
        {
            IReadOnlyCollection<ChapterProperty> properties =
                await _chapterAdminService.UpdateChapterPropertyDisplayOrder(GetMemberId(), id, -1);
            return properties.Select(_mapper.Map<ChapterPropertyApiResponse>);
        }

        [HttpDelete("Properties/{id}")]
        public async Task<IActionResult> DeleteProperty(Guid id)
        {
            await _chapterAdminService.DeleteChapterProperty(GetMemberId(), id);
            return NoContent();
        }

        [HttpGet("Questions/{id}")]
        public async Task<ChapterQuestionApiResponse> GetQuestion(Guid id)
        {
            ChapterQuestion question = await _chapterAdminService.GetChapterQuestion(GetMemberId(), id);
            return _mapper.Map<ChapterQuestionApiResponse>(question);
        }

        [HttpPut("Questions/{id}")]
        public async Task<IActionResult> UpdateQuestion(Guid id, [FromForm] CreateChapterQuestionApiRequest request)
        {
            CreateChapterQuestion question = _mapper.Map<CreateChapterQuestion>(request);
            await _chapterAdminService.UpdateChapterQuestion(GetMemberId(), id, question);
            return NoContent();
        }

        [HttpPut("Questions/{id}/MoveDown")]
        public async Task<IEnumerable<ChapterQuestionApiResponse>> MoveQuestionDown(Guid id)
        {
            IReadOnlyCollection<ChapterQuestion> questions =
                await _chapterAdminService.UpdateChapterQuestionDisplayOrder(GetMemberId(), id, 1);
            return questions.Select(_mapper.Map<ChapterQuestionApiResponse>);
        }

        [HttpPut("Questions/{id}/MoveUp")]
        public async Task<IEnumerable<ChapterQuestionApiResponse>> MoveQuestionUp(Guid id)
        {
            IReadOnlyCollection<ChapterQuestion> questions =
                await _chapterAdminService.UpdateChapterQuestionDisplayOrder(GetMemberId(), id, -1);
            return questions.Select(_mapper.Map<ChapterQuestionApiResponse>);
        }

        [HttpDelete("Questions/{id}")]
        public async Task<IActionResult> DeleteQuestion(Guid id)
        {
            await _chapterAdminService.DeleteChapterQuestion(GetMemberId(), id);
            return NoContent();
        }

        [HttpGet("Subscriptions/{id}")]
        public async Task<ChapterSubscriptionApiResponse> GetSubscription(Guid id)
        {
            ChapterSubscription subscription = await _chapterAdminService.GetChapterSubscription(GetMemberId(), id);
            return _mapper.Map<ChapterSubscriptionApiResponse>(subscription);
        }

        [HttpPut("Subscriptions/{id}")]
        public async Task<IActionResult> UpdateSubscription(Guid id, [FromForm] CreateChapterSubscriptionApiRequest request)
        {
            CreateChapterSubscription subscription = _mapper.Map<CreateChapterSubscription>(request);
            await _chapterAdminService.UpdateChapterSubscription(GetMemberId(), id, subscription);
            return NoContent();
        }

        [HttpDelete("Subscriptions/{id}")]
        public async Task<IActionResult> DeleteSubscription(Guid id)
        {
            await _chapterAdminService.DeleteChapterSubscription(GetMemberId(), id);
            return NoContent();
        }
    }
}
