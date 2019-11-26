using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ODK.Core.Chapters;
using ODK.Services.Chapters;
using ODK.Web.Api.Chapters.Responses;

namespace ODK.Web.Api.Chapters
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class ChaptersController : ControllerBase
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
        public async Task<IEnumerable<ChapterApiResponse>> Get()
        {
            IReadOnlyCollection<Chapter> chapters = await _chapterService.GetChapters();
            return chapters.Select(_mapper.Map<ChapterApiResponse>);
        }

        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<ChapterDetailsApiResponse> Get(Guid id)
        {
            Chapter chapter = await _chapterService.GetChapter(id);
            return _mapper.Map<ChapterDetailsApiResponse>(chapter);
        }

        [AllowAnonymous]
        [HttpGet("{id}/Links")]
        public async Task<ChapterLinksApiResponse> Links(Guid id)
        {
            ChapterLinks links = await _chapterService.GetChapterLinks(id);
            return _mapper.Map<ChapterLinksApiResponse>(links);
        }

        [AllowAnonymous]
        [HttpGet("{id}/Properties")]
        public async Task<IEnumerable<ChapterPropertyApiResponse>> Properties(Guid id)
        {
            IReadOnlyCollection<ChapterProperty> properties = await _chapterService.GetChapterProperties(id);
            return properties.Select(_mapper.Map<ChapterPropertyApiResponse>);
        }

        [AllowAnonymous]
        [HttpGet("{id}/PropertyOptions")]
        public async Task<IEnumerable<ChapterPropertyOptionApiResponse>> PropertyOptions(Guid id)
        {
            IReadOnlyCollection<ChapterPropertyOption> options = await _chapterService.GetChapterPropertyOptions(id);
            return options.Select(_mapper.Map<ChapterPropertyOptionApiResponse>);
        }
    }
}
