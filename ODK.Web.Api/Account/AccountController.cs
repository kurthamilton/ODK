using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ODK.Core.Members;
using ODK.Services.Members;
using ODK.Web.Api.Account.Requests;
using ODK.Web.Api.Account.Responses;
using ODK.Web.Api.Extensions;
using ODK.Web.Api.Members.Responses;
using AuthenticationToken = ODK.Services.Authentication.AuthenticationToken;
using IAuthenticationService = ODK.Services.Authentication.IAuthenticationService;

namespace ODK.Web.Api.Account
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class AccountController : OdkControllerBase
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly IMapper _mapper;
        private readonly IMemberService _memberService;

        public AccountController(IAuthenticationService authenticationService, IMemberService memberService, IMapper mapper)
        {
            _authenticationService = authenticationService;
            _mapper = mapper;
            _memberService = memberService;
        }

        [AllowAnonymous]
        [HttpPost("CompletePasswordReset")]
        public async Task<IActionResult> CompleteResetPassword([FromForm] ResetPasswordApiRequest request)
        {
            await _authenticationService.ResetPassword(request.Token, request.Password);
            return Created();
        }

        [HttpPut("Image")]
        public async Task<IActionResult> UpdateImage([FromForm] IFormFile file)
        {
            byte[] imageData = await file.ToByteArrayAsync();

            MemberImage image = new MemberImage(GetMemberId(), imageData, file.ContentType);
            image = await _memberService.UpdateMemberImage(image);
            return File(image.ImageData, image.MimeType);
        }

        [AllowAnonymous]
        [HttpPost("Login")]
        public async Task<ActionResult<AuthenticationTokenApiResponse>> Login([FromForm] LoginApiRequest request)
        {
            AuthenticationToken token = await _authenticationService.Login(request.Username, request.Password);
            AuthenticationTokenApiResponse response = _mapper.Map<AuthenticationTokenApiResponse>(token);
            return Created(response);
        }

        [HttpPut("Password")]
        public async Task<IActionResult> ChangePassword([FromForm] ChangePasswordApiRequest request)
        {
            await _authenticationService.ChangePassword(GetMemberId(), request.CurrentPassword, request.NewPassword);
            return NoContent();
        }

        [HttpGet("Profile")]
        public async Task<AccountProfileApiResponse> Profile()
        {
            MemberProfile profile = await _memberService.GetMemberProfile(GetMemberId(), GetMemberId());
            return _mapper.Map<AccountProfileApiResponse>(profile);
        }

        [HttpPut("Profile")]
        public async Task<MemberProfileApiResponse> UpdateProfile([FromForm] UpdateMemberProfileApiRequest request)
        {
            UpdateMemberProfile update = new UpdateMemberProfile
            {
                EmailAddress = request.EmailAddress,
                EmailOptIn = request.EmailOptIn,
                FirstName = request.FirstName,
                Id = GetMemberId(),
                LastName = request.LastName,
                Properties = request.Properties.Select(x => new UpdateMemberProperty
                {
                    ChapterPropertyId = x.ChapterPropertyId,
                    Value = x.Value
                })
            };

            MemberProfile profile = await _memberService.UpdateMemberProfile(update);

            return _mapper.Map<MemberProfileApiResponse>(profile);
        }

        [AllowAnonymous]
        [HttpPost("RefreshToken")]
        public async Task<ActionResult<AuthenticationTokenApiResponse>> RefreshToken([FromForm] RefreshTokenApiRequest request)
        {
            AuthenticationToken token = await _authenticationService.RefreshToken(request.RefreshToken);
            AuthenticationTokenApiResponse response = _mapper.Map<AuthenticationTokenApiResponse>(token);
            return Created(response);
        }

        [AllowAnonymous]
        [HttpPost("RequestPasswordReset")]
        public async Task<IActionResult> RequestPasswordReset(string username)
        {
            await _authenticationService.RequestPasswordReset(username);
            return Created();
        }
    }
}
