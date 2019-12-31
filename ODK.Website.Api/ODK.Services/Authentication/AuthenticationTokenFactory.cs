using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using ODK.Core.Members;

namespace ODK.Services.Authentication
{
    public class AuthenticationTokenFactory : IAuthenticationTokenFactory
    {
        private readonly AuthenticationTokenFactorySettings _settings;

        public AuthenticationTokenFactory(AuthenticationTokenFactorySettings settings)
        {
            _settings = settings;
        }

        public string Create(Member member, double lifetimeMinutes)
        {
            byte[] keyBytes = Encoding.ASCII.GetBytes(_settings.Key);
            SymmetricSecurityKey key = new SymmetricSecurityKey(keyBytes);

            JwtSecurityToken token = new JwtSecurityToken
            (
                claims: new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, member.Id.ToString()),
                    new Claim(ClaimTypes.Name, $"{member.FirstName} {member.LastName}"),
                    new Claim(ClaimTypes.Email, member.EmailAddress),
                    new Claim("ChapterId", member.ChapterId.ToString())
                },
                notBefore: DateTime.UtcNow,
                expires: DateTime.UtcNow.AddMinutes(lifetimeMinutes),
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature)
            );

            return new JwtSecurityTokenHandler()
                .WriteToken(token);
        }

        public Guid? DecodeMemberId(string token)
        {
            try
            {
                JwtSecurityToken decoded = new JwtSecurityTokenHandler()
                    .ReadToken(token) as JwtSecurityToken;

                Claim claim = decoded?.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);
                return Guid.TryParse(claim?.Value, out Guid memberId) ? memberId : new Guid?();
            }
            catch
            {
                return null;
            }
        }
    }
}
