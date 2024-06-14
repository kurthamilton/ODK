using ODK.Core.Chapters;
using ODK.Core.Countries;

namespace ODK.Web.Razor.Models;

public class PayPalFormViewModel
{
    public PayPalFormViewModel(ChapterPaymentSettings paymentSettings, Country country, 
        double amount, string description)
    {
        Amount = amount;
        Country = country;
        Description = description;
        PaymentSettings = paymentSettings;
    }

    public double Amount { get; }

    public Country Country { get; }

    public string Description { get; }

    public ChapterPaymentSettings PaymentSettings { get; }
}
