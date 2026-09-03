using System.ComponentModel.DataAnnotations;

namespace ODK.Web.Razor.Models.SiteAdmin;

/// <summary>
/// A refund to make through the payment provider.
/// </summary>
public class RefundPaymentFormSubmitViewModel
{
    /// <summary>
    /// What to give back, in the payment's currency. Required of the form, which always offers a figure -
    /// a blank one would otherwise arrive as the null the service reads as a full refund, which is not
    /// what clearing the field means.
    /// </summary>
    [Required]
    public decimal? Amount { get; init; }

    [Required]
    public string Reason { get; init; } = string.Empty;
}
