using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using ODK.Core.Members;
using ODK.Core.Platforms;
using ODK.Core.Referrals;
using ODK.Services.Exceptions;
using ODK.Services.Referrals;
using ODK.Services.Referrals.Models;
using ODK.Services.Tests.Helpers;

namespace ODK.Services.Tests.Referrals;

[Parallelizable]
public static class ReferralAdminServiceTests
{
    [Test]
    public static async Task CreateCampaign_BlankName_Fails()
    {
        // Arrange
        var (context, currentMember) = CreateContext(siteAdmin: true);
        var (service, request) = CreateService(context, currentMember);

        // Act
        var result = await service.CreateCampaign(request, CreateModel("   "));

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Be("Name required");
        context.Set<ReferralCampaign>().Should().BeEmpty();
    }

    [Test]
    public static async Task CreateCampaign_NoExpiry_StoresNull()
    {
        // Arrange
        var (context, currentMember) = CreateContext(siteAdmin: true);
        var (service, request) = CreateService(context, currentMember);

        // Act
        var result = await service.CreateCampaign(request, CreateModel("Spring drive", expires: null));

        // Assert
        result.Success.Should().BeTrue();
        context.Set<ReferralCampaign>().Single().ExpiresUtc.Should().BeNull();
    }

    [Test]
    public static async Task CreateCampaign_NormalisesName()
    {
        // Arrange
        var (context, currentMember) = CreateContext(siteAdmin: true);
        var (service, request) = CreateService(context, currentMember);

        // Act
        await service.CreateCampaign(request, CreateModel("  Spring   drive  "));

        // Assert
        context.Set<ReferralCampaign>().Single().Name.Should().Be("Spring drive");
    }

    [Test]
    public static async Task CreateCampaign_NotSiteAdmin_Throws()
    {
        // Arrange
        var (context, currentMember) = CreateContext(siteAdmin: false);
        var (service, request) = CreateService(context, currentMember);

        // Act
        var act = async () => await service.CreateCampaign(request, CreateModel("Spring drive"));

        // Assert
        await act.Should().ThrowAsync<OdkNotAuthorizedException>();
    }

    // The expiry is stored as the instant the chosen day *ends*, so a campaign expiring on the 31st is
    // still open all day on the 31st. The second case is a British Summer Time date, where the day ends an
    // hour before midnight UTC - pinning that the boundary is converted rather than assumed to be UTC.
    [TestCase("2026-12-31", ExpectedResult = "2027-01-01 00:00")]
    [TestCase("2026-06-30", ExpectedResult = "2026-06-30 23:00")]
    public static async Task<string> CreateCampaign_StoresExpiryAsEndOfTheChosenDay(string expires)
    {
        // Arrange
        var (context, currentMember) = CreateContext(siteAdmin: true);
        var (service, request) = CreateService(context, currentMember);

        // Act
        await service.CreateCampaign(request, CreateModel("Spring drive", DateTime.Parse(expires)));

        // Assert
        var referralCampaign = context.Set<ReferralCampaign>().Single();
        referralCampaign.ExpiresUtc.Should().NotBeNull();

        return referralCampaign.ExpiresUtc.Value.ToString("yyyy-MM-dd HH:mm");
    }

    [Test]
    public static async Task GetCampaignsViewModel_CountsSentAndCompleted()
    {
        // Arrange
        var (context, currentMember) = CreateContext(siteAdmin: true);
        var member = context.CreateMember();
        var campaign = CreateCampaign(context, "Spring drive");
        var other = CreateCampaign(context, "Other campaign");
        CreateReferral(context, campaign, member, completed: true);
        CreateReferral(context, campaign, member, completed: false);
        CreateReferral(context, campaign, member, completed: false);
        CreateReferral(context, other, member, completed: true);
        var (service, request) = CreateService(context, currentMember);

        // Act
        var result = await service.GetCampaignsViewModel(request);

        // Assert - counts are per campaign, and Completed is a subset of Sent rather than a separate total.
        var summary = result.Campaigns.Single(x => x.Campaign.Id == campaign.Id);
        summary.SentCount.Should().Be(3);
        summary.CompletedCount.Should().Be(1);
    }

    [Test]
    public static async Task GetCampaignsViewModel_OrdersMostRecentFirst()
    {
        // Arrange
        var (context, currentMember) = CreateContext(siteAdmin: true);
        CreateCampaign(context, "Oldest", DateTime.UtcNow.AddDays(-3));
        CreateCampaign(context, "Newest", DateTime.UtcNow.AddDays(-1));
        CreateCampaign(context, "Middle", DateTime.UtcNow.AddDays(-2));
        var (service, request) = CreateService(context, currentMember);

        // Act
        var result = await service.GetCampaignsViewModel(request);

        // Assert
        result.Campaigns.Select(x => x.Campaign.Name).Should().Equal("Newest", "Middle", "Oldest");
    }

    [Test]
    public static async Task GetCampaignViewModel_ReturnsOnlyThisCampaignsReferrals()
    {
        // Arrange
        var (context, currentMember) = CreateContext(siteAdmin: true);
        var member = context.CreateMember();
        var campaign = CreateCampaign(context, "Spring drive");
        var other = CreateCampaign(context, "Other campaign");
        CreateReferral(context, campaign, member, completed: false);
        CreateReferral(context, other, member, completed: false);
        var (service, request) = CreateService(context, currentMember);

        // Act
        var result = await service.GetCampaignViewModel(request, campaign.Id);

        // Assert
        result.Campaign.Name.Should().Be("Spring drive");
        result.Referrals.Should().HaveCount(1);
        result.Referrals.Single().Member.Id.Should().Be(member.Id);
    }

    [Test]
    public static async Task UpdateCampaign_ChangesNameAndExpiry()
    {
        // Arrange
        var (context, currentMember) = CreateContext(siteAdmin: true);
        var campaign = CreateCampaign(context, "Spring drive");
        var (service, request) = CreateService(context, currentMember);

        // Act
        var result = await service.UpdateCampaign(
            request, campaign.Id, CreateModel("Summer drive", new DateTime(2026, 12, 31)));

        // Assert
        result.Success.Should().BeTrue();
        var updated = context.Set<ReferralCampaign>().Single();
        updated.Name.Should().Be("Summer drive");
        updated.ExpiresUtc.Should().Be(new DateTime(2027, 1, 1, 0, 0, 0));
    }

    [Test]
    public static async Task UpdateCampaign_ClearsExpiry()
    {
        // Arrange
        var (context, currentMember) = CreateContext(siteAdmin: true);
        var campaign = CreateCampaign(context, "Spring drive", expiresUtc: DateTime.UtcNow.AddDays(30));
        var (service, request) = CreateService(context, currentMember);

        // Act
        await service.UpdateCampaign(request, campaign.Id, CreateModel("Spring drive", expires: null));

        // Assert
        context.Set<ReferralCampaign>().Single().ExpiresUtc.Should().BeNull();
    }

    private static ReferralCampaign CreateCampaign(
        MockOdkContext context, string name, DateTime? createdUtc = null, DateTime? expiresUtc = null)
        => context.Create(new ReferralCampaign
        {
            CreatedUtc = createdUtc ?? DateTime.UtcNow,
            ExpiresUtc = expiresUtc,
            Id = Guid.NewGuid(),
            Name = name
        });

    // Split from CreateService so a test can arrange its rows first: MockUnitOfWork.Create saves the
    // context, so anything added afterwards never reaches the (in-memory) database.
    private static (MockOdkContext Context, Member CurrentMember) CreateContext(bool siteAdmin)
    {
        var context = new MockOdkContext();
        return (context, context.CreateMember(siteAdmin: siteAdmin));
    }

    private static ReferralCampaignUpdateModel CreateModel(string name, DateTime? expires = null) => new()
    {
        Description = "<p>Refer a friend</p>",
        EmailSubject = "You have been referred",
        EmailText = "<p>Hello</p>",
        ExpiresLocalDate = expires,
        Name = name
    };

    private static Referral CreateReferral(
        MockOdkContext context, ReferralCampaign campaign, Member member, bool completed)
        => context.Create(new Referral
        {
            CompletedUtc = completed ? DateTime.UtcNow : null,
            CreatedUtc = DateTime.UtcNow,
            EmailAddress = "referred@example.com",
            Id = Guid.NewGuid(),
            MemberId = member.Id,
            ReferralCampaignId = campaign.Id
        });

    private static (IReferralAdminService Service, IMemberServiceRequest Request) CreateService(
        MockOdkContext context, Member currentMember)
    {
        var request = new Mock<IMemberServiceRequest>();
        request.Setup(x => x.CurrentMember).Returns(currentMember);
        request.Setup(x => x.CurrentMemberOrDefault).Returns(currentMember);
        request.Setup(x => x.Platform).Returns(PlatformType.Default);

        return (new ReferralAdminService(MockUnitOfWorkFactory.Create(context)), request.Object);
    }
}
