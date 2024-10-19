using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using ODK.Core.Chapters;
using ODK.Core.Countries;
using ODK.Core.Members;
using ODK.Core.Payments;

namespace ODK.Web.Razor.Models.Admin.Chapters;

public class ChapterPaymentSettingsFormViewModel
{
    public ChapterPaymentSettings? ChapterPaymentSettings { get; set; }

    [DisplayName("Currency")]
    [Required]
    public Guid? CurrencyId { get; set; }

    public IReadOnlyCollection<Currency> CurrencyOptions { get; set; } = [];

    [DisplayName("Email address")]
    [Required]
    [EmailAddress]
    public string? EmailAddress { get; set; }

    public MemberSiteSubscription? OwnerSubscription { get; set; }

    [Required]
    public PaymentProviderType? Provider { get; set; }

    [DisplayName("Use site payment provider")]
    public bool UseSitePaymentProvider { get; set; }
}
