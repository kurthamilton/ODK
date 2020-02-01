using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ODK.Core.Chapters;
using ODK.Services.Chapters;
using ODK.Web.Common;
using ODK.Web.Common.Chapters.Requests;
using ODK.Web.Common.Chapters.Responses;

namespace ODK.Web.Api.Chapters
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class ChaptersController : OdkControllerBase
    {
        private readonly IChapterService _chapterService;
        private readonly IMapper _mapper;

        public ChaptersController(IChapterService chapterService, IMapper mapper)
        {
            _chapterService = chapterService;
            _mapper = mapper;
        }

        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ChapterApiResponse>>> GetChapters()
        {
            return await HandleVersionedRequest(
                _chapterService.GetChapters,
                x => x.Select(_mapper.Map<ChapterApiResponse>));
        }

        [AllowAnonymous]
        [HttpPost("{id}/Contact")]
        public async Task<IActionResult> Contact(Guid id, [FromForm] ContactApiRequest request)
        {
            await _chapterService.SendContactMessage(id, request.EmailAddress, request.Message);
            return Created();
        }

        [AllowAnonymous]
        [HttpGet("{id}/Links")]
        public async Task<ActionResult<ChapterLinksApiResponse>> Links(Guid id)
        {
            return await HandleVersionedRequest(
                version => _chapterService.GetChapterLinks(version, id),
                x => _mapper.Map<ChapterLinksApiResponse>(x));
        }

        [AllowAnonymous]
        [HttpGet("{id}/Membership/Settings")]
        public async Task<ActionResult<ChapterMembershipSettingsApiResponse>> MembershipSettings(Guid id)
        {
            ChapterMembershipSettings settings = await _chapterService.GetChapterMembershipSettings(id);
            return _mapper.Map<ChapterMembershipSettingsApiResponse>(settings);
        }

        [HttpGet("{id}/Payments/Settings")]
        public async Task<ChapterPaymentSettingsApiResponse> PaymentSettings(Guid id)
        {
            ChapterPaymentSettings settings = await _chapterService.GetChapterPaymentSettings(GetMemberId(), id);
            return _mapper.Map<ChapterPaymentSettingsApiResponse>(settings);
        }

        [AllowAnonymous]
        [HttpGet("{id}/Properties")]
        public async Task<ActionResult<IEnumerable<ChapterPropertyApiResponse>>> Properties(Guid id)
        {
            return await HandleVersionedRequest(
                version => _chapterService.GetChapterProperties(version, id),
                x => x.Select(_mapper.Map<ChapterPropertyApiResponse>));
        }

        [AllowAnonymous]
        [HttpGet("{id}/PropertyOptions")]
        public async Task<ActionResult<IEnumerable<ChapterPropertyOptionApiResponse>>> PropertyOptions(Guid id)
        {
            return await HandleVersionedRequest(
                version => _chapterService.GetChapterPropertyOptions(version, id),
                x => x.Select(_mapper.Map<ChapterPropertyOptionApiResponse>));
        }

        [AllowAnonymous]
        [HttpGet("{id}/Questions")]
        public async Task<ActionResult<IEnumerable<ChapterQuestionApiResponse>>> Questions(Guid id)
        {
            return await HandleVersionedRequest(
                version => _chapterService.GetChapterQuestions(version, id),
                x => x.Select(_mapper.Map<ChapterQuestionApiResponse>));
        }

        [AllowAnonymous]
        [HttpGet("{id}/Subscriptions")]
        public async Task<IEnumerable<ChapterSubscriptionApiResponse>> Subscriptions(Guid id)
        {
            IReadOnlyCollection<ChapterSubscription> chapters = await _chapterService.GetChapterSubscriptions(id);
            return chapters.Select(_mapper.Map<ChapterSubscriptionApiResponse>);
        }

        [AllowAnonymous]
        [HttpGet("{id}/Texts")]
        public async Task<ActionResult<ChapterTextsApiResponse>> Texts(Guid id)
        {
            return await HandleVersionedRequest(
                version => _chapterService.GetChapterTexts(version, id),
                x => _mapper.Map<ChapterTextsApiResponse>(x));
        }
    }
}
