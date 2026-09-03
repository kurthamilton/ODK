using System.Security.Claims;
using ODK.Core.Members;

namespace ODK.Services.Authentication;

public class OdkClaimsUser
{
    private const char SignedInMemberIdSeparator = ',';

    private readonly List<string> _roles = [];
    private readonly List<Guid> _signedInMemberIds = [];

    public OdkClaimsUser(Member member, IEnumerable<Guid> signedInMemberIds)
    {
        MemberId = member.Id;

        if (member.SiteAdmin)
        {
            _roles.Add(OdkRoles.SiteAdmin);
        }

        _signedInMemberIds.AddRange(signedInMemberIds.Distinct());
        AddCurrentMemberId();
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

        if (claimDictionary.TryGetValue(OdkClaimTypes.SignedInMemberIds, out var signedInMemberIdClaims))
        {
            _signedInMemberIds.AddRange(ParseSignedInMemberIds(signedInMemberIdClaims.First().Value));
        }

        AddCurrentMemberId();

        var roleClaims = claims
            .Where(x => x.Type == ClaimTypes.Role);
        _roles.AddRange(roleClaims.Select(x => x.Value));
    }

    public Guid? MemberId { get; }

    public IReadOnlyCollection<string> Roles => _roles;

    /// <summary>
    /// Every member signed in on the cookie, oldest sign-in first, including <see cref="MemberId"/>.
    /// </summary>
    public IReadOnlyCollection<Guid> SignedInMemberIds => _signedInMemberIds;

    public IEnumerable<Claim> GetClaims()
    {
        if (MemberId != null)
        {
            yield return new Claim(ClaimTypes.NameIdentifier, MemberId.Value.ToString());
        }

        // A lone member is what NameIdentifier already says, so the claim appears only once there is
        // something to switch between - an ordinary sign-in carries no extra cookie weight.
        if (_signedInMemberIds.Count > 1)
        {
            yield return new Claim(
                OdkClaimTypes.SignedInMemberIds,
                string.Join(SignedInMemberIdSeparator, _signedInMemberIds));
        }

        foreach (var role in Roles)
        {
            yield return new Claim(ClaimTypes.Role, role);
        }
    }

    private static IEnumerable<Guid> ParseSignedInMemberIds(string value) => value
        .Split(SignedInMemberIdSeparator, StringSplitOptions.RemoveEmptyEntries)
        .Select(x => Guid.TryParse(x, out var memberId) ? memberId : default(Guid?))
        .Where(x => x != null)
        .Select(x => x!.Value)
        .Distinct();

    // The member being acted as is always one of the signed-in members, however the list arrived, so a
    // newly added account lands at the end and the order stays sign-in order.
    private void AddCurrentMemberId()
    {
        if (MemberId != null && !_signedInMemberIds.Contains(MemberId.Value))
        {
            _signedInMemberIds.Add(MemberId.Value);
        }
    }
}
