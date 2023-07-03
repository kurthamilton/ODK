using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ODK.Core.Chapters;
using ODK.Services;
using ODK.Services.Chapters;
using ODK.Services.Events;
using ODK.Web.Razor.Models.Admin.Events;

namespace ODK.Web.Razor.Controllers
{
    [Authorize(Roles = "Admin")]
    public class EventAdminController : OdkControllerBase
    {
        private readonly IChapterService _chapterService;
        private readonly IEventAdminService _eventAdminService;

        public EventAdminController(IEventAdminService eventAdminService, IChapterService chapterService)
        {
            _chapterService = chapterService;
            _eventAdminService = eventAdminService;
        }
    }
}
