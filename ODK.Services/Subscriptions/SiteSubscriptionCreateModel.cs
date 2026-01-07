namespace ODK.Services.Subscriptions;

public class SiteSubscriptionCreateModel
{
    public required string Description { get; init; } = string.Empty;

    public required bool Enabled { get; init; }

    public required Guid? FallbackSiteSubscriptionId { get; init; }

    public required int? GroupLimit { get; init; }

    public required int? MemberLimit { get; init; }

    public required bool MemberSubscriptions { get; init; }

    public required string Name { get; init; } = string.Empty;

    public required bool Premium { get; init; }

    public required bool SendMemberEmails { get; init; }

    public required Guid SitePaymentSettingId { get; init; }
}
