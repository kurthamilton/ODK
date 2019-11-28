using System;

namespace ODK.Core.Payments
{
    [Obsolete("TODO: Remove")]
    public interface IPayment
    {
        double Amount { get; }

        string ApiPublicKey { get; }

        string ApiSecretKey { get; }

        string CurrencyCode { get; }

        string CurrencyString { get; }

        string Description { get; }

        string Email { get; }

        int Id { get; }

        string Provider { get; }

        string SiteName { get; }

        string Title { get; }
    }
}
