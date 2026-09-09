using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Core.Platforms;
using ODK.Core.Subscriptions;
using ODK.Services.Emails;
using ODK.Services.Emails.Validation;
using ODK.Services.Members;
using ODK.Services.Members.Models;
using ODK.Services.Security;
using ODK.Services.Tests.Helpers;

namespace ODK.Services.Tests.Members;

[Parallelizable]
public static class MemberImportServiceTests
{
    private const int RetentionDays = 90;

    [Test]
    public static async Task ClearStagedMembers_RemovesEveryRow()
    {
        // Arrange
        using var context = CreateMockOdkContext();
        var (chapter, currentMember) = CreateGroup(context);
        Stage(context, chapter, "one@example.com");
        Stage(context, chapter, "two@example.com");

        var service = CreateService(context);

        // Act
        var result = await service.ClearStagedMembers(CreateRequest(chapter, currentMember));

        // Assert
        result.Success.Should().BeTrue();
        context.Set<MemberChapterImport>().Should().BeEmpty();
    }

    [Test]
    public static async Task DeleteStagedMember_RowBelongsToAnotherGroup_LeavesItAlone()
    {
        /* Arrange - the id comes from a form, so a row from a group the admin also administers must not be
           reachable through this group's page. */
        using var context = CreateMockOdkContext();
        var (chapter, currentMember) = CreateGroup(context);
        var (otherChapter, _) = CreateGroup(context);
        var other = Stage(context, otherChapter, "other@example.com");

        var service = CreateService(context);

        // Act
        var result = await service.DeleteStagedMember(CreateRequest(chapter, currentMember), other.Id);

        // Assert
        result.Success.Should().BeFalse();
        context.Set<MemberChapterImport>().Should().ContainSingle();
    }

    [Test]
    public static async Task DeleteStagedMember_RemovesThatRowOnly()
    {
        // Arrange
        using var context = CreateMockOdkContext();
        var (chapter, currentMember) = CreateGroup(context);
        var removed = Stage(context, chapter, "one@example.com");
        Stage(context, chapter, "two@example.com");

        var service = CreateService(context);

        // Act
        var result = await service.DeleteStagedMember(CreateRequest(chapter, currentMember), removed.Id);

        // Assert
        result.Success.Should().BeTrue();
        context.Set<MemberChapterImport>()
            .Should().ContainSingle()
            .Which.EmailAddress.Should().Be("two@example.com");
    }

    [Test]
    public static async Task GetMemberImportViewModel_MalformedEmailAddress_ReportsTheRowAsInvalid()
    {
        // Arrange
        using var context = CreateMockOdkContext();
        var (chapter, currentMember) = CreateGroup(context);
        Stage(context, chapter, "good@example.com");
        Stage(context, chapter, "not an email");

        var service = CreateService(context);

        // Act
        var result = await service.GetMemberImportViewModel(CreateRequest(chapter, currentMember));

        // Assert
        result.Rows.Single(x => x.EmailAddress == "not an email")
            .Status.Should().Be(MemberImportRowStatus.Invalid);
        result.Rows.Single(x => x.EmailAddress == "good@example.com")
            .Status.Should().Be(MemberImportRowStatus.New);

        // Only the usable one asks for a place - an address nothing can be sent to takes none.
        result.PlacesRequired.Should().Be(1);
    }

    [Test]
    public static async Task GetMemberImportViewModel_MemberJoinedSinceTheUpload_ReportsThemAsAlreadyInGroup()
    {
        /* Arrange - the status is derived on load rather than stored, so a row is re-read against the group
           as it is now. This is the case a stored status would get wrong. */
        using var context = CreateMockOdkContext();
        var (chapter, currentMember) = CreateGroup(context);

        var joined = context.CreateMember();
        joined.EmailAddress = "joined@example.com";
        JoinGroup(context, joined, chapter);

        Stage(context, chapter, "joined@example.com");

        var service = CreateService(context);

        // Act
        var result = await service.GetMemberImportViewModel(CreateRequest(chapter, currentMember));

        // Assert
        result.Rows.Should().ContainSingle()
            .Which.Status.Should().Be(MemberImportRowStatus.ExistingInGroup);
        result.PlacesRequired.Should().Be(0);
    }

    [Test]
    public static async Task GetMemberImportViewModel_MoreRowsThanRemainingCapacity_ReportsItDoesNotFit()
    {
        // Arrange
        using var context = CreateMockOdkContext();
        var (chapter, currentMember) = CreateGroup(context, memberLimit: 1);
        Stage(context, chapter, "one@example.com");
        Stage(context, chapter, "two@example.com");

        var service = CreateService(context);

        // Act
        var result = await service.GetMemberImportViewModel(CreateRequest(chapter, currentMember));

        // Assert
        result.PlacesRequired.Should().Be(2);
        result.FitsWithinCapacity.Should().BeFalse();
    }

    [Test]
    public static async Task GetMemberImportViewModel_ReportsWhenEachRowIsDeleted()
    {
        // Arrange
        using var context = CreateMockOdkContext();
        var (chapter, currentMember) = CreateGroup(context);
        Stage(context, chapter, "old@example.com", createdUtc: DateTime.UtcNow.AddDays(-RetentionDays + 10));

        var service = CreateService(context);

        // Act
        var result = await service.GetMemberImportViewModel(CreateRequest(chapter, currentMember));

        // Assert - measured from when the address arrived, so a row uploaded 80 days ago has 10 days left.
        result.Rows.Should().ContainSingle()
            .Which.DaysRemaining.Should().Be(10);
    }

    [Test]
    public static async Task PurgeExpiredImports_RowPastTheRetention_DeletesIt()
    {
        // Arrange
        using var context = CreateMockOdkContext();
        var (chapter, _) = CreateGroup(context);
        Stage(context, chapter, "old@example.com", createdUtc: DateTime.UtcNow.AddDays(-RetentionDays - 1));
        Stage(context, chapter, "new@example.com");

        var service = CreateService(context);

        // Act
        var purged = await service.PurgeExpiredImports();

        // Assert
        purged.Should().Be(1);
        context.Set<MemberChapterImport>()
            .Should().ContainSingle()
            .Which.EmailAddress.Should().Be("new@example.com");
    }

    [Test]
    public static async Task PurgeExpiredImports_RowReUploadedSinceItArrived_StillDeletesIt()
    {
        /* Arrange - the clock runs from when the address arrived, not from the last upload that carried it,
           so re-uploading the same file every month cannot hold an address indefinitely. */
        using var context = CreateMockOdkContext();
        var (chapter, _) = CreateGroup(context);
        var staged = Stage(context, chapter, "old@example.com",
            createdUtc: DateTime.UtcNow.AddDays(-RetentionDays - 1));
        staged.UploadedUtc = DateTime.UtcNow;
        context.SaveChanges();

        var service = CreateService(context);

        // Act
        var purged = await service.PurgeExpiredImports();

        // Assert
        purged.Should().Be(1);
        context.Set<MemberChapterImport>().Should().BeEmpty();
    }

    [Test]
    public static async Task StageMembers_AddressAlreadyHeld_UpdatesItWithoutMovingItsClock()
    {
        // Arrange
        using var context = CreateMockOdkContext();
        var (chapter, currentMember) = CreateGroup(context);
        var createdUtc = DateTime.UtcNow.AddDays(-5);
        Stage(context, chapter, "held@example.com", firstName: "Old", createdUtc: createdUtc);

        var service = CreateService(context);

        // Act
        var result = await service.StageMembers(
            CreateRequest(chapter, currentMember),
            [Row("held@example.com", "New", "Name")],
            "members.csv");

        // Assert
        result.Success.Should().BeTrue();
        result.Staged.Should().Be(0);
        result.Updated.Should().Be(1);

        var staged = context.Set<MemberChapterImport>().Should().ContainSingle().Subject;
        staged.FirstName.Should().Be("New");
        staged.SourceFileName.Should().Be("members.csv");
        staged.CreatedUtc.Should().BeCloseTo(createdUtc, TimeSpan.FromSeconds(1));
        staged.UploadedUtc.Should().BeAfter(createdUtc);
    }

    [Test]
    public static async Task StageMembers_AlreadyInvited_CountsThemWithoutHoldingARow()
    {
        /* Arrange - the outstanding invite is the ask, so there is nothing left to do about the address. A
           row would show as needing no action and never clear. */
        using var context = CreateMockOdkContext();
        var (chapter, currentMember) = CreateGroup(context);

        var invited = context.CreateMember();
        invited.EmailAddress = "invited@example.com";
        context.Create(new MemberChapterInvite
        {
            ChapterId = chapter.Id,
            CreatedUtc = DateTime.UtcNow,
            Id = Guid.NewGuid(),
            MemberId = invited.Id
        });

        var service = CreateService(context);

        // Act
        var result = await service.StageMembers(
            CreateRequest(chapter, currentMember),
            [Row("invited@example.com", "Invited", "Member")],
            "members.csv");

        // Assert
        result.Success.Should().BeTrue();
        result.AlreadyInvited.Should().Be(1);
        result.Staged.Should().Be(0);
        context.Set<MemberChapterImport>().Should().BeEmpty();
    }

    [Test]
    public static async Task StageMembers_ExistingGroupMember_CountsThemWithoutHoldingARow()
    {
        // Arrange
        using var context = CreateMockOdkContext();
        var (chapter, currentMember) = CreateGroup(context);

        var member = context.CreateMember();
        member.EmailAddress = "member@example.com";
        JoinGroup(context, member, chapter);

        var service = CreateService(context);

        // Act
        var result = await service.StageMembers(
            CreateRequest(chapter, currentMember),
            [Row("member@example.com", "Existing", "Member")],
            "members.csv");

        // Assert
        result.Success.Should().BeTrue();
        result.AlreadyInGroup.Should().Be(1);
        result.Staged.Should().Be(0);
        context.Set<MemberChapterImport>().Should().BeEmpty();
    }

    [Test]
    public static async Task StageMembers_FileHasDuplicateEmails_HoldsOneRow()
    {
        /* Arrange - a group holds one row per address, so the same address twice (differing only in case)
           collapses. The unique index would reject the second write. */
        using var context = CreateMockOdkContext();
        var (chapter, currentMember) = CreateGroup(context);

        var service = CreateService(context);

        // Act
        var result = await service.StageMembers(
            CreateRequest(chapter, currentMember),
            [Row("new@example.com", "New", "Member"), Row("NEW@example.com", "Dupe", "Member")],
            "members.csv");

        // Assert
        result.Success.Should().BeTrue();
        result.Staged.Should().Be(1);
        context.Set<MemberChapterImport>().Should().ContainSingle();
    }

    [Test]
    public static async Task StageMembers_MalformedEmailAddress_HoldsItAsAnIssue()
    {
        /* Arrange - correcting the address is an action, so the row is kept: deleting it would take the only
           record of the problem with it. */
        using var context = CreateMockOdkContext();
        var (chapter, currentMember) = CreateGroup(context);

        var service = CreateService(context);

        // Act
        var result = await service.StageMembers(
            CreateRequest(chapter, currentMember),
            [Row("not an email", "Bad", "Member")],
            "members.csv");

        // Assert
        result.Success.Should().BeTrue();
        result.Invalid.Should().Be(1);
        result.Staged.Should().Be(1);
        context.Set<MemberChapterImport>().Should().ContainSingle();
    }

    [Test]
    public static async Task StageMembers_NoRowsWithAnEmailAddress_Fails()
    {
        // Arrange
        using var context = CreateMockOdkContext();
        var (chapter, currentMember) = CreateGroup(context);

        var service = CreateService(context);

        // Act
        var result = await service.StageMembers(
            CreateRequest(chapter, currentMember),
            [Row(" ", "No", "Address")],
            "members.csv");

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("email address");
        context.Set<MemberChapterImport>().Should().BeEmpty();
    }

    /* Staging takes no account of the group's remaining places: a file that does not fit is held so the
       admin can remove people from it, and the invite is where the limit bites. */
    [Test]
    public static async Task StageMembers_MoreRowsThanRemainingCapacity_HoldsThemAnyway()
    {
        // Arrange
        using var context = CreateMockOdkContext();
        var (chapter, currentMember) = CreateGroup(context, memberLimit: 1);

        var service = CreateService(context);

        // Act
        var result = await service.StageMembers(
            CreateRequest(chapter, currentMember),
            [Row("one@example.com", "One", "Member"), Row("two@example.com", "Two", "Member")],
            "members.csv");

        // Assert
        result.Success.Should().BeTrue();
        result.Staged.Should().Be(2);
        context.Set<MemberChapterImport>().Should().HaveCount(2);
    }

    [Test]
    public static async Task StageMembers_NewAddresses_HoldsThemWithoutRaisingAnAccount()
    {
        /* Arrange - a held row is data. Nothing is created for somebody who might never be invited, which
           is what stops a bad address leaving an account behind. */
        using var context = CreateMockOdkContext();
        var (chapter, currentMember) = CreateGroup(context);

        var service = CreateService(context);
        var membersBefore = context.Set<Member>().Count();

        // Act
        var result = await service.StageMembers(
            CreateRequest(chapter, currentMember),
            [Row("new@example.com", "New", "Member")],
            "members.csv");

        // Assert
        result.Success.Should().BeTrue();
        result.Staged.Should().Be(1);

        context.Set<Member>().Count().Should().Be(membersBefore);
        context.Set<MemberChapterInvite>().Should().BeEmpty();

        var staged = context.Set<MemberChapterImport>().Should().ContainSingle().Subject;
        staged.ChapterId.Should().Be(chapter.Id);
        staged.EmailAddress.Should().Be("new@example.com");
    }

    private static MemberImportService CreateService(MockOdkContext context)
        => new MemberImportService(
            MockUnitOfWorkFactory.Create(context),
            new EmailValidationService(new InconclusiveEmailVerifier()),
            new SiteSubscriptionCooldown(months: 0),
            new MemberImportServiceSettings { RetentionDays = RetentionDays });

    private static (Chapter Chapter, Member CurrentMember) CreateGroup(
        MockOdkContext context, int? memberLimit = null)
    {
        var currentMember = context.CreateMember();
        var chapter = context.CreateChapter(
            owner: currentMember,
            siteSubscription: context.CreateSiteSubscription(memberLimit: memberLimit));

        return (chapter, currentMember);
    }

    private static MockOdkContext CreateMockOdkContext() => new MockOdkContext();

    private static IMemberChapterAdminServiceRequest CreateRequest(Chapter chapter, Member currentMember)
    {
        var mock = new Mock<IMemberChapterAdminServiceRequest>();

        mock.Setup(x => x.Chapter).Returns(chapter);
        mock.Setup(x => x.CurrentMember).Returns(currentMember);
        mock.Setup(x => x.CurrentMemberOrDefault).Returns(currentMember);
        mock.Setup(x => x.Platform).Returns(PlatformType.Default);
        mock.Setup(x => x.Securable).Returns(ChapterAdminSecurable.MemberImport);

        return mock.Object;
    }

    private static void JoinGroup(MockOdkContext context, Member member, Chapter chapter)
    {
        context.Create(new MemberChapter
        {
            Approved = true,
            ChapterId = chapter.Id,
            CreatedUtc = DateTime.UtcNow,
            Id = Guid.NewGuid(),
            MemberId = member.Id
        });

        context.SaveChanges();
    }

    private static MemberImportCsvRow Row(string emailAddress, string firstName, string lastName)
        => new MemberImportCsvRow
        {
            EmailAddress = emailAddress,
            FirstName = firstName,
            LastName = lastName
        };

    private static MemberChapterImport Stage(
        MockOdkContext context,
        Chapter chapter,
        string emailAddress,
        string firstName = "Staged",
        DateTime? createdUtc = null)
    {
        var staged = context.Create(new MemberChapterImport
        {
            ChapterId = chapter.Id,
            CreatedUtc = createdUtc ?? DateTime.UtcNow,
            EmailAddress = emailAddress,
            FirstName = firstName,
            LastName = "Member",
            UploadedUtc = createdUtc ?? DateTime.UtcNow
        });

        context.SaveChanges();

        return staged;
    }
}
