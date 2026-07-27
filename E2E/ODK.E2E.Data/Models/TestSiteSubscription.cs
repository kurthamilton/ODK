namespace ODK.E2E.Data.Models;

/// <summary>
/// A provisioned, purchasable site subscription. <see cref="PriceId"/> drives the checkout URL
/// (<c>/account/subscription/{priceId}/checkout</c>); <see cref="Id"/> drives DB assertions.
/// </summary>
public sealed record TestSiteSubscription(Guid Id, Guid PriceId, string Name);
