namespace ODK.Web.Razor.Models.Account;

public class PurchaseSubscriptionRequest
{
    public Guid SubscriptionId { get; set; }

    public string Token { get; set; } = string.Empty;
}
