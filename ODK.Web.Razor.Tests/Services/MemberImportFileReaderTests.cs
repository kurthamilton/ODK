using System.Text;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using NUnit.Framework;
using ODK.Services.Csv;
using ODK.Services.Integrations.Csv;
using ODK.Services.Members.Models;
using ODK.Web.Razor.Services;

namespace ODK.Web.Razor.Tests.Services;

[Parallelizable]
public static class MemberImportFileReaderTests
{
    [Test]
    public static void Read_NoFile_ReturnsFailure()
    {
        // Act
        var result = CreateReader().Read(file: null);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("No file uploaded");
    }

    [Test]
    public static void Read_TooLarge_ReturnsFailure()
    {
        // Act
        var result = CreateReader().Read(File("members.csv", "text/csv", 6 * 1024 * 1024));

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("too large");
    }

    [Test]
    public static void Read_ValidFile_ReturnsEveryRow()
    {
        // Arrange - the real reader, because what a CSV binds to is the question.
        var csv = string.Join(
            Environment.NewLine,
            "FirstName,LastName,EmailAddress",
            "A,One,a@example.com",
            "B,Two,b@example.com");

        // Act
        var result = new MemberImportFileReader(new CsvReader()).Read(CsvFile(csv));

        // Assert
        result.Success.Should().BeTrue();
        result.Value!.Select(x => x.EmailAddress)
            .Should().BeEquivalentTo(["a@example.com", "b@example.com"]);
    }

    /* Blank addresses are passed through rather than dropped here: what a group does about a row it cannot
       use is the import service's, and the reader saying nothing about it is what lets that stay in one
       place. */
    [Test]
    public static void Read_RowWithNoEmailAddress_ReturnsItAnyway()
    {
        // Arrange
        var csvReader = new Mock<ICsvReader>();
        csvReader.Setup(x => x.Read<MemberImportCsvRow>(It.IsAny<Stream>()))
            .Returns([new MemberImportCsvRow { EmailAddress = " ", FirstName = "A", LastName = "B" }]);

        // Act
        var result = CreateReader(csvReader.Object).Read(File("members.csv", "text/csv", 10));

        // Assert
        result.Success.Should().BeTrue();
        result.Value.Should().ContainSingle();
    }

    [Test]
    public static void Read_WrongContentType_ReturnsFailure()
    {
        // Act
        var result = CreateReader().Read(File("members.csv", "application/zip", 10));

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Invalid content type");
    }

    [Test]
    public static void Read_WrongExtension_ReturnsFailure()
    {
        // Act
        var result = CreateReader().Read(File("members.txt", "text/csv", 10));

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain(".csv");
    }

    private static MemberImportFileReader CreateReader(ICsvReader? csvReader = null)
        => new MemberImportFileReader(csvReader ?? Mock.Of<ICsvReader>());

    private static IFormFile CsvFile(string content)
    {
        var bytes = Encoding.UTF8.GetBytes(content);
        return File("members.csv", "text/csv", bytes.Length, bytes);
    }

    private static IFormFile File(string fileName, string contentType, long length, byte[]? content = null)
    {
        var bytes = content ?? Encoding.UTF8.GetBytes("data");

        var file = new Mock<IFormFile>();
        file.SetupGet(x => x.FileName).Returns(fileName);
        file.SetupGet(x => x.ContentType).Returns(contentType);
        file.SetupGet(x => x.Length).Returns(length);
        file.Setup(x => x.OpenReadStream()).Returns(() => new MemoryStream(bytes));
        return file.Object;
    }
}
