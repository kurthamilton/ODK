using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using ODK.Core.Extensions;

namespace ODK.Core.Tests.Extensions;

[Parallelizable]
public static class DictionaryExtensionsTests
{
    [Test]
    public static void WithComparer()
    {
        // Arrange
        var source = new Dictionary<string, string>(StringComparer.InvariantCulture)
        {
            { "a", "A" },
            { "b", "B" },
            { "c", "C" }
        };

        // Act
        var result = ((IDictionary<string, string>)source)
            .WithComparer(StringComparer.InvariantCultureIgnoreCase);

        // Assert
        foreach (var key in source.Keys)
        {
            var upperKey = key.ToUpperInvariant();

            source.ContainsKey(upperKey).Should().BeFalse();
            result.ContainsKey(upperKey).Should().BeTrue();
        }
    }
}
