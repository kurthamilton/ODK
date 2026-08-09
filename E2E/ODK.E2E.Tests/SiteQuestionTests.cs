using FluentAssertions;
using NUnit.Framework;
using ODK.E2E.Data;
using ODK.E2E.Tests.Helpers;
using ODK.E2E.Tests.Pages;

namespace ODK.E2E.Tests;

/// <summary>
/// Site FAQ management and display. Default platform only - Drunken Knitwits has no site questions, and
/// its About page is expected to stay absent (see SiteViewModelService.GetAboutPage, which 404s when the
/// platform has no questions).
/// </summary>
[TestFixture]
[Category("SiteQuestions")]
public class SiteQuestionTests : DefaultPageTest
{
    [Test]
    public async Task CreateQuestion_ShowsItOnTheAboutPage()
    {
        // Arrange - a site admin, since site questions are site-admin managed.
        var admin = await SharedAccounts.Get(SharedAccounts.SiteAdmin);
        await new LoginPage(Page).LogIn(admin.Email, admin.Password);

        var question = TestQuestionName();
        var answer = $"Answer {Guid.NewGuid():N}";

        // Act
        await new SiteAdminQuestionsPage(Page).Create(question, answer);

        // Assert - persisted, and visible to a visitor on the public page.
        (await SiteQuestionDataHelper.QuestionExists(question)).Should().BeTrue();
        (await new AboutPage(Page).HasQuestion(question, answer)).Should().BeTrue();
    }

    [Test]
    public async Task CreateQuestion_FilesItUnderTheCurrentPlatform()
    {
        // Arrange - questions are per-platform, so one created while browsing Group Squirrel must not
        // appear in the Drunken Knitwits FAQ. This fixture only drives the Default platform, so assert on
        // the stored platform rather than by visiting the other site.
        var admin = await SharedAccounts.Get(SharedAccounts.SiteAdmin);
        await new LoginPage(Page).LogIn(admin.Email, admin.Password);

        var question = TestQuestionName();

        // Act
        await new SiteAdminQuestionsPage(Page).Create(question, "Answer");

        // Assert - PlatformType.Default is 1.
        (await SiteQuestionDataHelper.GetPlatform(question)).Should().Be(1);
    }

    [Test]
    public async Task CreateQuestion_ListsItInTheSiteAdminFaq()
    {
        // Arrange
        var admin = await SharedAccounts.Get(SharedAccounts.SiteAdmin);
        await new LoginPage(Page).LogIn(admin.Email, admin.Password);

        var question = TestQuestionName();
        var questionsPage = new SiteAdminQuestionsPage(Page);

        // Act
        await questionsPage.Create(question, "Answer");

        // Assert
        (await questionsPage.IsListed(question)).Should().BeTrue();
    }

    [Test]
    public async Task CreateQuestions_AppendsEachToTheEndOfTheDisplayOrder()
    {
        // Arrange
        var admin = await SharedAccounts.Get(SharedAccounts.SiteAdmin);
        await new LoginPage(Page).LogIn(admin.Email, admin.Password);

        var first = TestQuestionName();
        var second = TestQuestionName();
        var questionsPage = new SiteAdminQuestionsPage(Page);

        // Act
        await questionsPage.Create(first, "First answer");
        await questionsPage.Create(second, "Second answer");

        // Assert - the second sorts after the first, whatever the starting order was.
        var firstOrder = await SiteQuestionDataHelper.GetDisplayOrder(first);
        var secondOrder = await SiteQuestionDataHelper.GetDisplayOrder(second);
        firstOrder.Should().NotBeNull();
        secondOrder.Should().BeGreaterThan(firstOrder!.Value);
    }

    [Test]
    public async Task MoveQuestionUp_ReordersItOnTheAboutPage()
    {
        // Arrange - two questions created in a known order.
        var admin = await SharedAccounts.Get(SharedAccounts.SiteAdmin);
        await new LoginPage(Page).LogIn(admin.Email, admin.Password);

        var first = TestQuestionName();
        var second = TestQuestionName();
        var questionsPage = new SiteAdminQuestionsPage(Page);
        await questionsPage.Create(first, "First answer");
        await questionsPage.Create(second, "Second answer");

        var secondId = await SiteQuestionDataHelper.GetQuestionId(second);
        secondId.Should().NotBeNull();

        // Act
        await questionsPage.MoveUp(secondId!.Value);

        // Assert - the display order the admin sets is the order visitors read.
        var displayed = await new AboutPage(Page).QuestionsInOrder();
        displayed.Should().ContainInOrder(second, first);
    }

    [Test]
    public async Task UpdateQuestion_ShowsTheNewTextOnTheAboutPage()
    {
        // Arrange
        var admin = await SharedAccounts.Get(SharedAccounts.SiteAdmin);
        await new LoginPage(Page).LogIn(admin.Email, admin.Password);

        var question = TestQuestionName();
        var questionsPage = new SiteAdminQuestionsPage(Page);
        await questionsPage.Create(question, "Original answer");

        var questionId = await SiteQuestionDataHelper.GetQuestionId(question);
        questionId.Should().NotBeNull();

        var updated = TestQuestionName();
        var updatedAnswer = $"Updated {Guid.NewGuid():N}";

        // Act
        await questionsPage.Update(questionId!.Value, updated, updatedAnswer);

        // Assert
        (await new AboutPage(Page).HasQuestion(updated, updatedAnswer)).Should().BeTrue();
    }

    [Test]
    public async Task DeleteQuestion_RemovesItFromTheAboutPage()
    {
        // Arrange
        var admin = await SharedAccounts.Get(SharedAccounts.SiteAdmin);
        await new LoginPage(Page).LogIn(admin.Email, admin.Password);

        var question = TestQuestionName();
        var questionsPage = new SiteAdminQuestionsPage(Page);
        await questionsPage.Create(question, "Answer");

        var questionId = await SiteQuestionDataHelper.GetQuestionId(question);
        questionId.Should().NotBeNull();

        // Act
        await questionsPage.Delete(questionId!.Value);

        // Assert
        (await SiteQuestionDataHelper.QuestionExists(question)).Should().BeFalse();
        (await new AboutPage(Page).QuestionsInOrder()).Should().NotContain(question);
    }

    [Test]
    public async Task AboutPage_WithQuestions_IsLinkedFromTheFooter()
    {
        // Arrange - the footer link is rendered whenever the platform has questions, so make sure it does.
        var admin = await SharedAccounts.Get(SharedAccounts.SiteAdmin);
        await new LoginPage(Page).LogIn(admin.Email, admin.Password);
        await new SiteAdminQuestionsPage(Page).Create(TestQuestionName(), "Answer");

        // Act
        var hasLink = await new AboutPage(Page).HasFooterLink();

        // Assert
        hasLink.Should().BeTrue();
    }

    /// <summary>
    /// Carries the prefix TestDataCleaner looks for - site questions belong to a platform rather than to a
    /// member, so nothing cascades them away when the test accounts are removed.
    /// </summary>
    private static string TestQuestionName()
        => $"{SiteQuestionDataHelper.TestNamePrefix}{Guid.NewGuid():N}";
}
