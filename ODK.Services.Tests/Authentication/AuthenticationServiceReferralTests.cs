using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using ODK.Core.Countries;
using ODK.Core.Members;
using ODK.Core.Referrals;
using ODK.Services.Authentication;
using ODK.Services.Emails;
using ODK.Services.Members;
using ODK.Services.Notifications;
using Microsoft.Extensions.DependencyInjection;
using ODK.Core.Workflows;
using ODK.Data.Core;
using ODK.Services.Authentication.OAuth;
using ODK.Services.Authorization;
using ODK.Services.Emails.Validation;
using ODK.Services.Geolocation;
using ODK.Services.Logging;
using ODK.Services.Members.Workflows.Account;
using ODK.Services.Members.Workflows.ChapterMembership;
using ODK.Services.Recaptcha;
using ODK.Services.Subscriptions;
using ODK.Services.Topics;
using ODK.Services.Workflows;
using ODK.Services.Tests.Helpers;

namespace ODK.Services.Tests.Authentication;

/// <summary>
/// Covers only the referral completion that hangs off a successful login - the rest of
/// <see cref="AuthenticationService"/> is out of scope here.
/// </summary>
[Parallelizable]
public static class AuthenticationServiceReferralTests
{
    private const string Password = "correct-horse";

    [Test]
    public static async Task GetMemberAsync_FirstLoginOfAReferredMember_CompletesTheReferral()
    {
        // Arrange
        var (context, member, referral) = CreateReferredMember();
        var service = CreateService(context);

        // Act
        var result = await service.GetMemberAsync(member.EmailAddress, Password);

        // Assert
        result.Should().NotBeNull();
        context.Set<Referral>().Single(x => x.Id == referral.Id).CompletedUtc.Should().NotBeNull();
    }

    [Test]
    public static async Task GetMemberAsync_FailedLogin_LeavesTheReferralIncomplete()
    {
        // Arrange - completion hangs off a *successful* login, so a wrong password must not trigger it.
        var (context, member, referral) = CreateReferredMember();
        var service = CreateService(context);

        // Act
        var result = await service.GetMemberAsync(member.EmailAddress, "wrong-password");

        // Assert
        result.Should().BeNull();
        context.Set<Referral>().Single(x => x.Id == referral.Id).CompletedUtc.Should().BeNull();
    }

    [Test]
    public static async Task GetMemberAsync_MemberWithNoReferral_Succeeds()
    {
        // Arrange - the overwhelming majority of logins. Nothing to complete, and nothing should break.
        var context = new MockOdkContext();
        var member = CreateMemberWithPassword(context);
        var service = CreateService(context);

        // Act
        var result = await service.GetMemberAsync(member.EmailAddress, Password);

        // Assert
        result.Should().NotBeNull();
        context.Set<Referral>().Should().BeEmpty();
    }

    [Test]
    public static async Task GetMemberAsync_SecondLogin_KeepsTheOriginalCompletionTime()
    {
        // Arrange - the timestamp records the *first* login, so a later one must not move it.
        var (context, member, referral) = CreateReferredMember();
        var service = CreateService(context);

        // Act
        await service.GetMemberAsync(member.EmailAddress, Password);
        var firstCompletedUtc = context.Set<Referral>().Single(x => x.Id == referral.Id).CompletedUtc;
        await service.GetMemberAsync(member.EmailAddress, Password);

        // Assert
        firstCompletedUtc.Should().NotBeNull();
        context.Set<Referral>().Single(x => x.Id == referral.Id).CompletedUtc.Should().Be(firstCompletedUtc);
    }

    private static Member CreateMemberWithPassword(MockOdkContext context, Guid? referralId = null)
    {
        var member = context.CreateMember(activated: true, afterCreate: x =>
        {
            x.EmailAddress = "member@example.com";
            x.ReferralId = referralId;
        });

        context.Create(new MemberPassword
        {
            Hash = Password,
            MemberId = member.Id
        });

        return member;
    }

    private static (MockOdkContext Context, Member Member, Referral Referral) CreateReferredMember()
    {
        var context = new MockOdkContext();

        var referrer = context.CreateMember(afterCreate: x => x.EmailAddress = "referrer@example.com");
        var campaign = context.Create(new ReferralCampaign
        {
            CreatedUtc = DateTime.UtcNow.AddDays(-1),
            Id = Guid.NewGuid(),
            Name = "Spring drive"
        });
        var referral = context.Create(new Referral
        {
            CreatedUtc = DateTime.UtcNow,
            EmailAddress = "member@example.com",
            Id = Guid.NewGuid(),
            MemberId = referrer.Id,
            ReferralCampaignId = campaign.Id
        });

        var member = CreateMemberWithPassword(context, referral.Id);

        return (context, member, referral);
    }

    private static IAuthenticationService CreateService(MockOdkContext context)
    {
        // A hasher that treats the stored value as the plain text, so the tests don't depend on the real
        // hashing scheme - what's under test is what happens after the check passes.
        var passwordHasher = new Mock<IPasswordHasher>();
        passwordHasher
            .Setup(x => x.Check(It.IsAny<string>(), It.IsAny<IHashedPassword>()))
            .Returns((string plainText, IHashedPassword hashed) => plainText == hashed.Hash);
        passwordHasher.Setup(x => x.ShouldUpdate(It.IsAny<IHashedPassword>())).Returns(false);
        passwordHasher
            .Setup(x => x.ComputeHash(It.IsAny<string>()))
            .Returns((string plainText) => (plainText, Mock.Of<IHashedPasswordOptions>()));

        var unitOfWork = MockUnitOfWork.Create(context);
        var workflow = CreateAccountWorkflow(unitOfWork);

        return new AuthenticationService(
            new AuthenticationServiceSettings { PasswordResetTokenLifetimeMinutes = 60 },
            unitOfWork,
            Mock.Of<IMemberEmailService>(),
            passwordHasher.Object,
            new MemberPasswordService(
                Mock.Of<IPasswordPolicy>(), passwordHasher.Object, Mock.Of<IBreachedPasswordChecker>()),
            new EmailValidationService(new InconclusiveEmailVerifier()),
            workflow.GetRequiredService<StateMachineRunner<AccountState, AccountTrigger, AccountContext>>(),
            workflow.GetRequiredService<IAccountContextFactory>());
    }

    /// <summary>
    /// The account machine wired the way the app wires it. These tests only exercise logging in, which fires
    /// no transition - but the service takes the runner, so it has to resolve.
    /// </summary>
    private static IServiceProvider CreateAccountWorkflow(IUnitOfWork unitOfWork)
    {
        var definition = AccountStateMachine.Create();

        var services = new ServiceCollection()
            .AddSingleton(unitOfWork)
            .AddSingleton(definition)
            .AddSingleton(Mock.Of<IAuthorizationService>())
            .AddSingleton(Mock.Of<IDistanceUnitFactory>())
            .AddSingleton(Mock.Of<IGeolocationService>())
            .AddSingleton(Mock.Of<ILoggingService>())
            .AddSingleton(Mock.Of<IMemberEmailService>())
            .AddSingleton(Mock.Of<IMemberImageService>())
            .AddSingleton(Mock.Of<IMemberPasswordService>())
            .AddSingleton(Mock.Of<IMemberSiteSubscriptionWriter>())
            .AddSingleton(Mock.Of<INotificationService>())
            .AddSingleton(Mock.Of<IOAuthProviderFactory>())
            .AddSingleton(Mock.Of<IRecaptchaService>())
            .AddSingleton(Mock.Of<ITopicService>())
            .AddSingleton<IEmailValidationService>(
                new EmailValidationService(new InconclusiveEmailVerifier()))
            .AddSingleton(Mock.Of<IMemberChapterSubscriptionWriter>())
            .AddSingleton(ChapterMembershipStateMachine.Create())
            .AddScoped<IAccountContextFactory, AccountContextFactory>()
            .AddScoped<IChapterMembershipContextFactory, ChapterMembershipContextFactory>()
            .AddScoped<IStateResolver<AccountState, AccountContext>, AccountStateResolver>()
            .AddScoped<
                IStateResolver<ChapterMembershipState, ChapterMembershipContext>,
                ChapterMembershipStateResolver>()
            .AddScoped<IStepFactory<AccountContext>, ServiceProviderStepFactory<AccountContext>>()
            .AddScoped<
                IStepFactory<ChapterMembershipContext>,
                ServiceProviderStepFactory<ChapterMembershipContext>>()
            .AddScoped<StateMachineRunner<AccountState, AccountTrigger, AccountContext>>()
            .AddScoped<StateMachineRunner<
                ChapterMembershipState, ChapterMembershipTrigger, ChapterMembershipContext>>();

        foreach (var stepType in definition.StepTypes)
        {
            services.AddScoped(stepType);
        }

        return services.BuildServiceProvider();
    }
}
