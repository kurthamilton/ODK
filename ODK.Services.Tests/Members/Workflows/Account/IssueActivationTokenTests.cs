using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using ODK.Core.Countries;
using ODK.Core.Members;
using ODK.Services;
using ODK.Services.Authentication.OAuth;
using ODK.Services.Members.Models;
using ODK.Services.Members.Workflows.Account;
using ODK.Services.Members.Workflows.Account.Steps;
using ODK.Services.Tests.Helpers;

namespace ODK.Services.Tests.Members.Workflows.Account;

[Parallelizable]
public static class IssueActivationTokenTests
{
    [Test]
    public static async Task Execute_SignUpStatedAnIntent_CarriesItOntoTheToken()
    {
        /* Arrange - the token is the only thing joining a form filled in here to a link followed from an
           inbox, so an intent the sign-up stated has to survive on it or the journey ends at the site's
           front page. */
        using var context = new MockOdkContext();
        var unitOfWork = MockUnitOfWorkFactory.Create(context);

        var accountContext = Context(SignUpIntentType.CreateGroup);

        // Act
        await new IssueActivationToken(unitOfWork).Execute(accountContext, CancellationToken.None);
        await unitOfWork.SaveChanges();

        // Assert
        var token = context.Set<MemberActivationToken>().Should().ContainSingle().Subject;

        token.Intent.Should().Be(SignUpIntentType.CreateGroup);
        token.MemberId.Should().Be(accountContext.RequiredNewMember.Id);
    }

    [Test]
    public static async Task Execute_SignUpStatedNoIntent_LeavesTheTokenWithout()
    {
        // Arrange - null rather than None, which is not a valid target for the column's foreign key.
        using var context = new MockOdkContext();
        var unitOfWork = MockUnitOfWorkFactory.Create(context);

        // Act
        await new IssueActivationToken(unitOfWork).Execute(Context(intent: null), CancellationToken.None);
        await unitOfWork.SaveChanges();

        // Assert
        context.Set<MemberActivationToken>()
            .Should().ContainSingle().Subject
            .Intent.Should().BeNull();
    }

    [Test]
    public static async Task Execute_GroupSignUp_LeavesTheTokenWithoutAnIntent()
    {
        /* Arrange - only a sign-up to the site can state an intent. A group sign-up submits a different
           form, and joining a group is not something an intent has anything to add to. */
        using var context = new MockOdkContext();
        var unitOfWork = MockUnitOfWorkFactory.Create(context);

        var accountContext = new AccountContext
        {
            ActivationToken = "token",
            Request = Mock.Of<IServiceRequest>(),
            VerifiedByOAuth = false
        };

        accountContext.NewMember = new Member { Id = Guid.NewGuid() };

        // Act
        await new IssueActivationToken(unitOfWork).Execute(accountContext, CancellationToken.None);
        await unitOfWork.SaveChanges();

        // Assert
        context.Set<MemberActivationToken>()
            .Should().ContainSingle().Subject
            .Intent.Should().BeNull();
    }

    private static AccountContext Context(SignUpIntentType? intent)
    {
        var context = new AccountContext
        {
            ActivationToken = "token",
            Request = Mock.Of<IServiceRequest>(),
            SiteProfile = new AccountCreateModel
            {
                EmailAddress = "member@example.com",
                FirstName = "First",
                Intent = intent,
                LastName = "Last",
                Location = default(LatLong?),
                LocationName = string.Empty,
                NewTopics = [],
                OAuthProviderType = default(OAuthProviderType?),
                OAuthToken = null,
                RecaptchaToken = string.Empty,
                TopicIds = []
            },
            VerifiedByOAuth = false
        };

        context.NewMember = new Member { Id = Guid.NewGuid() };

        return context;
    }
}
