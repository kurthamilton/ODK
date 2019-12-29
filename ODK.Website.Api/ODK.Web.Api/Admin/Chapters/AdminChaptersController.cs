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
using ODK.Web.Api.Chapters.Responses;

namespace ODK.Web.Api.Admin.Chapters
{
    [Authorize]
    [ApiController]
    [Route("Admin/Chapters")]
    public class AdminChaptersController : OdkControllerBase
    {
        private readonly IChapterAdminService _chapterAdminService;
        private readonly IMapper _mapper;

        public AdminChaptersController(IChapterAdminService chapterAdminService, IMapper mapper)
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

        [HttpGet("{id}/Payments/Settings")]
        public async Task<ChapterAdminPaymentSettingsApiResponse> PaymentSettings(Guid id)
        {
            ChapterPaymentSettings paymentSettings = await _chapterAdminService.GetChapterPaymentSettings(GetMemberId(), id);
            return _mapper.Map<ChapterAdminPaymentSettingsApiResponse>(paymentSettings);
        }

        [HttpPut("{id}/Payments/Settings")]
        public async Task<ChapterAdminPaymentSettingsApiResponse> UpdatePaymentSettings(Guid id,
            [FromForm] UpdateChapterPaymentSettingsApiRequest request)
        {
            UpdateChapterPaymentSettings update = _mapper.Map<UpdateChapterPaymentSettings>(request);
            ChapterPaymentSettings paymentSettings = await _chapterAdminService.UpdateChapterPaymentSettings(GetMemberId(), id, update);
            return _mapper.Map<ChapterAdminPaymentSettingsApiResponse>(paymentSettings);
        }

        [HttpPost("{id}/Questions")]
        public async Task<IActionResult> CreateQuestion(Guid id, [FromForm] CreateChapterQuestionApiRequest request)
        {
            CreateChapterQuestion question = _mapper.Map<CreateChapterQuestion>(request);
            await _chapterAdminService.CreateChapterQuestion(GetMemberId(), id, question);
            return Created();
        }

        [HttpPut("{id}/Texts")]
        public async Task<ActionResult<ChapterTextsApiResponse>> UpdateChapterTexts(Guid id, [FromForm] UpdateChapterTextsApiRequest request)
        {
            UpdateChapterTexts texts = _mapper.Map<UpdateChapterTexts>(request);
            ChapterTexts updated = await _chapterAdminService.UpdateChapterTexts(GetMemberId(), id, texts);
            return _mapper.Map<ChapterTextsApiResponse>(updated);
        }
    }
}
