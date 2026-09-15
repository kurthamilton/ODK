using FluentAssertions;
using NUnit.Framework;
using ODK.Services.Chapters.ViewModels;

namespace ODK.Services.Tests.Chapters;

[Parallelizable]
public static class ChapterMigrationShareViewModelTests
{
    [Test]
    public static void Create_NamesTheGroupAndTheAddressInEveryBlock()
    {
        /* Arrange - the whole point of the wording is that an organiser can paste it without editing, so
           every block has to carry the two things they would otherwise have to fill in. */
        var result = Create(previousPlatformName: null);

        // Act / Assert
        foreach (var text in new[]
        {
            result.Announcement, result.Email, result.OldGroupDescription, result.ShortMessage
        })
        {
            text.Should().Contain("Bristol Knitters");
        }

        // The email sends people to the group itself, because they are being asked to join rather than to
        // read about the move.
        result.Email.Should().Contain("https://example.com/groups/bristol-knitters");

        result.Announcement.Should().Contain("https://example.com/groups/bristol-knitters/moved");
        result.OldGroupDescription.Should().Contain("https://example.com/groups/bristol-knitters/moved");
        result.ShortMessage.Should().Contain("https://example.com/groups/bristol-knitters/moved");
    }

    [Test]
    public static void Create_WithAPreviousPlatform_NamesIt()
    {
        // Arrange
        var result = Create("Meetup");

        // Act / Assert
        result.Email.Should().Contain("from Meetup");
        result.ShortMessage.Should().Contain("from Meetup");
    }

    [Test]
    public static void Create_WithoutAPreviousPlatform_ReadsGenerically()
    {
        /* Arrange - most groups never say where they came from, and the copy has to read as though the
           question was never asked rather than leaving a gap where the answer would be. */
        var result = Create(previousPlatformName: null);

        // Act / Assert
        result.Email.Should().NotContain(" from .");
        result.Email.Should().Contain("Bristol Knitters has moved.");
        result.ShortMessage.Should().Contain("Bristol Knitters has moved -");
    }

    [Test]
    public static void Create_WhitespacePreviousPlatform_IsTreatedAsUnset()
    {
        // Arrange - a name the organiser cleared leaves an empty string rather than a null.
        var result = Create(previousPlatformName: "   ");

        // Act / Assert
        result.ShortMessage.Should().Contain("Bristol Knitters has moved -");
    }

    [Test]
    public static void Create_OnlyTheEmailMentionsTheInvitation()
    {
        /* Arrange - the people who receive the email are the ones an import has invited. Anyone reading
           the old group's description is not, and telling them to look for an invitation that does not
           exist sends them looking for nothing. */
        var result = Create(previousPlatformName: null);

        // Act / Assert
        result.Email.Should().Contain("invitation");
        result.Announcement.Should().NotContain("invitation");
        result.OldGroupDescription.Should().NotContain("invitation");
        result.ShortMessage.Should().NotContain("invitation");
    }

    private static ChapterMigrationShareViewModel Create(string? previousPlatformName)
        => ChapterMigrationShareViewModel.Create(
            "Bristol Knitters",
            "https://example.com/groups/bristol-knitters",
            "https://example.com/groups/bristol-knitters/moved",
            previousPlatformName);
}
