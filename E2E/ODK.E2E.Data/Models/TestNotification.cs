namespace ODK.E2E.Data.Models;

/// <summary>
/// What a notification says and which group it is about. The chapter is null for one about the site
/// itself, which is the only thing distinguishing a site subscription's notification from a membership's.
/// </summary>
public sealed record TestNotification(string Text, Guid? ChapterId);
