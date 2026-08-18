using ODK.Core.Workflows;
using ODK.Services.Notifications;

namespace ODK.Services.Members.Workflows.Steps;

public sealed class RaiseNewMemberNotifications : IStep<AccountContext>
{
    private readonly INotificationService _notificationService;

    public RaiseNewMemberNotifications(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    public static string Description => "notifies the group's admins in the app";

    public static StepKind Kind => StepKind.Write;

    public Task<StepOutcome> Execute(AccountContext context, CancellationToken cancellationToken)
    {
        _notificationService.AddNewMemberNotifications(
            context.RequiredMember,
            context.ChapterId,
            context.AdminMembers,
            context.NotificationSettings);

        return Task.FromResult(StepOutcome.Continue());
    }
}
