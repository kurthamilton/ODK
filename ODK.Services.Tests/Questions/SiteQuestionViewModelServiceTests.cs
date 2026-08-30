using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using ODK.Core.Exceptions;
using ODK.Core.Platforms;
using ODK.Core.Web;
using ODK.Services.Platforms;
using ODK.Services.Questions;
using ODK.Services.Tests.Helpers;

namespace ODK.Services.Tests.Questions;

[Parallelizable]
public static class SiteQuestionViewModelServiceTests
{
    [Test]
    public static async Task GetAboutPage_NoQuestionsForThePlatform_Throws()
    {
        // Arrange - the page is nothing but its questions, so with none it should 404 rather than render
        // an empty shell. This is the only thing keeping /about off Drunken Knitwits.
        using var context = new MockOdkContext();
        var service = CreateService(context);

        // Act
        var act = async () => await service.GetAboutPage(CreateRequest(PlatformType.DrunkenKnitwits));

        // Assert
        await act.Should().ThrowAsync<OdkNotFoundException>();
    }

    [Test]
    public static async Task GetAboutPage_OnlyAnotherPlatformHasQuestions_Throws()
    {
        // Arrange - questions existing somewhere is not enough; they have to be this platform's.
        using var context = new MockOdkContext();
        context.AddRange(CreateQuestion(PlatformType.Default, "Ours"));
        var service = CreateService(context);

        // Act
        var act = async () => await service.GetAboutPage(CreateRequest(PlatformType.DrunkenKnitwits));

        // Assert
        await act.Should().ThrowAsync<OdkNotFoundException>();
    }

    [Test]
    public static async Task GetAboutPage_PlatformNamePlaceholder_IsEncodedInTheAnswer()
    {
        // Arrange - the answer is rendered unencoded, so a name carrying markup characters has to arrive
        // encoded. The question name is rendered as text and Razor encodes that itself.
        using var context = new MockOdkContext();
        context.AddRange(CreateQuestion(
            PlatformType.Default,
            "{platform.name}",
            answer: "Welcome to {platform.name}"));
        var service = CreateService(context, TestPlatformProvider.Create("Bells & Whistles"));

        // Act
        var viewModel = await service.GetAboutPage(CreateRequest(PlatformType.Default));

        // Assert
        var question = viewModel.Questions.Single();
        question.AnswerHtml.Should().Be("Welcome to Bells &amp; Whistles");
        question.Name.Should().Be("Bells & Whistles");
    }

    [Test]
    public static async Task GetAboutPage_PlatformNamePlaceholder_IsResolvedInTheAnswer()
    {
        // Arrange
        using var context = new MockOdkContext();
        context.AddRange(CreateQuestion(
            PlatformType.Default,
            "Where am I?",
            answer: "You are on {platform.name}"));
        var service = CreateService(context);

        // Act
        var viewModel = await service.GetAboutPage(CreateRequest(PlatformType.Default));

        // Assert
        viewModel.Questions.Single().AnswerHtml
            .Should().Be($"You are on {TestPlatformProvider.DefaultName}");
    }

    [Test]
    public static async Task GetAboutPage_PlatformNamePlaceholder_IsResolvedInTheName()
    {
        // Arrange
        using var context = new MockOdkContext();
        context.AddRange(CreateQuestion(PlatformType.Default, "What is {platform.name}?"));
        var service = CreateService(context);

        // Act
        var viewModel = await service.GetAboutPage(CreateRequest(PlatformType.Default));

        // Assert
        viewModel.Questions.Single().Name
            .Should().Be($"What is {TestPlatformProvider.DefaultName}?");
    }

    [Test]
    public static async Task GetAboutPage_ReturnsOnlyTheRequestPlatformQuestions()
    {
        // Arrange
        using var context = new MockOdkContext();
        context.AddRange(
            CreateQuestion(PlatformType.Default, "Ours"),
            CreateQuestion(PlatformType.DrunkenKnitwits, "Theirs"));
        var service = CreateService(context);

        // Act
        var viewModel = await service.GetAboutPage(CreateRequest(PlatformType.Default));

        // Assert
        viewModel.Questions.Select(x => x.Name).Should().Equal("Ours");
    }

    [Test]
    public static async Task GetAboutPage_ReturnsQuestionsInDisplayOrder()
    {
        // Arrange
        using var context = new MockOdkContext();
        context.AddRange(
            CreateQuestion(PlatformType.Default, "Second", displayOrder: 2),
            CreateQuestion(PlatformType.Default, "First", displayOrder: 1));
        var service = CreateService(context);

        // Act
        var viewModel = await service.GetAboutPage(CreateRequest(PlatformType.Default));

        // Assert
        viewModel.Questions.Select(x => x.Name).Should().Equal("First", "Second");
    }

    [Test]
    public static async Task HasAboutPage_NoQuestionsForThePlatform_ReturnsFalse()
    {
        // Arrange - callers link to /about only when this says yes, so it has to agree with the 404 above.
        using var context = new MockOdkContext();
        context.AddRange(CreateQuestion(PlatformType.Default, "Ours"));
        var service = CreateService(context);

        // Act
        var result = await service.HasAboutPage(CreateRequest(PlatformType.DrunkenKnitwits));

        // Assert
        result.Should().BeFalse();
    }

    [Test]
    public static async Task HasAboutPage_PlatformHasQuestions_ReturnsTrue()
    {
        // Arrange
        using var context = new MockOdkContext();
        context.AddRange(CreateQuestion(PlatformType.Default, "Ours"));
        var service = CreateService(context);

        // Act
        var result = await service.HasAboutPage(CreateRequest(PlatformType.Default));

        // Assert
        result.Should().BeTrue();
    }

    private static SiteQuestion CreateQuestion(
        PlatformType platform, string name, int displayOrder = 1, string? answer = null) => new()
    {
        AnswerHtml = answer ?? $"{name} answer",
        DisplayOrder = displayOrder,
        Id = Guid.NewGuid(),
        Name = name,
        Platform = platform
    };

    private static IServiceRequest CreateRequest(PlatformType platform)
    {
        var mock = new Mock<IServiceRequest>();
        mock.Setup(x => x.Platform).Returns(platform);
        return mock.Object;
    }

    private static SiteQuestionViewModelService CreateService(
        MockOdkContext context, IPlatformProvider? platformProvider = null)
        => new(
            MockUnitOfWorkFactory.Create(context),
            platformProvider ?? TestPlatformProvider.Create());
}
