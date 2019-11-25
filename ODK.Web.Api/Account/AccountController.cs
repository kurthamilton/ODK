using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ODK.Core.Members;
using ODK.Services.Members;
using ODK.Web.Api.Extensions;
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

        [HttpPut("Image")]
        public async Task UpdateImage([FromForm] IFormFile file)
        {
            byte[] imageData = await file.ToByteArrayAsync();
            
            MemberImage image = new MemberImage(GetMemberId(), imageData, file.ContentType);
            await _memberService.UpdateMemberImage(image);
        }

        [AllowAnonymous]
        [HttpPost("Login")]
        public async Task<AuthenticationTokenResponse> Login([FromForm] LoginRequest request)
        {
            AuthenticationToken token = await _authenticationService.Login(request.Username, request.Password);
            return _mapper.Map<AuthenticationTokenResponse>(token);
        }

        [HttpPut("Password")]
        public async Task ChangePassword([FromForm] ChangePasswordRequest request)
        {
            await _authenticationService.ChangePassword(GetMemberId(), request.CurrentPassword, request.NewPassword);
        }

        [HttpGet("Profile")]
        public async Task<AccountProfileResponse> Profile()
        {
            MemberProfile profile = await _memberService.GetMemberProfile(GetMemberId(), GetMemberId());
            return _mapper.Map<AccountProfileResponse>(profile);
        }

        [HttpPut("Profile")]
        public async Task UpdateProfile([FromForm] UpdateMemberProfileRequest request)
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

            await _memberService.UpdateMemberProfile(update);
        }

        [AllowAnonymous]
        [HttpPost("RefreshToken")]
        public async Task<AuthenticationTokenResponse> RefreshToken([FromForm] RefreshTokenRequest request)
        {
            AuthenticationToken token = await _authenticationService.RefreshToken(request.RefreshToken);
            return _mapper.Map<AuthenticationTokenResponse>(token);
        }

        [AllowAnonymous]
        [HttpPost("RequestPasswordReset")]
        public async Task RequestPasswordReset(string username)
        {
            await _authenticationService.RequestPasswordReset(username);
        }

        [AllowAnonymous]
        [HttpPost("ResetPassword")]
        public async Task ResetPassword([FromForm] ResetPasswordRequest request)
        {
            await _authenticationService.ResetPassword(request.Token, request.Password);            
        }        
    }
}
