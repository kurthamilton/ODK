using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Nodes;
using NUnit.Framework;
using ODK.Core.Utils;

namespace ODK.Core.Tests.Utils;

[Parallelizable]
public static class JsonUtilsTests
{
    [TestCase("a", ExpectedResult = "a")]
    [TestCase("b", ExpectedResult = "b")]
    [TestCase("c", ExpectedResult = "c")]
    [TestCase("d", ExpectedResult = null)]
    public static string? Find_Child_ByPropertyName(string propertyName)
    {
        // Arrange
        var json = JsonUtils.Serialize(new
        {
            a = "a",
            b = "b",
            c = "c"
        });

        var node = JsonNode.Parse(json);

        // Act
        var result = JsonUtils.Find(node, x => x.PropertyName == propertyName);

        // Assert
        return result?.GetValue<string>();
    }

    [TestCase("child1", "a", ExpectedResult = "a1")]
    [TestCase("child1", "b", ExpectedResult = "b1")]
    [TestCase("child1", "c", ExpectedResult = "c1")]
    [TestCase("child2", "a", ExpectedResult = "a2")]
    [TestCase("child2", "b", ExpectedResult = "b2")]
    [TestCase("child2", "c", ExpectedResult = "c2")]
    public static string? Find_ChildNestedInObject_ByPropertyName(string parentName, string propertyName)
    {
        // Arrange
        var json = JsonUtils.Serialize(new
        {
            child1 = new
            {
                a = "a1",
                b = "b1",
                c = "c1"
            },
            child2 = new
            {
                a = "a2",
                b = "b2",
                c = "c2"
            }
        });

        var node = JsonNode.Parse(json);

        // Act
        var result = JsonUtils.Find(
            node, 
            x => x.PropertyName == propertyName && x.ParentContext?.PropertyName == parentName);

        // Assert
        return result?.GetValue<string>();
    }
}
