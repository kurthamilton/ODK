using ODK.Core.Workflows;
using ODK.Services.Notifications;

namespace ODK.Services.Members.Workflows.Account.Steps;

/// <summary>
/// Raises the in-app notification telling the group's admins somebody has joined. Before the commit, with
/// the activation itself - a notification and the membership it announces go in together.
/// </summary>
public sealed class NotifyGroupOfNewMember : IStep<AccountContext>
{
    private readonly INotificationService _notificationService;

    public NotifyGroupOfNewMember(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    public static string Description => "notifies the group of a new member";

    public static StepKind Kind => StepKind.Write;

    public Task<StepOutcome> Execute(AccountContext context, CancellationToken cancellationToken)
    {
        _notificationService.AddNewMemberNotifications(
            context.RequiredMember,
            context.RequiredChapter.Id,
            context.AdminMembers,
            context.NotificationSettings);

        return Task.FromResult(StepOutcome.Continue());
    }
}
