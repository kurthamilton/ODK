using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using NUnit.Framework;
using ODK.Services.Members.Models;
using ODK.Web.Razor.Services;

namespace ODK.Web.Razor.Tests.Services;

[Parallelizable]
public static class MemberImportStagingServiceTests
{
    [Test]
    public static void Retrieve_AfterRemove_ReturnsNull()
    {
        var service = CreateService();
        var token = service.Stage(Members());

        service.Remove(token);

        service.Retrieve(token).Should().BeNull();
    }

    [Test]
    public static void Retrieve_AfterStage_ReturnsStagedMembers()
    {
        var service = CreateService();
        var members = Members();

        var token = service.Stage(members);

        service.Retrieve(token).Should().BeEquivalentTo(members);
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("does-not-exist")]
    public static void Retrieve_MissingOrUnknownToken_ReturnsNull(string? token)
        => CreateService().Retrieve(token).Should().BeNull();

    private static MemberImportStagingService CreateService()
        => new MemberImportStagingService(new MemoryCache(new MemoryCacheOptions()));

    private static IReadOnlyCollection<MemberImportModel> Members()
        =>
        [
            new MemberImportModel { EmailAddress = "a@b.com", FirstName = "A", LastName = "B" }
        ];
}
