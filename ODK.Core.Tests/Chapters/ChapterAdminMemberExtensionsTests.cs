using FluentAssertions;
using NUnit.Framework;
using ODK.Core.Chapters;
using ODK.Core.Members;

namespace ODK.Core.Tests.Chapters;

[Parallelizable]
public static class ChapterAdminMemberExtensionsTests
{
    [TestCase(ChapterAdminRole.Owner)]
    [TestCase(ChapterAdminRole.Admin)]
    [TestCase(ChapterAdminRole.Organiser)]
    public static void HasAccessTo_SiteAdmin_ReturnsTrue(ChapterAdminRole targetRole)
    {
        // Arrange
        ChapterAdminMember? adminMember = null;
        var currentMember = CreateMember(siteAdmin: true);

        // Act
        var result = adminMember.HasAccessTo(targetRole, currentMember);

        // Assert
        result.Should().BeTrue();
    }

    [TestCase(ChapterAdminRole.Owner)]
    [TestCase(ChapterAdminRole.Admin)]
    [TestCase(ChapterAdminRole.Organiser)]
    public static void HasAccessTo_NoRole_ReturnsFalse(ChapterAdminRole targetRole)
    {
        // Arrange
        ChapterAdminMember? adminMember = null;
        var currentMember = CreateMember(siteAdmin: false);

        // Act
        var result = adminMember.HasAccessTo(targetRole, currentMember);

        // Assert
        result.Should().BeFalse();
    }

    [TestCase(ChapterAdminRole.Owner, ChapterAdminRole.Owner, ExpectedResult = true)]
    [TestCase(ChapterAdminRole.Owner, ChapterAdminRole.Admin, ExpectedResult = true)]
    [TestCase(ChapterAdminRole.Owner, ChapterAdminRole.Organiser, ExpectedResult = true)]
    [TestCase(ChapterAdminRole.Admin, ChapterAdminRole.Owner, ExpectedResult = false)]
    [TestCase(ChapterAdminRole.Admin, ChapterAdminRole.Admin, ExpectedResult = true)]
    [TestCase(ChapterAdminRole.Admin, ChapterAdminRole.Organiser, ExpectedResult = true)]
    [TestCase(ChapterAdminRole.Organiser, ChapterAdminRole.Owner, ExpectedResult = false)]
    [TestCase(ChapterAdminRole.Organiser, ChapterAdminRole.Admin, ExpectedResult = false)]
    [TestCase(ChapterAdminRole.Organiser, ChapterAdminRole.Organiser, ExpectedResult = true)]
    public static bool HasAccessTo_ComparesLevels(ChapterAdminRole currentRole, ChapterAdminRole targetRole)
    {
        // Arrange
        var adminMember = CreateAdminMember(currentRole);
        var currentMember = CreateMember(siteAdmin: false);

        // Act
        var result = adminMember.HasAccessTo(targetRole, currentMember);

        // Assert
        return result;
    }

    private static ChapterAdminMember CreateAdminMember(ChapterAdminRole role) => new()
    {
        Role = role
    };

    private static Member CreateMember(bool siteAdmin = false) => new()
    {
        SiteAdmin = siteAdmin
    };
}