using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using ODK.Core.Chapters;
using ODK.Core.DataTypes;
using ODK.Core.Members;
using ODK.Core.Platforms;
using ODK.Services.Members.Tasks;
using ODK.Services.Members.Tasks.Providers;
using ODK.Services.Tests.Helpers;

namespace ODK.Services.Tests.Members.Tasks;

[Parallelizable]
public static class MemberTaskServiceTests
{
    [Test]
    public static async Task GetOutstandingTasks_MemberMissingProfileAndAvatar_ReturnsBothTasks()
    {
        // Arrange - a member in a chapter with a required property they haven't answered, and no avatar.
        using var context = new MockOdkContext();

        var member = context.CreateMember();
        var chapter = context.CreateChapter(members: [member]);
        context.Create(new ChapterProperty
        {
            Id = Guid.NewGuid(),
            ChapterId = chapter.Id,
            Required = true,
            ApplicationOnly = false,
            DataType = DataType.Text,
            Label = "Bio"
        });

        var service = CreateService(context);
        var request = CreateRequest(member);

        // Act
        var tasks = await service.GetOutstandingTasks(request);

        // Assert
        tasks.Select(x => x.Type).Should().BeEquivalentTo(
            new[] { MemberTaskType.CompleteChapterProfile, MemberTaskType.UploadImage });
        tasks.Single(x => x.Type == MemberTaskType.CompleteChapterProfile).Chapter!.Id
            .Should().Be(chapter.Id);
    }

    [Test]
    public static async Task GetOutstandingTasks_MemberWithCompletedProfileAndAvatar_ReturnsNoTasks()
    {
        // Arrange
        using var context = new MockOdkContext();

        var member = context.CreateMember();
        var chapter = context.CreateChapter(members: [member]);
        var property = context.Create(new ChapterProperty
        {
            Id = Guid.NewGuid(),
            ChapterId = chapter.Id,
            Required = true,
            ApplicationOnly = false,
            DataType = DataType.Text,
            Label = "Bio"
        });
        context.Create(new MemberProperty
        {
            ChapterPropertyId = property.Id,
            MemberId = member.Id,
            Value = "answered"
        });
        context.Create(new MemberAvatar
        {
            MemberId = member.Id,
            ImageData = [1],
            MimeType = "image/png"
        });

        var service = CreateService(context);
        var request = CreateRequest(member);

        // Act
        var tasks = await service.GetOutstandingTasks(request);

        // Assert
        tasks.Should().BeEmpty();
    }

    private static IMemberServiceRequest CreateRequest(Member member) =>
        Mock.Of<IMemberServiceRequest>(x =>
            x.CurrentMember == member && x.Platform == PlatformType.Default);

    private static MemberTaskService CreateService(MockOdkContext context) =>
        new MemberTaskService(
            MockUnitOfWork.Create(context),
            [new CompleteChapterProfileTaskProvider(), new UploadImageTaskProvider()]);
}
