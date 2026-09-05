namespace ODK.E2E.Data;

/// <summary>
/// The app's <c>NotificationType</c> numbers, which are what its <c>NotificationTypeId</c> columns store.
/// Repeated here rather than referenced because these tests deliberately do not depend on the app's
/// projects; the numbers are safe to repeat because they are a persisted contract, which is why that enum
/// assigns them explicitly rather than letting them fall where they may.
/// </summary>
public static class NotificationTypeIds
{
    public const int None = 0;

    public const int SubscriptionRenewed = 9;
}
