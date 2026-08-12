using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using ODK.Core.Exceptions;
using ODK.Core.Members;
using ODK.Core.Platforms;
using ODK.Core.Web;
using ODK.Services.Exceptions;
using ODK.Services.Html;
using ODK.Services.Questions;
using ODK.Services.Questions.Models;
using ODK.Services.Tests.Helpers;

namespace ODK.Services.Tests.Questions;

[Parallelizable]
public static class SiteQuestionAdminServiceTests
{
    [Test]
    public static async Task CreateQuestion_FirstQuestion_StartsTheDisplayOrderAtOne()
    {
        // Arrange
        using var context = new MockOdkContext();
        var member = context.CreateMember(siteAdmin: true);
        var service = CreateService(context);

        // Act
        var result = await service.CreateQuestion(
            CreateRequest(member, PlatformType.Default), CreateModel("Question", "Answer"));

        // Assert
        result.Success.Should().BeTrue();
        var questions = await service.GetQuestionsViewModel(CreateRequest(member, PlatformType.Default));
        questions.Questions.Single().DisplayOrder.Should().Be(1);
    }

    [Test]
    public static async Task CreateQuestion_ExistingQuestions_AppendsAfterTheHighestDisplayOrder()
    {
        // Arrange - the new question goes to the end rather than colliding with an order already in use.
        using var context = new MockOdkContext();
        var member = context.CreateMember(siteAdmin: true);
        context.AddRange(
            CreateQuestion(PlatformType.Default, "First", displayOrder: 1),
            CreateQuestion(PlatformType.Default, "Second", displayOrder: 2));
        var service = CreateService(context);

        // Act
        await service.CreateQuestion(
            CreateRequest(member, PlatformType.Default), CreateModel("Third", "Answer"));

        // Assert
        var questions = await service.GetQuestionsViewModel(CreateRequest(member, PlatformType.Default));
        questions.Questions.Single(x => x.Name == "Third").DisplayOrder.Should().Be(3);
    }

    [Test]
    public static async Task CreateQuestion_OtherPlatformHasQuestions_NumbersFromThisPlatformOnly()
    {
        // Arrange - display order is per-platform, so a busy Group Squirrel must not push Drunken
        // Knitwits' first question to order 4.
        using var context = new MockOdkContext();
        var member = context.CreateMember(siteAdmin: true);
        context.AddRange(
            CreateQuestion(PlatformType.Default, "First", displayOrder: 1),
            CreateQuestion(PlatformType.Default, "Second", displayOrder: 2),
            CreateQuestion(PlatformType.Default, "Third", displayOrder: 3));
        var service = CreateService(context);

        // Act
        await service.CreateQuestion(
            CreateRequest(member, PlatformType.DrunkenKnitwits), CreateModel("Only", "Answer"));

        // Assert
        var questions = await service.GetQuestionsViewModel(
            CreateRequest(member, PlatformType.DrunkenKnitwits));
        questions.Questions.Single().DisplayOrder.Should().Be(1);
    }

    [Test]
    public static async Task DeleteQuestion_ClosesTheGapInDisplayOrder()
    {
        // Arrange - leaving a hole would let the next created question reuse a number still in use.
        using var context = new MockOdkContext();
        var member = context.CreateMember(siteAdmin: true);
        var second = CreateQuestion(PlatformType.Default, "Second", displayOrder: 2);
        context.AddRange(
            CreateQuestion(PlatformType.Default, "First", displayOrder: 1),
            second,
            CreateQuestion(PlatformType.Default, "Third", displayOrder: 3));
        var service = CreateService(context);

        // Act
        await service.DeleteQuestion(CreateRequest(member, PlatformType.Default), second.Id);

        // Assert
        var questions = await service.GetQuestionsViewModel(CreateRequest(member, PlatformType.Default));
        questions.Questions
            .OrderBy(x => x.DisplayOrder)
            .Select(x => (x.Name, x.DisplayOrder))
            .Should()
            .Equal(("First", 1), ("Third", 2));
    }

    [Test]
    public static async Task GetQuestionViewModel_QuestionBelongsToAnotherPlatform_Throws()
    {
        // Arrange - the id is real, so only the platform scoping stops a Group Squirrel site admin from
        // reading (and then editing) a Drunken Knitwits question.
        using var context = new MockOdkContext();
        var member = context.CreateMember(siteAdmin: true);
        var question = CreateQuestion(PlatformType.DrunkenKnitwits, "Theirs", displayOrder: 1);
        context.AddRange(question);
        var service = CreateService(context);

        // Act
        var act = async () => await service.GetQuestionViewModel(
            CreateRequest(member, PlatformType.Default), question.Id);

        // Assert
        await act.Should().ThrowAsync<OdkNotFoundException>();
    }

    [Test]
    public static async Task UpdateQuestion_QuestionBelongsToAnotherPlatform_Throws()
    {
        // Arrange
        using var context = new MockOdkContext();
        var member = context.CreateMember(siteAdmin: true);
        var question = CreateQuestion(PlatformType.DrunkenKnitwits, "Theirs", displayOrder: 1);
        context.AddRange(question);
        var service = CreateService(context);

        // Act
        var act = async () => await service.UpdateQuestion(
            CreateRequest(member, PlatformType.Default), question.Id, CreateModel("Mine", "Answer"));

        // Assert
        await act.Should().ThrowAsync<OdkNotFoundException>();
    }

    [Test]
    public static async Task UpdateQuestionDisplayOrder_MoveUp_SwapsWithThePrecedingQuestion()
    {
        // Arrange
        using var context = new MockOdkContext();
        var member = context.CreateMember(siteAdmin: true);
        var second = CreateQuestion(PlatformType.Default, "Second", displayOrder: 2);
        context.AddRange(CreateQuestion(PlatformType.Default, "First", displayOrder: 1), second);
        var service = CreateService(context);

        // Act
        await service.UpdateQuestionDisplayOrder(
            CreateRequest(member, PlatformType.Default), second.Id, moveBy: -1);

        // Assert
        var questions = await service.GetQuestionsViewModel(CreateRequest(member, PlatformType.Default));
        questions.Questions
            .OrderBy(x => x.DisplayOrder)
            .Select(x => x.Name)
            .Should()
            .Equal("Second", "First");
    }

    [Test]
    public static async Task UpdateQuestionDisplayOrder_AlreadyFirst_LeavesTheOrderAlone()
    {
        // Arrange - there is nothing to swap with, so moving up must be a no-op rather than an error.
        using var context = new MockOdkContext();
        var member = context.CreateMember(siteAdmin: true);
        var first = CreateQuestion(PlatformType.Default, "First", displayOrder: 1);
        context.AddRange(first, CreateQuestion(PlatformType.Default, "Second", displayOrder: 2));
        var service = CreateService(context);

        // Act
        await service.UpdateQuestionDisplayOrder(
            CreateRequest(member, PlatformType.Default), first.Id, moveBy: -1);

        // Assert
        var questions = await service.GetQuestionsViewModel(CreateRequest(member, PlatformType.Default));
        questions.Questions
            .OrderBy(x => x.DisplayOrder)
            .Select(x => x.Name)
            .Should()
            .Equal("First", "Second");
    }

    [Test]
    public static async Task GetQuestionsViewModel_ReturnsOnlyTheRequestPlatformQuestions()
    {
        // Arrange
        using var context = new MockOdkContext();
        var member = context.CreateMember(siteAdmin: true);
        context.AddRange(
            CreateQuestion(PlatformType.Default, "Ours", displayOrder: 1),
            CreateQuestion(PlatformType.DrunkenKnitwits, "Theirs", displayOrder: 1));
        var service = CreateService(context);

        // Act
        var viewModel = await service.GetQuestionsViewModel(CreateRequest(member, PlatformType.Default));

        // Assert
        viewModel.Questions.Select(x => x.Name).Should().Equal("Ours");
    }

    [TestCase("", "Answer")]
    [TestCase("Question", "")]
    [TestCase(" ", "Answer")]
    public static async Task CreateQuestion_MissingRequiredField_Fails(string name, string answer)
    {
        // Arrange
        using var context = new MockOdkContext();
        var member = context.CreateMember(siteAdmin: true);
        var service = CreateService(context);

        // Act
        var result = await service.CreateQuestion(
            CreateRequest(member, PlatformType.Default), CreateModel(name, answer));

        // Assert
        result.Success.Should().BeFalse();
    }

    [Test]
    public static async Task CreateQuestion_NotASiteAdmin_Throws()
    {
        // Arrange
        using var context = new MockOdkContext();
        var member = context.CreateMember(siteAdmin: false);
        var service = CreateService(context);

        // Act
        var act = async () => await service.CreateQuestion(
            CreateRequest(member, PlatformType.Default), CreateModel("Question", "Answer"));

        // Assert
        await act.Should().ThrowAsync<OdkNotAuthorizedException>();
    }

    private static SiteQuestionUpdateModel CreateModel(string name, string answer) => new()
    {
        Answer = answer,
        Name = name
    };

    private static SiteQuestion CreateQuestion(PlatformType platform, string name, int displayOrder) => new()
    {
        Answer = $"{name} answer",
        DisplayOrder = displayOrder,
        Id = Guid.NewGuid(),
        Name = name,
        Platform = platform
    };

    private static IMemberServiceRequest CreateRequest(Member member, PlatformType platform)
    {
        var mock = new Mock<IMemberServiceRequest>();
        mock.Setup(x => x.CurrentMember).Returns(member);
        mock.Setup(x => x.CurrentMemberOrDefault).Returns(member);
        mock.Setup(x => x.Platform).Returns(platform);
        return mock.Object;
    }

    private static SiteQuestionAdminService CreateService(MockOdkContext context)
    {
        var htmlValidator = new Mock<IHtmlValidator>();
        htmlValidator
            .Setup(x => x.Validate(It.IsAny<string?>(), It.IsAny<HtmlValidatorOptions>()))
            .Returns(ServiceResult.Successful());

        return new SiteQuestionAdminService(MockUnitOfWork.Create(context), htmlValidator.Object);
    }
}
