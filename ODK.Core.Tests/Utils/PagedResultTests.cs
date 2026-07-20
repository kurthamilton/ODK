using FluentAssertions;
using NUnit.Framework;
using ODK.Core.Utils;

namespace ODK.Core.Tests.Utils;

[Parallelizable]
public static class PagedResultTests
{
    [TestCase(0, 20, ExpectedResult = 0)]
    [TestCase(1, 20, ExpectedResult = 1)]
    [TestCase(20, 20, ExpectedResult = 1)]
    [TestCase(21, 20, ExpectedResult = 2)]
    [TestCase(40, 20, ExpectedResult = 2)]
    [TestCase(41, 20, ExpectedResult = 3)]
    public static int TotalPages_ReturnsCeilingOfCountOverPageSize(int totalCount, int pageSize)
    {
        var result = new PagedResult<int>
        {
            Items = [],
            Page = 1,
            PageSize = pageSize,
            TotalCount = totalCount
        };

        return result.TotalPages;
    }

    [Test]
    public static void TotalPages_ZeroPageSize_ReturnsZero()
    {
        var result = new PagedResult<int>
        {
            Items = [],
            Page = 1,
            PageSize = 0,
            TotalCount = 10
        };

        result.TotalPages.Should().Be(0);
    }
}
