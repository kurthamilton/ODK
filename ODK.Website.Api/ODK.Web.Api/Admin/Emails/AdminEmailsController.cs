using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ODK.Core.Chapters;
using ODK.Core.Emails;
using ODK.Services.Emails;
using ODK.Web.Api.Admin.Emails.Requests;
using ODK.Web.Api.Admin.Emails.Responses;

namespace ODK.Web.Api.Admin.Emails
{
    [Authorize]
    [ApiController]
    [Route("Admin/Emails")]
    public class AdminEmailsController : OdkControllerBase
    {
        private readonly IEmailAdminService _emailAdminService;
        private readonly IEmailService _emailService;
        private readonly IMapper _mapper;

        public AdminEmailsController(IEmailAdminService emailAdminService, IMapper mapper, IEmailService emailService)
        {
            _emailAdminService = emailAdminService;
            _emailService = emailService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IEnumerable<EmailApiResponse>> Get(Guid chapterId)
        {
            IReadOnlyCollection<Email> emails = await _emailAdminService.GetEmails(GetMemberId(), chapterId);
            return emails.Select(_mapper.Map<EmailApiResponse>);
        }

        [HttpPut("{type}")]
        public async Task<IActionResult> UpdateEmail(EmailType type, Guid chapterId,
            [FromForm] UpdateEmailApiRequest request)
        {
            UpdateEmail email = _mapper.Map<UpdateEmail>(request);
            await _emailAdminService.UpdateEmail(GetMemberId(), chapterId, type, email);
            return NoContent();
        }

        [HttpGet("Chapters/{id}")]
        public async Task<IEnumerable<ChapterEmailApiResponse>> GetChapterEmails(Guid id)
        {
            IReadOnlyCollection<ChapterEmail> emails = await _emailAdminService.GetChapterEmails(GetMemberId(), id);
            return emails.Select(_mapper.Map<ChapterEmailApiResponse>);
        }

        [HttpPut("Chapters/{id}/{type}")]
        public async Task<IActionResult> UpdateChapterEmail(Guid id, EmailType type,
            [FromForm] UpdateEmailApiRequest request)
        {
            UpdateEmail chapterEmail = _mapper.Map<UpdateEmail>(request);
            await _emailAdminService.UpdateChapterEmail(GetMemberId(), id, type, chapterEmail);
            return NoContent();
        }

        [HttpDelete("Chapters/{id}/{type}")]
        public async Task<IActionResult> DeleteChapterEmail(Guid id, EmailType type)
        {
            await _emailAdminService.DeleteChapterEmail(GetMemberId(), id, type);
            return NoContent();
        }

        [HttpGet("Chapters/{id}/Provider/Settings")]
        public async Task<ChapterEmailProviderSettingsApiResponse> GetChapterEmailProviderSettings(Guid id)
        {
            ChapterEmailProviderSettings settings = await _emailAdminService.GetChapterEmailProviderSettings(GetMemberId(), id);
            return settings != null
                ? _mapper.Map<ChapterEmailProviderSettingsApiResponse>(settings)
                : new ChapterEmailProviderSettingsApiResponse();
        }

        [HttpPut("Chapters/{id}/Provider/Settings")]
        public async Task<IActionResult> UpdateChapterEmailProviderSettings(Guid id,
            [FromForm] UpdateChapterEmailProviderSettingsApiRequest request)
        {
            UpdateChapterEmailProviderSettings emailProviderSettings = _mapper.Map<UpdateChapterEmailProviderSettings>(request);
            await _emailAdminService.UpdateChapterEmailProviderSettings(GetMemberId(), id, emailProviderSettings);
            return NoContent();
        }

        [HttpGet("Providers")]
        public async Task<IEnumerable<string>> GetEmailProviders()
        {
            IReadOnlyCollection<string> providers = await _emailAdminService.GetEmailProviders();
            return providers;
        }

        [HttpPost("Send")]
        public async Task<IActionResult> SendEmail([FromForm] SendEmailApiRequest request)
        {
            await _emailService.SendMail(GetMemberId(), request.MemberId, request.Subject, request.Body);
            return Created();
        }
    }
}
