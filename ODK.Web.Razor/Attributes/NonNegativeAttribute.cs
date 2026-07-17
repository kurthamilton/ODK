using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace ODK.Web.Razor.Attributes;

/// <summary>
/// Validates that a numeric value is not negative. Null passes (presence is <see cref="RequiredAttribute"/>'s job).
/// Emits client-side validation via the 'nonnegative' provider registered in odk.forms.js.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public class NonNegativeAttribute : ValidationAttribute, IClientModelValidator
{
    public NonNegativeAttribute()
        : base("The {0} field must not be negative.")
    {
    }

    public override bool IsValid(object? value)
        => value switch
        {
            null => true,
            decimal d => d >= 0,
            double db => db >= 0,
            float f => f >= 0,
            long l => l >= 0,
            int i => i >= 0,
            _ => true
        };

    public void AddValidation(ClientModelValidationContext context)
    {
        MergeAttribute(context.Attributes, "data-val", "true");
        MergeAttribute(
            context.Attributes,
            "data-val-nonnegative",
            FormatErrorMessage(context.ModelMetadata.GetDisplayName()));
    }

    private static void MergeAttribute(IDictionary<string, string> attributes, string key, string value)
    {
        if (!attributes.ContainsKey(key))
        {
            attributes.Add(key, value);
        }
    }
}
