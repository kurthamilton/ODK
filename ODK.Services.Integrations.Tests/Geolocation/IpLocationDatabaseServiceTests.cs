using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using FluentAssertions;
using Moq;
using ODK.Core.Members;
using ODK.Data.Core;
using ODK.Services;
using ODK.Services.Exceptions;
using ODK.Services.Integrations.Geolocation;
using ODK.Services.Logging;

namespace ODK.Services.Integrations.Tests.Geolocation;

[Parallelizable]
public static class IpLocationDatabaseServiceTests
{
    private const string Current = "dbip-city-lite-2026-09.mmdb";

    private const string Directory = @"C:\geoip";

    private const string Previous = "dbip-city-lite-2026-08.mmdb";

    [TestCase("../../appsettings.json")]
    [TestCase(@"..\..\appsettings.json")]
    [TestCase(@"C:\Windows\System32\config\SAM")]
    [TestCase("dbip-city-lite-2026-09.mmdb.tmp")]
    [TestCase("something-else.mmdb")]
    [TestCase("")]
    public static async Task Delete_NameIsNotADatabaseInTheDirectory_Fails(string fileName)
    {
        // Arrange
        var fileSystem = CreateFileSystem(Previous);

        // Act
        var result = await CreateService(fileSystem).Delete(CreateRequest(siteAdmin: true), fileName);

        // Assert
        result.Success.Should().BeFalse();
        fileSystem.AllFiles.Should().HaveCount(1);
    }

    [Test]
    public static async Task Delete_NotSiteAdmin_Throws()
    {
        // Arrange
        var fileSystem = CreateFileSystem(Previous);

        // Act
        var act = async () => await CreateService(fileSystem)
            .Delete(CreateRequest(siteAdmin: false), Previous);

        // Assert
        await act.Should().ThrowAsync<OdkNotAuthorizedException>();
        fileSystem.FileExists(Path(Previous)).Should().BeTrue();
    }

    [Test]
    public static async Task Delete_FileInUse_Fails()
    {
        // Arrange
        var fileSystem = CreateFileSystem(Current);

        // Act
        var result = await CreateService(fileSystem, loadedDatabase: Path(Current))
            .Delete(CreateRequest(siteAdmin: true), Current);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("in use");
        fileSystem.FileExists(Path(Current)).Should().BeTrue();
    }

    [Test]
    public static async Task Delete_OldFile_RemovesIt()
    {
        // Arrange
        var fileSystem = CreateFileSystem(Previous, Current);

        // Act
        var result = await CreateService(fileSystem, loadedDatabase: Path(Current))
            .Delete(CreateRequest(siteAdmin: true), Previous);

        // Assert
        result.Success.Should().BeTrue();
        fileSystem.FileExists(Path(Previous)).Should().BeFalse();
        fileSystem.FileExists(Path(Current)).Should().BeTrue();
    }

    [Test]
    public static async Task GetViewModel_MarksTheLoadedFile()
    {
        // Arrange
        var fileSystem = CreateFileSystem(Previous, Current);

        // Act
        var result = await CreateService(fileSystem, loadedDatabase: Path(Current))
            .GetViewModel(CreateRequest(siteAdmin: true));

        // Assert
        result.Files.Select(x => x.Name).Should().Equal(Current, Previous);
        result.Files.Single(x => x.Loaded).Name.Should().Be(Current);
    }

    [Test]
    public static async Task GetViewModel_NotSiteAdmin_Throws()
    {
        // Act
        var act = async () => await CreateService(CreateFileSystem(Current))
            .GetViewModel(CreateRequest(siteAdmin: false));

        // Assert
        await act.Should().ThrowAsync<OdkNotAuthorizedException>();
    }

    [Test]
    public static async Task Update_CurrentMonthAlreadyPresent_PrunesOlderFilesWithoutDownloading()
    {
        // Arrange
        var month = DateTime.UtcNow.ToString("yyyy-MM");
        var current = $"dbip-city-lite-{month}.mmdb";
        var fileSystem = CreateFileSystem(Previous, current);

        var lookup = new Mock<IIpLocationLookup>();

        // Act
        await CreateService(fileSystem, lookup: lookup).Update();

        // Assert
        fileSystem.FileExists(Path(current)).Should().BeTrue();
        fileSystem.FileExists(Path(Previous)).Should().BeFalse();
        lookup.Verify(x => x.Reload(), Times.Never);
    }

    private static MockFileSystem CreateFileSystem(params string[] fileNames)
    {
        var fileSystem = new MockFileSystem();
        fileSystem.AddDirectory(Directory);

        foreach (var fileName in fileNames)
        {
            fileSystem.AddFile(Path(fileName), new MockFileData("mmdb"));
        }

        return fileSystem;
    }

    private static IMemberServiceRequest CreateRequest(bool siteAdmin)
    {
        var request = new Mock<IMemberServiceRequest>();
        request.Setup(x => x.CurrentMember).Returns(new Member { SiteAdmin = siteAdmin });
        return request.Object;
    }

    private static IpLocationDatabaseService CreateService(
        IFileSystem fileSystem,
        string? loadedDatabase = null,
        Mock<IIpLocationLookup>? lookup = null)
    {
        lookup ??= new Mock<IIpLocationLookup>();
        lookup.Setup(x => x.CurrentDatabase).Returns(loadedDatabase);

        return new IpLocationDatabaseService(
            new Mock<IUnitOfWork>().Object,
            new GeolocationServiceSettings
            {
                GoogleApiKey = string.Empty,
                IpDatabaseDirectory = Directory,
                PreloadIpDatabase = false
            },
            fileSystem,
            new Mock<IHttpClientFactory>().Object,
            new Mock<ILoggingService>().Object,
            lookup.Object);
    }

    private static string Path(string fileName) => System.IO.Path.Combine(Directory, fileName);
}
