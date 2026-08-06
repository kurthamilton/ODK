namespace ODK.Web.Razor.Models.Components;

public class StripeCheckoutViewModel
{
    public required string ApiPublicKey { get; init; }

    public required string ClientSecret { get; init; }
}
