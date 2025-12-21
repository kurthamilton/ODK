namespace ODK.Services.Subscriptions.ViewModels;

public class SiteSubscriptionSuperAdminListItemViewModel
{
    public required bool Default { get; init; }

    public required bool Enabled { get; init; }

    public required int? GroupLimit { get; init; }

    public required Guid Id { get; init; }

    public required int? MemberLimit { get; init; }

    public required bool MemberSubscriptions { get; init; }

    public required string Name { get; init; }

    public required string PaymentSettingsName { get; init; }

    public required bool Premium { get; init; }

    public required int PriceCount { get; init; }

    public required bool SendMemberEmails { get; init; }
}
