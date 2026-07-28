using System;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using ODK.Core.Members;
using ODK.Services.Tests.Helpers;

namespace ODK.Services.Tests.Members;

[Parallelizable]
public static class MemberSubscriptionRecordRepositoryTests
{
    [Test]
    public static async Task HasActiveRecurringSubscription_LatestRecordNotRecurring_ReturnsFalse()
    {
        // Arrange - an older recurring record, but the latest (most recent) is a one-off.
        using var context = new MockOdkContext();
        var member = context.CreateMember();
        var chapter = context.CreateChapter();

        var recurring = context.CreateChapterSubscription(chapter);
        recurring.Recurring = true;
        var oneOff = context.CreateChapterSubscription(chapter);

        CreateRecord(context, member.Id, chapter.Id, recurring.Id, DateTime.UtcNow.AddDays(-30), cancelledUtc: null);
        CreateRecord(context, member.Id, chapter.Id, oneOff.Id, DateTime.UtcNow, cancelledUtc: null);

        var unitOfWork = MockUnitOfWork.Create(context);

        // Act
        var result = await unitOfWork.MemberSubscriptionRecordRepository
            .HasActiveRecurringSubscription(member.Id, chapter.Id).Run();

        // Assert
        result.Should().BeFalse();
    }

    [Test]
    public static async Task HasActiveRecurringSubscription_NoRecords_ReturnsFalse()
    {
        // Arrange
        using var context = new MockOdkContext();
        var member = context.CreateMember();
        var chapter = context.CreateChapter();
        var unitOfWork = MockUnitOfWork.Create(context);

        // Act
        var result = await unitOfWork.MemberSubscriptionRecordRepository
            .HasActiveRecurringSubscription(member.Id, chapter.Id).Run();

        // Assert
        result.Should().BeFalse();
    }

    [Test]
    public static async Task HasActiveRecurringSubscription_NonRecurring_ReturnsFalse()
    {
        // Arrange
        using var context = new MockOdkContext();
        var member = context.CreateMember();
        var chapter = context.CreateChapter();
        var subscription = context.CreateChapterSubscription(chapter);
        CreateRecord(context, member.Id, chapter.Id, subscription.Id, DateTime.UtcNow, cancelledUtc: null);
        var unitOfWork = MockUnitOfWork.Create(context);

        // Act
        var result = await unitOfWork.MemberSubscriptionRecordRepository
            .HasActiveRecurringSubscription(member.Id, chapter.Id).Run();

        // Assert
        result.Should().BeFalse();
    }

    [Test]
    public static async Task HasActiveRecurringSubscription_RecurringCancelled_ReturnsFalse()
    {
        // Arrange
        using var context = new MockOdkContext();
        var member = context.CreateMember();
        var chapter = context.CreateChapter();
        var subscription = context.CreateChapterSubscription(chapter);
        subscription.Recurring = true;
        CreateRecord(context, member.Id, chapter.Id, subscription.Id, DateTime.UtcNow, cancelledUtc: DateTime.UtcNow);
        var unitOfWork = MockUnitOfWork.Create(context);

        // Act
        var result = await unitOfWork.MemberSubscriptionRecordRepository
            .HasActiveRecurringSubscription(member.Id, chapter.Id).Run();

        // Assert
        result.Should().BeFalse();
    }

    [Test]
    public static async Task HasActiveRecurringSubscription_RecurringNotCancelled_ReturnsTrue()
    {
        // Arrange
        using var context = new MockOdkContext();
        var member = context.CreateMember();
        var chapter = context.CreateChapter();
        var subscription = context.CreateChapterSubscription(chapter);
        subscription.Recurring = true;
        CreateRecord(context, member.Id, chapter.Id, subscription.Id, DateTime.UtcNow, cancelledUtc: null);
        var unitOfWork = MockUnitOfWork.Create(context);

        // Act
        var result = await unitOfWork.MemberSubscriptionRecordRepository
            .HasActiveRecurringSubscription(member.Id, chapter.Id).Run();

        // Assert
        result.Should().BeTrue();
    }

    private static MemberSubscriptionRecord CreateRecord(
        MockOdkContext context,
        Guid memberId,
        Guid chapterId,
        Guid chapterSubscriptionId,
        DateTime purchasedUtc,
        DateTime? cancelledUtc)
        => context.Create(new MemberSubscriptionRecord
        {
            CancelledUtc = cancelledUtc,
            ChapterId = chapterId,
            ChapterSubscriptionId = chapterSubscriptionId,
            Id = Guid.NewGuid(),
            MemberId = memberId,
            PurchasedUtc = purchasedUtc
        });
}
