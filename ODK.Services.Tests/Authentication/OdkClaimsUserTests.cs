using System;
using System.Linq;
using System.Security.Claims;
using FluentAssertions;
using NUnit.Framework;
using ODK.Core.Members;
using ODK.Services.Authentication;

namespace ODK.Services.Tests.Authentication;

[Parallelizable]
public static class OdkClaimsUserTests
{
    [Test]
    public static void GetClaims_SeveralSignedInMembers_WritesThemInSignInOrder()
    {
        // Arrange
        var first = Guid.NewGuid();
        var second = CreateMember();

        // Act
        var claims = new OdkClaimsUser(second, [first]).GetClaims().ToArray();

        // Assert
        claims.Should().ContainSingle(x => x.Type == OdkClaimTypes.SignedInMemberIds)
            .Which.Value.Should().Be($"{first},{second.Id}");
    }

    [Test]
    public static void GetClaims_SingleSignedInMember_OmitsSignedInMemberIdsClaim()
    {
        // Arrange
        var member = CreateMember();

        // Act
        var claims = new OdkClaimsUser(member, []).GetClaims().ToArray();

        // Assert
        claims.Should().NotContain(x => x.Type == OdkClaimTypes.SignedInMemberIds);
    }

    [Test]
    public static void GetClaims_SiteAdmin_SetsRole()
    {
        // Arrange
        var member = CreateMember(siteAdmin: true);

        // Act
        var claims = new OdkClaimsUser(member, []).GetClaims().ToArray();

        // Assert
        claims.Should().ContainSingle(x => x.Type == ClaimTypes.Role)
            .Which.Value.Should().Be(OdkRoles.SiteAdmin);
    }

    [Test]
    public static void GetClaims_SwitchedToMemberWithoutSiteAdmin_UnsetsRole()
    {
        // Arrange
        var siteAdmin = CreateMember(siteAdmin: true);
        var member = CreateMember();
        var claims = new OdkClaimsUser(siteAdmin, []).GetClaims().ToArray();

        // Act
        var switched = new OdkClaimsUser(member, new OdkClaimsUser(claims).SignedInMemberIds)
            .GetClaims()
            .ToArray();

        // Assert
        switched.Should().NotContain(x => x.Type == ClaimTypes.Role);
    }

    [Test]
    public static void SignedInMemberIds_AddedMember_IsAppended()
    {
        // Arrange
        var existing = Guid.NewGuid();
        var added = CreateMember();

        // Act
        var claimsUser = new OdkClaimsUser(added, [existing]);

        // Assert
        claimsUser.SignedInMemberIds.Should().Equal(existing, added.Id);
    }

    [Test]
    public static void SignedInMemberIds_ClaimsWithoutTheClaim_ReturnsCurrentMemberAlone()
    {
        // Arrange
        var memberId = Guid.NewGuid();
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, memberId.ToString()) };

        // Act
        var claimsUser = new OdkClaimsUser(claims);

        // Assert
        claimsUser.SignedInMemberIds.Should().Equal(memberId);
    }

    [Test]
    public static void SignedInMemberIds_RoundTrippedThroughClaims_KeepsSignInOrder()
    {
        // Arrange
        var first = Guid.NewGuid();
        var second = CreateMember();

        // Act
        var claims = new OdkClaimsUser(second, [first]).GetClaims().ToArray();
        var claimsUser = new OdkClaimsUser(claims);

        // Assert
        claimsUser.MemberId.Should().Be(second.Id);
        claimsUser.SignedInMemberIds.Should().Equal(first, second.Id);
    }

    private static Member CreateMember(bool siteAdmin = false)
        => new Member
        {
            Activated = true,
            Id = Guid.NewGuid(),
            SiteAdmin = siteAdmin,
            TimeZone = TimeZoneInfo.Utc
        };
}
