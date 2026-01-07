using System.Security.Claims;
using ODK.Core.Chapters;
using ODK.Core.Members;

namespace ODK.Services.Authentication;

public class OdkClaimsUser
{
    private readonly List<string> _roles = [];

    public OdkClaimsUser(
        Member member,
        IEnumerable<ChapterAdminMember> adminMembers)
    {
        MemberId = member.Id;

        _roles.Add(OdkRoles.Member);

        if (adminMembers.Any())
        {
            _roles.Add(OdkRoles.Admin);
        }

        if (member.SuperAdmin)
        {
            _roles.Add(OdkRoles.SuperAdmin);
        }
    }

    public OdkClaimsUser(IEnumerable<Claim> claims)
    {
        var claimDictionary = claims
            .GroupBy(x => x.Type)
            .ToDictionary(x => x.Key, x => x.ToArray(), StringComparer.OrdinalIgnoreCase);

        if (claimDictionary.TryGetValue(ClaimTypes.NameIdentifier, out var memberIdClaims) &&
            Guid.TryParse(memberIdClaims.First().Value, out var memberId))
        {

            MemberId = memberId;
        }

        var roleClaims = claims
            .Where(x => x.Type == ClaimTypes.Role);
        _roles.AddRange(roleClaims.Select(x => x.Value));
    }

    public Guid? MemberId { get; }

    public IReadOnlyCollection<string> Roles => _roles;

    public IEnumerable<Claim> GetClaims()
    {
        if (MemberId != null)
        {
            yield return new Claim(ClaimTypes.NameIdentifier, MemberId.Value.ToString());
        }

        foreach (var role in Roles)
        {
            yield return new Claim(ClaimTypes.Role, role);
        }
    }
}
