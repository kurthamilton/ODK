using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using ODK.Core.Members;
using ODK.Services.Exceptions;
using ODK.Services.Mails;

namespace ODK.Services.Authentication
{
    public class AuthenticationService : IAuthenticationService
    {        
        private readonly IMailService _mailService;
        private readonly IMemberRepository _memberRepository;
        private readonly AuthenticationSettings _settings;

        public AuthenticationService(IMemberRepository memberRepository, IMailService mailService, AuthenticationSettings settings)
        {
            _mailService = mailService;
            _memberRepository = memberRepository;
            _settings = settings;
        }

        public async Task ChangePassword(Guid memberId, string currentPassword, string newPassword)
        {
            await AssertMemberPasswordMatches(memberId, currentPassword, "Current password is incorrect");
            await UpdatePassword(memberId, newPassword);
        }

        public async Task<AuthenticationToken> Login(string username, string password)
        {
            const string message = "Username or password incorrect";

            Member member = await _memberRepository.FindMemberByEmailAddress(username);
            if (member == null)
            {
                throw new OdkServiceException(message);
            }


            await AssertMemberPasswordMatches(member.Id, password, message);

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
                await _memberRepository.DeleteRefreshToken(memberRefreshToken.Id);
                throw new OdkServiceException(message);
            }

            Member member = await _memberRepository.GetMember(memberRefreshToken.MemberId);

            AuthenticationToken authenticationToken = await GenerateAccessToken(member, memberRefreshToken.Expires);

            await _memberRepository.DeleteRefreshToken(memberRefreshToken.Id);

            return authenticationToken;
        }

        public async Task RequestPasswordReset(string username)
        {
            Member member = await _memberRepository.FindMemberByEmailAddress(username);
            if (member == null)
            {
                return;
            }

            DateTime created = DateTime.UtcNow;
            DateTime expires = created.AddHours(1);
            string token = GenerateRandomString(64);
            await _memberRepository.AddPasswordResetRequest(member.Id, created, expires, token);

            string from = "noreply@drunkenknitwits.com";
            string[] to = new[] { member.EmailAddress };
            string subject = "Password reset request";
            string body = "Body";

            await _mailService.SendMail(from, to, subject, body);
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

            return;
        }

        private static string GenerateRandomString(int length)
        {
            byte[] randomNumber = new byte[length];
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
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

            if (PasswordHasher.ComputeHash(password, memberPassword.Salt) != memberPassword.Password)
            {
                throw new OdkServiceException(message);
            }
        }

        private async Task<AuthenticationToken> GenerateAccessToken(Member member, DateTime? refreshTokenExpires = null)
        {
            byte[] keyBytes = Encoding.ASCII.GetBytes(_settings.Key);
            SymmetricSecurityKey key = new SymmetricSecurityKey(keyBytes);

            JwtSecurityToken token = new JwtSecurityToken
            (
                claims: new Claim[]
                {
                    new Claim(ClaimTypes.NameIdentifier, member.Id.ToString())
                },
                notBefore: DateTime.UtcNow,
                expires: DateTime.UtcNow.AddMinutes(_settings.AccessTokenLifetimeMinutes),
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature)
            );

            string accessToken = new JwtSecurityTokenHandler().WriteToken(token);
            string refreshToken = await GenerateRefreshToken(member.Id, refreshTokenExpires);

            return new AuthenticationToken(member.Id, member.ChapterId, accessToken, refreshToken);
        }

        private async Task<string> GenerateRefreshToken(Guid memberId, DateTime? refreshTokenExpires = null)
        {
            string refreshToken = GenerateRandomString(32);

            if (refreshTokenExpires == null)
            {
                refreshTokenExpires = DateTime.UtcNow.AddDays(_settings.RefreshTokenLifetimeDays);
            }

            await _memberRepository.AddRefreshToken(memberId, refreshToken, refreshTokenExpires.Value);

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
