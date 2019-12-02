using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using ODK.Core.Chapters;
using ODK.Core.Cryptography;
using ODK.Core.Mail;
using ODK.Core.Members;
using ODK.Core.Utils;
using ODK.Services.Authorization;
using ODK.Services.Exceptions;
using ODK.Services.Mails;

namespace ODK.Services.Authentication
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IChapterRepository _chapterRepository;
        private readonly IMailService _mailService;
        private readonly IMemberRepository _memberRepository;
        private readonly AuthenticationServiceSettings _settings;

        public AuthenticationService(IMemberRepository memberRepository, IMailService mailService, AuthenticationServiceSettings settings,
            IAuthorizationService authorizationService, IChapterRepository chapterRepository)
        {
            _authorizationService = authorizationService;
            _chapterRepository = chapterRepository;
            _mailService = mailService;
            _memberRepository = memberRepository;
            _settings = settings;
        }

        public async Task ActivateAccount(string activationToken, string password)
        {
            MemberActivationToken token = await _memberRepository.GetMemberActivationToken(activationToken);
            if (token == null)
            {
                return;
            }

            await UpdatePassword(token.MemberId, password);

            await _memberRepository.DeleteActivationToken(token.MemberId);
            await _memberRepository.ActivateMember(token.MemberId);

            // TODO: Send new member emails
        }

        public async Task ChangePassword(Guid memberId, string currentPassword, string newPassword)
        {
            await _authorizationService.AssertMemberIsCurrent(memberId);
            await AssertMemberPasswordMatches(memberId, currentPassword, "Current password is incorrect");
            await UpdatePassword(memberId, newPassword);
        }

        public async Task DeleteRefreshToken(string refreshToken)
        {
            MemberRefreshToken token = await _memberRepository.GetRefreshToken(refreshToken);
            if (token == null)
            {
                return;
            }

            await _memberRepository.DeleteRefreshToken(token);
        }

        public async Task<AuthenticationToken> Login(string username, string password)
        {
            const string message = "Username or password incorrect";

            Member member = await _memberRepository.FindMemberByEmailAddress(username);

            await AssertMemberPasswordMatches(member.Id, password, message);

            try
            {
                _authorizationService.AssertMemberIsCurrent(member);
            }
            catch
            {
                if (member.Disabled)
                {
                    throw new OdkServiceException("This account has been disabled");
                }

                throw;
            }

            return await GenerateAccessToken(member);
        }

        public async Task<AuthenticationToken> RefreshToken(string refreshToken)
        {
            const string message = "Invalid token";

            MemberRefreshToken memberRefreshToken = await _memberRepository.GetRefreshToken(refreshToken);
            if (memberRefreshToken == null)
            {
                throw new OdkServiceException(message);
            }

            if (memberRefreshToken.Expires < DateTime.UtcNow)
            {
                await _memberRepository.DeleteRefreshToken(memberRefreshToken);
                throw new OdkServiceException(message);
            }

            Member member = await _memberRepository.GetMember(memberRefreshToken.MemberId);
            _authorizationService.AssertMemberIsCurrent(member);

            AuthenticationToken authenticationToken = await GenerateAccessToken(member, memberRefreshToken.Expires);

            await _memberRepository.DeleteRefreshToken(memberRefreshToken);

            return authenticationToken;
        }

        public async Task RequestPasswordReset(string emailAddress)
        {
            Member member = await _memberRepository.FindMemberByEmailAddress(emailAddress);

            DateTime created = DateTime.UtcNow;
            DateTime expires = created.AddMinutes(_settings.PasswordResetTokenLifetimeMinutes);
            string token = RandomStringGenerator.Generate(64);

            try
            {
                _authorizationService.AssertMemberIsCurrent(member);
            }
            catch
            {
                return;
            }

            await _memberRepository.AddPasswordResetRequest(member.Id, created, expires, token);

            string url = _settings.PasswordResetUrl.Interpolate(new Dictionary<string, string> { { "token", token } });

            await _mailService.SendMemberMail(member, EmailType.PasswordReset, new Dictionary<string, string>
            {
                { "url", url }
            });
        }

        public async Task ResetPassword(string token, string password)
        {
            const string message = "Invalid token";

            MemberPasswordResetRequest request = await _memberRepository.GetPasswordResetRequest(token);
            if (request == null)
            {
                throw new OdkServiceException(message);
            }

            if (request.Expires < DateTime.UtcNow)
            {
                await _memberRepository.DeletePasswordResetRequest(request.Id);
                throw new OdkServiceException(message);
            }

            await UpdatePassword(request.MemberId, password);

            await _memberRepository.DeletePasswordResetRequest(request.Id);
        }

        private static void ValidatePassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                throw new OdkServiceException("Password cannot be blank");
            }
        }

        private async Task AssertMemberPasswordMatches(Guid memberId, string password, string message)
        {
            MemberPassword memberPassword = await _memberRepository.GetMemberPassword(memberId);
            if (memberPassword == null)
            {
                throw new OdkNotFoundException();
            }

            if (string.IsNullOrWhiteSpace(memberPassword.Password))
            {
                message = "Please use the forgotten password feature to reset your password. " +
                          "The website has been moved to a new system. " +
                          "Your password is stored securely and could not be moved across with the rest of your data.";
                throw new OdkServiceException(message);
            }

            if (PasswordHasher.ComputeHash(password, memberPassword.Salt) != memberPassword.Password)
            {
                throw new OdkServiceException(message);
            }
        }

        private async Task<AuthenticationToken> GenerateAccessToken(Member member, DateTime? refreshTokenExpires = null)
        {
            IReadOnlyCollection<ChapterAdminMember> adminChapterMembers = await _chapterRepository.GetChapterAdminMembers(member.Id);
            MemberSubscription subscription = await _memberRepository.GetMemberSubscription(member.Id);

            byte[] keyBytes = Encoding.ASCII.GetBytes(_settings.Key);
            SymmetricSecurityKey key = new SymmetricSecurityKey(keyBytes);

            JwtSecurityToken token = new JwtSecurityToken
            (
                claims: new []
                {
                    new Claim(ClaimTypes.NameIdentifier, member.Id.ToString())
                },
                notBefore: DateTime.UtcNow,
                expires: DateTime.UtcNow.AddMinutes(_settings.AccessTokenLifetimeMinutes),
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature)
            );

            string accessToken = new JwtSecurityTokenHandler().WriteToken(token);
            string refreshToken = await GenerateRefreshToken(member.Id, refreshTokenExpires);

            return new AuthenticationToken(member.Id, member.ChapterId, accessToken, refreshToken,
                adminChapterMembers.Select(x => x.ChapterId),
                adminChapterMembers.Any(x => x.SuperAdmin),
                subscription.ExpiryDate);
        }

        private async Task<string> GenerateRefreshToken(Guid memberId, DateTime? expires = null)
        {
            string refreshToken = RandomStringGenerator.Generate(64);

            if (expires == null)
            {
                expires = DateTime.UtcNow.AddDays(_settings.RefreshTokenLifetimeDays);
            }

            MemberRefreshToken token = new MemberRefreshToken(Guid.Empty, memberId, expires.Value, refreshToken);
            await _memberRepository.AddRefreshToken(token);

            return refreshToken;
        }

        private async Task UpdatePassword(Guid memberId, string password)
        {
            ValidatePassword(password);

            (string hash, string salt) = PasswordHasher.ComputeHash(password);

            await _memberRepository.UpdateMemberPassword(new MemberPassword(memberId, hash, salt));
        }
    }
}
