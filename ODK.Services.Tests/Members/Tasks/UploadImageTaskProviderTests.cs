using System;
using FluentAssertions;
using NUnit.Framework;
using ODK.Core.Members;
using ODK.Core.Platforms;
using ODK.Services.Members.Tasks;
using ODK.Services.Members.Tasks.Providers;

namespace ODK.Services.Tests.Members.Tasks;

[Parallelizable]
public static class UploadImageTaskProviderTests
{
    [Test]
    public static void GetTasks_MemberHasAvatar_ReturnsNoTask()
    {
        // Arrange
        var context = CreateContext(hasAvatar: true);

        // Act
        var tasks = new UploadImageTaskProvider().GetTasks(context);

        // Assert
        tasks.Should().BeEmpty();
    }

    [Test]
    public static void GetTasks_MemberHasNoAvatar_ReturnsUploadImageTask()
    {
        // Arrange
        var context = CreateContext(hasAvatar: false);

        // Act
        var tasks = new UploadImageTaskProvider().GetTasks(context);

        // Assert
        tasks.Should().ContainSingle().Which.Type.Should().Be(MemberTaskType.UploadImage);
    }

    private static MemberTaskContext CreateContext(bool hasAvatar) => new MemberTaskContext
    {
        Chapters = [],
        ChapterProperties = [],
        HasAvatar = hasAvatar,
        Member = new Member { Id = Guid.NewGuid() },
        MemberProperties = [],
        Platform = PlatformType.Default
    };
}
