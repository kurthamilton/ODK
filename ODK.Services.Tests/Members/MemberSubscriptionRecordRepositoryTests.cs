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
    public static async Task ToChapterSubscription_CurrentRecordNonRecurring_IsNotActiveRecurring()
    {
        // Arrange
        using var context = new MockOdkContext();
        var member = context.CreateMember();
        var chapter = context.CreateChapter();
        var subscription = context.CreateChapterSubscription(chapter);
        CreateCurrentRecord(context, member.Id, chapter.Id, subscription.Id, cancelledUtc: null);

        // Act
        var result = await GetCurrentChapterSubscription(context, member.Id, chapter.Id);

        // Assert
        result.Should().NotBeNull();
        result.IsActiveRecurring().Should().BeFalse();
    }

    [Test]
    public static async Task ToChapterSubscription_CurrentRecordNotRecurringOlderRecurring_IsNotActiveRecurring()
    {
        // Arrange - an older recurring record, but the current one is a one-off.
        using var context = new MockOdkContext();
        var member = context.CreateMember();
        var chapter = context.CreateChapter();

        var recurring = context.CreateChapterSubscription(chapter);
        recurring.Recurring = true;
        var oneOff = context.CreateChapterSubscription(chapter);

        CreateRecord(context, member.Id, chapter.Id, recurring.Id, DateTime.UtcNow.AddDays(-30), isCurrent: false, cancelledUtc: null);
        CreateRecord(context, member.Id, chapter.Id, oneOff.Id, DateTime.UtcNow, isCurrent: true, cancelledUtc: null);

        // Act
        var result = await GetCurrentChapterSubscription(context, member.Id, chapter.Id);

        // Assert
        result.Should().NotBeNull();
        result.IsActiveRecurring().Should().BeFalse();
    }

    [Test]
    public static async Task ToChapterSubscription_CurrentRecordRecurringCancelled_IsNotActiveRecurring()
    {
        // Arrange
        using var context = new MockOdkContext();
        var member = context.CreateMember();
        var chapter = context.CreateChapter();
        var subscription = context.CreateChapterSubscription(chapter);
        subscription.Recurring = true;
        CreateCurrentRecord(context, member.Id, chapter.Id, subscription.Id, cancelledUtc: DateTime.UtcNow);

        // Act
        var result = await GetCurrentChapterSubscription(context, member.Id, chapter.Id);

        // Assert
        result.Should().NotBeNull();
        result.IsActiveRecurring().Should().BeFalse();
    }

    [Test]
    public static async Task ToChapterSubscription_CurrentRecordRecurringNotCancelled_IsActiveRecurring()
    {
        // Arrange
        using var context = new MockOdkContext();
        var member = context.CreateMember();
        var chapter = context.CreateChapter();
        var subscription = context.CreateChapterSubscription(chapter);
        subscription.Recurring = true;
        CreateCurrentRecord(context, member.Id, chapter.Id, subscription.Id, cancelledUtc: null);

        // Act
        var result = await GetCurrentChapterSubscription(context, member.Id, chapter.Id);

        // Assert
        result.Should().NotBeNull();
        result.IsActiveRecurring().Should().BeTrue();
    }

    [Test]
    public static async Task ToChapterSubscription_NoCurrentRecord_ReturnsNull()
    {
        // Arrange
        using var context = new MockOdkContext();
        var member = context.CreateMember();
        var chapter = context.CreateChapter();

        // Act
        var result = await GetCurrentChapterSubscription(context, member.Id, chapter.Id);

        // Assert
        result.Should().BeNull();
    }

    private static MemberSubscriptionRecord CreateCurrentRecord(
        MockOdkContext context,
        Guid memberId,
        Guid chapterId,
        Guid chapterSubscriptionId,
        DateTime? cancelledUtc)
        => CreateRecord(context, memberId, chapterId, chapterSubscriptionId, DateTime.UtcNow, isCurrent: true, cancelledUtc);

    private static MemberSubscriptionRecord CreateRecord(
        MockOdkContext context,
        Guid memberId,
        Guid chapterId,
        Guid chapterSubscriptionId,
        DateTime purchasedUtc,
        bool isCurrent,
        DateTime? cancelledUtc)
        => context.Create(new MemberSubscriptionRecord
        {
            CancelledUtc = cancelledUtc,
            ChapterId = chapterId,
            ChapterSubscriptionId = chapterSubscriptionId,
            Id = Guid.NewGuid(),
            IsCurrent = isCurrent,
            MemberId = memberId,
            PurchasedUtc = purchasedUtc
        });

    private static async Task<MemberChapterSubscription?> GetCurrentChapterSubscription(
        MockOdkContext context, Guid memberId, Guid chapterId)
    {
        var unitOfWork = MockUnitOfWorkFactory.Create(context);
        return await unitOfWork.MemberSubscriptionRecordRepository
            .Query()
            .Current()
            .ForMember(memberId)
            .ForChapter(chapterId)
            .ToChapterSubscription()
            .GetSingleOrDefault()
            .Run();
    }
}
