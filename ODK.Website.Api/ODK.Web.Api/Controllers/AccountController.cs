﻿using System;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ODK.Core.Members;
using ODK.Services.Members;
using ODK.Web.Common;
using ODK.Web.Common.Account.Requests;
using ODK.Web.Common.Account.Responses;
using ODK.Web.Common.Members.Responses;
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

        [HttpDelete]
        public async Task<IActionResult> DeleteAccount()
        {
            await _memberService.DeleteMember(GetMemberId());
            return NoContent();
        }

        [AllowAnonymous]
        [HttpPost("Activate")]
        public async Task<IActionResult> Activate([FromForm] ActivateAccountApiRequest request)
        {
            await _authenticationService.ActivateAccount(request.ActivationToken, request.Password);
            return Created();
        }

        [HttpPut("Emails/OptIn")]
        public async Task<IActionResult> UpdateEmailOptIn([FromForm] UpdateEmailOptInApiRequest request)
        {
            await _memberService.UpdateMemberEmailOptIn(GetMemberId(), request.OptIn);
            return NoContent();
        }

        [HttpPut("Image")]
        public async Task<IActionResult> UpdateImage([FromForm] IFormFile file)
        {
            UpdateMemberImageApiRequest request = await FileToApiRequest(file);
            UpdateMemberImage image = _mapper.Map<UpdateMemberImage>(request);

            MemberImage updated = await _memberService.UpdateMemberImage(GetMemberId(), image);
            return File(updated.ImageData, updated.MimeType);
        }

        [HttpPut("Image/Rotate/Right")]
        public async Task<IActionResult> MemberImageRotateRight()
        {
            MemberImage image = await _memberService.RotateMemberImage(GetMemberId(), 90);
            return MemberImageResult(image);
        }

        [HttpPut("Image/Rotate/Left")]
        public async Task<IActionResult> MemberImageRotateLeft()
        {
            MemberImage image = await _memberService.RotateMemberImage(GetMemberId(), -90);
            return MemberImageResult(image);
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

        [AllowAnonymous]
        [HttpPost("Password/Reset/Complete")]
        public async Task<IActionResult> CompleteResetPassword([FromForm] ResetPasswordApiRequest request)
        {
            await _authenticationService.ResetPassword(request.Token, request.Password);
            return Created();
        }

        [AllowAnonymous]
        [HttpPost("Password/Reset/Request")]
        public async Task<IActionResult> RequestPasswordReset([FromForm] RequestPasswordResetApiRequest request)
        {
            await _authenticationService.RequestPasswordReset(request.EmailAddress);
            return Created();
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

        [HttpPost("Profile/EmailAddress/Request")]
        public async Task<IActionResult> RequestEmailAddressUpdate([FromForm] UpdateEmailAddressApiRequest request)
        {
            await _memberService.RequestMemberEmailAddressUpdate(GetMemberId(), request.NewEmailAddress);
            return Created();
        }

        [HttpPost("Profile/EmailAddress/Confirm")]
        public async Task<IActionResult> ConfirmEmailAddressUpdate(string token)
        {
            await _memberService.ConfirmEmailAddressUpdate(GetMemberId(), token);
            return Created();
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

            UpdateMemberImageApiRequest requestImage = await FileToApiRequest(request.Image);
            profile.Image = _mapper.Map<UpdateMemberImage>(requestImage);

            await _memberService.CreateMember(request.ChapterId, profile);
            return Created();
        }

        [HttpGet("Subscription")]
        public async Task<SubscriptionApiResponse> Subscription()
        {
            MemberSubscription subscription = await _memberService.GetMemberSubscription(GetMemberId());
            return _mapper.Map<SubscriptionApiResponse>(subscription);
        }

        [HttpPost("Subscriptions/{id}/Purchase")]
        public async Task<SubscriptionApiResponse> PurchaseSubscription(Guid id, [FromForm] PurchaseSubscriptionApiRequest request)
        {
            MemberSubscription memberSubscription = await _memberService.PurchaseSubscription(GetMemberId(), id, request.Token);
            return _mapper.Map<SubscriptionApiResponse>(memberSubscription);
        }
    }
}
