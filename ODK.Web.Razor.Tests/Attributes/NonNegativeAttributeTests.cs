using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Routing;
using NUnit.Framework;
using ODK.Web.Razor.Attributes;

namespace ODK.Web.Razor.Tests.Attributes;

[Parallelizable]
public static class NonNegativeAttributeTests
{
    [Test]
    public static void AddValidation_AddsClientValidationAttributes()
    {
        var attributes = new Dictionary<string, string>();
        var metadataProvider = new EmptyModelMetadataProvider();
        var context = new ClientModelValidationContext(
            new ActionContext(new DefaultHttpContext(), new RouteData(), new ActionDescriptor()),
            metadataProvider.GetMetadataForType(typeof(decimal)),
            metadataProvider,
            attributes);

        new NonNegativeAttribute().AddValidation(context);

        attributes.Should().ContainKey("data-val");
        attributes["data-val"].Should().Be("true");
        attributes.Should().ContainKey("data-val-nonnegative");
    }

    [Test]
    public static void IsValid_NegativeDecimal_ReturnsFalse()
        => new NonNegativeAttribute().IsValid(-0.01m).Should().BeFalse();

    [Test]
    public static void IsValid_NegativeDouble_ReturnsFalse()
        => new NonNegativeAttribute().IsValid(-1.5d).Should().BeFalse();

    [Test]
    public static void IsValid_NegativeInt_ReturnsFalse()
        => new NonNegativeAttribute().IsValid(-1).Should().BeFalse();

    [TestCase(0)]
    [TestCase(5)]
    public static void IsValid_NonNegativeInt_ReturnsTrue(int value)
        => new NonNegativeAttribute().IsValid(value).Should().BeTrue();

    [Test]
    public static void IsValid_Null_ReturnsTrue()
        => new NonNegativeAttribute().IsValid(null).Should().BeTrue();

    [Test]
    public static void IsValid_ZeroDecimal_ReturnsTrue()
        => new NonNegativeAttribute().IsValid(0m).Should().BeTrue();
}
