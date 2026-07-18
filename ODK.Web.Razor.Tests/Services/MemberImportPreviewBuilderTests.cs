using System.Text;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using NUnit.Framework;
using ODK.Services;
using ODK.Services.Csv;
using ODK.Services.Members;
using ODK.Services.Members.Models;
using ODK.Web.Razor.Services;

namespace ODK.Web.Razor.Tests.Services;

[Parallelizable]
public static class MemberImportPreviewBuilderTests
{
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
            .ReturnsAsync(new MemberImportPreview { Rows = [] });

        var staging = new Mock<IMemberImportStagingService>();
        staging.Setup(x => x.Stage(It.IsAny<IReadOnlyCollection<MemberImportModel>>())).Returns("tok-123");

        var result = await CreateBuilder(csvReader.Object, memberAdminService.Object, staging.Object)
            .Build(Request(), File("members.csv", "text/csv", 10));

        result.Success.Should().BeTrue();
        result.Value!.Token.Should().Be("tok-123");
        staging.Verify(x => x.Stage(It.IsAny<IReadOnlyCollection<MemberImportModel>>()), Times.Once);
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
