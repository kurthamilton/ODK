using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.Extensions.DependencyInjection;
using ODK.Services.Authentication;

namespace ODK.Web.Razor.Attributes;

/// <summary>
/// Validates a password against the configured <see cref="IPasswordPolicy"/> length limits. The limits
/// are runtime config, so they can't be baked into a <c>[StringLength]</c> constant - this resolves the
/// policy at validation/render time and emits the standard <c>data-val-length</c> client rules. Null/empty
/// passes (presence is <see cref="RequiredAttribute"/>'s job); the service layer remains the authoritative
/// server-side guard.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public class PasswordLengthAttribute : ValidationAttribute, IClientModelValidator
{
    public void AddValidation(ClientModelValidationContext context)
    {
        var policy = context.ActionContext.HttpContext.RequestServices.GetRequiredService<IPasswordPolicy>();

        MergeAttribute(context.Attributes, "data-val", "true");
        MergeAttribute(context.Attributes, "data-val-length", Message(policy));
        MergeAttribute(context.Attributes, "data-val-length-min", policy.MinLength.ToString());
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not string password || password.Length == 0)
        {
            return ValidationResult.Success;
        }

        var policy = validationContext.GetRequiredService<IPasswordPolicy>();
        if (password.Length < policy.MinLength)
        {
            return new ValidationResult(Message(policy), [validationContext.MemberName!]);
        }

        return ValidationResult.Success;
    }

    private static string Message(IPasswordPolicy policy)
        => $"Password must be at least {policy.MinLength} characters";

    private static void MergeAttribute(IDictionary<string, string> attributes, string key, string value)
    {
        if (!attributes.ContainsKey(key))
        {
            attributes.Add(key, value);
        }
    }
}
