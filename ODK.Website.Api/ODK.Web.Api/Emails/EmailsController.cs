using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using ODK.Services.Mails;

namespace ODK.Web.Api.Emails
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class EmailsController : OdkControllerBase
    {
        private readonly IFileProvider _fileProvider;
        private readonly IMailService _mailService;

        public EmailsController(IMailService mailService, IFileProvider fileProvider)
        {
            _fileProvider = fileProvider;
            _mailService = mailService;
        }

        [AllowAnonymous]
        [HttpGet("{id}/Read")]
        public async Task<IActionResult> Read(Guid id)
        {
            await _mailService.ConfirmEmailRead(id);

            IFileInfo fileInfo = _fileProvider.GetFileInfo("/Assets/EmailRead.jpg");
            return File(fileInfo.CreateReadStream(), "image/jpeg");
        }
    }
}
