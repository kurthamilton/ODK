using ODK.Core.Workflows;
using ODK.Services.Notifications;

namespace ODK.Services.Members.Workflows.ChapterMembership.Steps;

public sealed class RaiseNewMemberNotifications : IStep<ChapterMembershipContext>
{
    private readonly INotificationService _notificationService;

    public RaiseNewMemberNotifications(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    public static string Description => "notifies the group's admins in the app";

    public static StepKind Kind => StepKind.Write;

    public Task<StepOutcome> Execute(ChapterMembershipContext context, CancellationToken cancellationToken)
    {
        _notificationService.AddNewMemberNotifications(
            context.Member,
            context.ChapterId,
            context.AdminMembers,
            context.NotificationSettings);

        return Task.FromResult(StepOutcome.Continue());
    }
}
