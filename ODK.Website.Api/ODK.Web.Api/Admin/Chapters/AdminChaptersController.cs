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

        [HttpPut("{id}/Details")]
        public async Task<ActionResult<ChapterDetailsApiResponse>> UpdateChapterDetails(Guid id, [FromForm] UpdateChapterDetailsApiRequest request)
        {
            UpdateChapterDetails details = _mapper.Map<UpdateChapterDetails>(request);
            Chapter chapter = await _chapterAdminService.UpdateChapterDetails( GetMemberId(), id, details);
            return _mapper.Map<ChapterDetailsApiResponse>(chapter);
        }

        [HttpGet("{id}/Emails/Settings")]
        public async Task<ChapterEmailSettingsApiResponse> GetChapterEmailSettings(Guid id)
        {
            ChapterEmailSettings settings = await _chapterAdminService.GetChapterEmailSettings(GetMemberId(), id);
            return _mapper.Map<ChapterEmailSettingsApiResponse>(settings);
        }

        [HttpPut("{id}/Emails/Settings")]
        public async Task<IActionResult> UpdateChapterEmailSettings(Guid id, [FromForm] UpdateChapterEmailSettingsApiRequest request)
        {
            UpdateChapterEmailSettings emailSettings = _mapper.Map<UpdateChapterEmailSettings>(request);
            await _chapterAdminService.UpdateChapterEmailSettings(GetMemberId(), id, emailSettings);
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
    }
}
