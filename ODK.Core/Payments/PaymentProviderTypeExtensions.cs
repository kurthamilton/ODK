﻿namespace ODK.Core.Payments;

public static class PaymentProviderTypeExtensions
{
    public static bool HasCheckout(this PaymentProviderType provider) => provider switch
    {
        PaymentProviderType.Stripe => true,
        _ => false
    };
}