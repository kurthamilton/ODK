using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using ODK.Services.Authentication;
using ODK.Web.Razor.Attributes;

namespace ODK.Web.Razor.Tests.Attributes;

[Parallelizable]
public static class PasswordLengthAttributeTests
{
    private const int MinLength = 8;

    [Test]
    public static void AddValidation_EmitsLengthRuleFromPolicy()
    {
        var attributes = new Dictionary<string, string>();
        var metadataProvider = new EmptyModelMetadataProvider();
        var httpContext = new DefaultHttpContext { RequestServices = ServiceProvider() };
        var context = new ClientModelValidationContext(
            new ActionContext(httpContext, new RouteData(), new ActionDescriptor()),
            metadataProvider.GetMetadataForType(typeof(string)),
            metadataProvider,
            attributes);

        new PasswordLengthAttribute().AddValidation(context);

        attributes["data-val"].Should().Be("true");
        attributes.Should().ContainKey("data-val-length");
        attributes["data-val-length-min"].Should().Be(MinLength.ToString());
    }

    [Test]
    public static void GetValidationResult_EmptyString_IsValid()
        => Validate(string.Empty).Should().Be(ValidationResult.Success);

    [Test]
    public static void GetValidationResult_LongEnough_IsValid()
        => Validate(new string('a', MinLength)).Should().Be(ValidationResult.Success);

    [Test]
    public static void GetValidationResult_TooShort_IsInvalid()
    {
        var result = Validate(new string('a', MinLength - 1));

        result.Should().NotBe(ValidationResult.Success);
        result!.ErrorMessage.Should().Contain("at least");
    }

    private static ServiceProvider ServiceProvider()
        => new ServiceCollection()
            .AddSingleton(Mock.Of<IPasswordPolicy>(x => x.MinLength == MinLength))
            .BuildServiceProvider();

    private static ValidationResult? Validate(string? value)
    {
        var validationContext = new ValidationContext(new object(), ServiceProvider(), items: null)
        {
            MemberName = "Password"
        };

        return new PasswordLengthAttribute().GetValidationResult(value, validationContext);
    }
}
