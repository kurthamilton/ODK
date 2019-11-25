using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ODK.Core.Chapters;
using ODK.Services.Chapters;
using ODK.Web.Api.Chapters;

namespace ODK.Web.Api.Admin.Chapters
{
    [Authorize]
    [ApiController]
    [Route("admin/chapters")]
    public class AdminChaptersController : OdkControllerBase
    {
        private readonly IChapterService _chapterService;
        private readonly IMapper _mapper;

        public AdminChaptersController(IChapterService chapterService, IMapper mapper)
        {
            _chapterService = chapterService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IEnumerable<ChapterResponse>> Get()
        {
            IReadOnlyCollection<Chapter> chapters = await _chapterService.GetAdminChapters(GetMemberId());
            return chapters.Select(_mapper.Map<ChapterResponse>);
        }
    }
}
