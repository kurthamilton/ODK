using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ODK.Core.Members;
using ODK.Services.Members;
using ODK.Web.Api.Account.Requests;
using ODK.Web.Api.Account.Responses;
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
        [HttpPost("Activate")]
        public async Task<IActionResult> Activate([FromForm] ActivateAccountApiRequest request)
        {
            await _authenticationService.ActivateAccount(request.ActivationToken, request.Password);
            return Created();
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
            UpdateMemberImageApiRequest request = await FileToApiRequest(file);
            UpdateMemberImage image = _mapper.Map<UpdateMemberImage>(request);

            MemberImage updated = await _memberService.UpdateMemberImage(GetMemberId(), image);
            return File(updated.ImageData, updated.MimeType);
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
            UpdateMemberProfile update = _mapper.Map<UpdateMemberProfile>(request);

            MemberProfile profile = await _memberService.UpdateMemberProfile(GetMemberId(), update);

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

        [HttpDelete("RefreshToken")]
        public async Task<IActionResult> DeleteRefreshToken([FromForm] RefreshTokenApiRequest request)
        {
            await _authenticationService.DeleteRefreshToken(request.RefreshToken);
            return NoContent();
        }

        [AllowAnonymous]
        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromForm] CreateMemberProfileApiRequest request)
        {
            CreateMemberProfile profile = _mapper.Map<CreateMemberProfile>(request);
            await _memberService.CreateMember(request.ChapterId, profile);
            return Created();
        }

        [AllowAnonymous]
        [HttpPost("RequestPasswordReset")]
        public async Task<IActionResult> RequestPasswordReset([FromForm] RequestPasswordResetApiRequest request)
        {
            await _authenticationService.RequestPasswordReset(request.Username);
            return Created();
        }

        [HttpGet("Subscription")]
        public async Task<SubscriptionApiResponse> Subscription()
        {
            MemberSubscription subscription = await _memberService.GetMemberSubscription(GetMemberId());
            return _mapper.Map<SubscriptionApiResponse>(subscription);
        }
    }
}
