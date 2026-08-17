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
        model.VisiblePages.Should().Equal(1, 2, 3, 4, null, 100);
    }

    [Test]
    public static void VisiblePages_OnTheLastPage_DoesNotRepeatIt()
    {
        // Arrange
        var model = CreateModel(page: 100, totalPages: 100);

        // Act / Assert
        model.VisiblePages.Should().Equal(1, null, 97, 98, 99, 100);
    }

    [TestCase(1)]
    [TestCase(2)]
    [TestCase(8)]
    [TestCase(15)]
    [TestCase(16)]
    public static void VisiblePages_WhereverTheCurrentPageSits_OffersTheSameNumberOfPages(int page)
    {
        /* Arrange - the first page used to offer [1] [2] ... [16] against the [1] ... [7] [8] [9] ... [16] of
           the middle, because the half of the window that fell outside the list was simply lost. An end has
           nowhere to put that half, so it goes on the other side instead. */
        var model = CreateModel(page, totalPages: 16);

        // Act
        var numbered = model.VisiblePages.Where(x => x != null).ToArray();

        // Assert - the ellipses vary with where the run sits; the number of pages to click does not.
        numbered.Should().HaveCount(5);
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
