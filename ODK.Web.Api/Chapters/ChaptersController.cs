using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ODK.Services.Chapters;
using ODK.Web.Api.Chapters.Requests;
using ODK.Web.Api.Chapters.Responses;

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
        public async Task<ActionResult<IEnumerable<ChapterApiResponse>>> Get()
        {
            return await HandleVersionedRequest(
                _chapterService.GetChapters,
                x => x.Select(_mapper.Map<ChapterApiResponse>));
        }

        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<ActionResult<ChapterDetailsApiResponse>> Get(Guid id)
        {
            return await HandleVersionedRequest(
                version => _chapterService.GetChapter(version, id),
                x => _mapper.Map<ChapterDetailsApiResponse>(x));
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
    }
}
