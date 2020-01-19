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
using ODK.Web.Common;

namespace ODK.Web.Api.Admin.Emails
{
    [Authorize]
    [ApiController]
    [Route("Admin/Emails")]
    public class EmailsController : OdkControllerBase
    {
        private readonly IEmailAdminService _emailAdminService;
        private readonly IEmailService _emailService;
        private readonly IMapper _mapper;

        public EmailsController(IEmailAdminService emailAdminService, IMapper mapper, IEmailService emailService)
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

        [HttpGet("Chapters/{id}/Providers")]
        public async Task<IEnumerable<ChapterEmailProviderApiResponse>> GetChapterEmailProviders(Guid id)
        {
            IReadOnlyCollection<ChapterEmailProvider> providers = await _emailAdminService.GetChapterEmailProviders(GetMemberId(), id);
            return providers.Select(_mapper.Map<ChapterEmailProviderApiResponse>);
        }

        [HttpPost("Chapters/{id}/Providers")]
        public async Task<IActionResult> CreateChapterEmailProvider(Guid id, [FromForm] UpdateChapterEmailProviderApiRequest request)
        {
            UpdateChapterEmailProvider provider = _mapper.Map<UpdateChapterEmailProvider>(request);
            await _emailAdminService.AddChapterEmailProvider(GetMemberId(), id, provider);
            return Created();
        }

        [HttpGet("Chapters/{id}/Providers/{chapterEmailProviderId}")]
        public async Task<ChapterEmailProviderApiResponse> GetChapterEmailProvider(Guid id, Guid chapterEmailProviderId)
        {
            ChapterEmailProvider provider = await _emailAdminService.GetChapterEmailProvider(GetMemberId(), chapterEmailProviderId);
            return _mapper.Map<ChapterEmailProviderApiResponse>(provider);
        }

        [HttpPut("Chapters/{id}/Providers/{chapterEmailProviderId}")]
        public async Task<IActionResult> UpdateChapterEmailProvider(Guid id, Guid chapterEmailProviderId,
            [FromForm] UpdateChapterEmailProviderApiRequest request)
        {
            UpdateChapterEmailProvider provider = _mapper.Map<UpdateChapterEmailProvider>(request);
            await _emailAdminService.UpdateChapterEmailProvider(GetMemberId(), chapterEmailProviderId, provider);
            return NoContent();
        }

        [HttpDelete("Chapters/{id}/Providers/{chapterEmailProviderId}")]
        public async Task<IActionResult> DeleteChapterEmailProvider(Guid id, Guid chapterEmailProviderId)
        {
            await _emailAdminService.DeleteChapterEmailProvider(GetMemberId(), chapterEmailProviderId);
            return NoContent();
        }

        [HttpPost("Send")]
        public async Task<IActionResult> SendEmail([FromForm] SendEmailApiRequest request)
        {
            await _emailService.SendMemberEmail(GetMemberId(), request.MemberId, request.Subject, request.Body);
            return Created();
        }
    }
}
