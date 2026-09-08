using System.Text;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using NUnit.Framework;
using ODK.Core.Subscriptions;
using ODK.Services;
using ODK.Services.Csv;
using ODK.Services.Integrations.Csv;
using ODK.Services.Members;
using ODK.Services.Members.Models;
using ODK.Web.Razor.Services;

namespace ODK.Web.Razor.Tests.Services;

[Parallelizable]
public static class MemberImportPreviewBuilderTests
{
    private static MemberImportPreview EmptyPreview => new()
    {
        Capacity = new MemberImportCapacity
        {
            MemberCount = 0,
            OutstandingInviteCount = 0,
            OwnerSubscription = new SiteSubscription()
        },
        PlacesRequired = 0,
        Rows = []
    };

    [Test]
    public static async Task Build_NoFile_ReturnsFailure()
    {
        var result = await CreateBuilder().Build(Request(), file: null);

        result.Success.Should().BeFalse();
        result.Message.Should().Be("No file uploaded");
    }

    [Test]
    public static async Task Build_NoRowsWithEmail_ReturnsFailure()
    {
        var csvReader = new Mock<ICsvReader>();
        csvReader.Setup(x => x.Read<MemberImportModel>(It.IsAny<Stream>()))
            .Returns([new MemberImportModel { EmailAddress = " ", FirstName = "A", LastName = "B" }]);

        var result = await CreateBuilder(csvReader.Object).Build(Request(), File("members.csv", "text/csv", 10));

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("email address");
    }

    [Test]
    public static async Task Build_TooLarge_ReturnsFailure()
    {
        var result = await CreateBuilder().Build(Request(), File("members.csv", "text/csv", 6 * 1024 * 1024));

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("too large");
    }

    [Test]
    public static async Task Build_ValidFile_StagesRowsAndReturnsPreview()
    {
        var members = new[]
        {
            new MemberImportModel { EmailAddress = "a@b.com", FirstName = "A", LastName = "B" }
        };

        var csvReader = new Mock<ICsvReader>();
        csvReader.Setup(x => x.Read<MemberImportModel>(It.IsAny<Stream>())).Returns(members);

        var memberAdminService = new Mock<IMemberAdminService>();
        memberAdminService
            .Setup(x => x.GetMemberImportPreview(
                It.IsAny<IMemberChapterAdminServiceRequest>(),
                It.IsAny<IReadOnlyCollection<MemberImportModel>>()))
            .ReturnsAsync(new MemberImportPreview
            {
                Capacity = new MemberImportCapacity
                {
                    MemberCount = 0,
                    OutstandingInviteCount = 0,
                    OwnerSubscription = new SiteSubscription()
                },
                PlacesRequired = 0,
                Rows = []
            });

        var staging = new Mock<IMemberImportStagingService>();
        staging.Setup(x => x.Stage(It.IsAny<IReadOnlyCollection<MemberImportModel>>())).Returns("tok-123");

        var result = await CreateBuilder(csvReader.Object, memberAdminService.Object, staging.Object)
            .Build(Request(), File("members.csv", "text/csv", 10));

        result.Success.Should().BeTrue();
        result.Value!.Token.Should().Be("tok-123");
        staging.Verify(x => x.Stage(It.IsAny<IReadOnlyCollection<MemberImportModel>>()), Times.Once);
    }

    [Test]
    public static async Task Build_SecondFile_StagesOnlyTheSecondFilesRows()
    {
        /* Arrange - re-uploading from the review step replaces the preview rather than adding to it, so the
           rows behind the second token are the second file's alone. Real reader and real staging, because
           the question is what the pair of them accumulates across two calls. */
        var staging = new MemberImportStagingService(new MemoryCache(new MemoryCacheOptions()));

        var memberAdminService = new Mock<IMemberAdminService>();
        memberAdminService
            .Setup(x => x.GetMemberImportPreview(
                It.IsAny<IMemberChapterAdminServiceRequest>(),
                It.IsAny<IReadOnlyCollection<MemberImportModel>>()))
            .ReturnsAsync(EmptyPreview);

        var builder = CreateBuilder(new CsvReader(), memberAdminService.Object, staging);

        // Act
        var first = await builder.Build(Request(), CsvFile(Csv("A", "One", "a@example.com")));
        var second = await builder.Build(
            Request(), CsvFile(Csv("B", "Two", "b@example.com")), first.Value!.Token);

        // Assert
        first.Success.Should().BeTrue();
        second.Success.Should().BeTrue();
        second.Value!.Token.Should().NotBe(first.Value!.Token);

        staging.Retrieve(second.Value!.Token)
            .Should().ContainSingle()
            .Which.EmailAddress.Should().Be("b@example.com");

        // Replaced, not added to: the file it superseded can no longer be imported by a token that
        // outlived the page it was rendered on.
        staging.Retrieve(first.Value!.Token).Should().BeNull();
    }

    [Test]
    public static async Task Build_SecondFileRejected_LeavesTheFirstStillImportable()
    {
        /* Arrange - a rejected re-upload must not take the preview on screen with it: the admin is left with
           the file they had, and the confirm button behind it still works. */
        var staging = new MemberImportStagingService(new MemoryCache(new MemoryCacheOptions()));

        var memberAdminService = new Mock<IMemberAdminService>();
        memberAdminService
            .Setup(x => x.GetMemberImportPreview(
                It.IsAny<IMemberChapterAdminServiceRequest>(),
                It.IsAny<IReadOnlyCollection<MemberImportModel>>()))
            .ReturnsAsync(EmptyPreview);

        var builder = CreateBuilder(new CsvReader(), memberAdminService.Object, staging);

        var first = await builder.Build(Request(), CsvFile(Csv("A", "One", "a@example.com")));

        // Act
        var second = await builder.Build(
            Request(), File("members.txt", "text/csv", 10), first.Value!.Token);

        // Assert
        second.Success.Should().BeFalse();

        staging.Retrieve(first.Value!.Token)
            .Should().ContainSingle()
            .Which.EmailAddress.Should().Be("a@example.com");
    }

    [Test]
    public static async Task Build_WrongContentType_ReturnsFailure()
    {
        var result = await CreateBuilder().Build(Request(), File("members.csv", "application/zip", 10));

        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Invalid content type");
    }

    [Test]
    public static async Task Build_WrongExtension_ReturnsFailure()
    {
        var result = await CreateBuilder().Build(Request(), File("members.txt", "text/csv", 10));

        result.Success.Should().BeFalse();
        result.Message.Should().Contain(".csv");
    }

    private static MemberImportPreviewBuilder CreateBuilder(
        ICsvReader? csvReader = null,
        IMemberAdminService? memberAdminService = null,
        IMemberImportStagingService? staging = null)
        => new MemberImportPreviewBuilder(
            csvReader ?? Mock.Of<ICsvReader>(),
            memberAdminService ?? Mock.Of<IMemberAdminService>(),
            staging ?? Mock.Of<IMemberImportStagingService>());

    private static string Csv(string firstName, string lastName, string emailAddress)
        => string.Join(
            Environment.NewLine,
            "FirstName,LastName,EmailAddress",
            $"{firstName},{lastName},{emailAddress}");

    private static IFormFile CsvFile(string content)
    {
        var bytes = Encoding.UTF8.GetBytes(content);
        var file = new Mock<IFormFile>();
        file.SetupGet(x => x.FileName).Returns("members.csv");
        file.SetupGet(x => x.ContentType).Returns("text/csv");
        file.SetupGet(x => x.Length).Returns(bytes.Length);
        file.Setup(x => x.OpenReadStream()).Returns(() => new MemoryStream(bytes));
        return file.Object;
    }

    private static IFormFile File(string fileName, string contentType, long length)
    {
        var file = new Mock<IFormFile>();
        file.SetupGet(x => x.FileName).Returns(fileName);
        file.SetupGet(x => x.ContentType).Returns(contentType);
        file.SetupGet(x => x.Length).Returns(length);
        file.Setup(x => x.OpenReadStream()).Returns(() => new MemoryStream(Encoding.UTF8.GetBytes("data")));
        return file.Object;
    }

    private static IMemberChapterAdminServiceRequest Request() => Mock.Of<IMemberChapterAdminServiceRequest>();
}
