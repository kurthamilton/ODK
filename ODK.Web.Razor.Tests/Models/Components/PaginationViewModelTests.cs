using FluentAssertions;
using NUnit.Framework;
using ODK.Web.Razor.Models.Components;

namespace ODK.Web.Razor.Tests.Models.Components;

[Parallelizable]
public static class PaginationViewModelTests
{
    [Test]
    public static void VisiblePages_FewEnoughPages_ShowsThemAll()
    {
        // Arrange - nothing to leave out, so nothing should be replaced by an ellipsis.
        var model = CreateModel(page: 3, totalPages: 5);

        // Act / Assert
        model.VisiblePages.Should().Equal(1, 2, 3, 4, 5);
    }

    [Test]
    public static void VisiblePages_ManyPages_KeepsTheEndsAndTheCurrentPage()
    {
        // Arrange - the case that widened the component past a phone screen: a hundred pages rendered in
        // full. Bounded to the first, the last, and the current page with one either side.
        var model = CreateModel(page: 50, totalPages: 100);

        // Act / Assert - null is the ellipsis standing in for the pages left out.
        model.VisiblePages.Should().Equal(1, null, 49, 50, 51, null, 100);
    }

    [Test]
    public static void VisiblePages_GapOfOnePage_ShowsThePageRatherThanAnEllipsis()
    {
        // Arrange - page 3 of 100 leaves only page 2 out between the first page and the window. An ellipsis
        // there would be no shorter than the number it hides, and would hide a page one click away.
        var model = CreateModel(page: 3, totalPages: 100);

        // Act / Assert
        model.VisiblePages.Should().Equal(1, 2, 3, 4, null, 100);
    }

    [Test]
    public static void VisiblePages_OnTheFirstPage_DoesNotRepeatIt()
    {
        // Arrange - the window overlaps the first page, which is always included in its own right.
        var model = CreateModel(page: 1, totalPages: 100);

        // Act / Assert
        model.VisiblePages.Should().Equal(1, 2, null, 100);
    }

    [Test]
    public static void VisiblePages_OnTheLastPage_DoesNotRepeatIt()
    {
        // Arrange
        var model = CreateModel(page: 100, totalPages: 100);

        // Act / Assert
        model.VisiblePages.Should().Equal(1, null, 99, 100);
    }

    [Test]
    public static void VisiblePages_SinglePage_ShowsJustIt()
    {
        // Arrange - the first and the last page are the same one, so it must not appear twice.
        var model = CreateModel(page: 1, totalPages: 1);

        // Act / Assert
        model.VisiblePages.Should().Equal(1);
    }

    [Test]
    public static void VisiblePages_NoPages_IsEmpty()
    {
        // Arrange - an empty list has nothing to page through.
        var model = CreateModel(page: 1, totalPages: 0);

        // Act / Assert
        model.VisiblePages.Should().BeEmpty();
    }

    private static PaginationViewModel CreateModel(int page, int totalPages) => new()
    {
        AccessibilityLabel = "Pages",
        GetPageUrl = x => $"?page={x}",
        Page = page,
        TotalPages = totalPages
    };
}
