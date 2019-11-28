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
using ODK.Web.Api.Chapters.Responses;

namespace ODK.Web.Api.Admin.Chapters
{
    [Authorize]
    [ApiController]
    [Route("admin/chapters")]
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

        [HttpPut("{id}/links")]
        public async Task<IActionResult> UpdateLinks(Guid id, [FromForm] UpdateChapterLinksApiRequest request)
        {
            UpdateChapterLinks links = _mapper.Map<UpdateChapterLinks>(request);
            await _chapterAdminService.UpdateChapterLinks(GetMemberId(), id, links);
            return NoContent();
        }
    }
}
