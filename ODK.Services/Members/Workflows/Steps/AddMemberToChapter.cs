using ODK.Core.Features;
using ODK.Core.Members;
using ODK.Core.Workflows;
using ODK.Data.Core;
using ODK.Services.Authorization;
using ODK.Services.Subscriptions;

namespace ODK.Services.Members.Workflows.Steps;

/// <summary>
/// Writes the membership, the answers to the group's questions, and the free or trial subscription record
/// where the group runs memberships. Approval is taken from the context, so which state the member lands in
/// is decided by the transition rather than recomputed here.
/// </summary>
public sealed class AddMemberToChapter : IStep<AccountContext>
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IMemberChapterSubscriptionWriter _memberChapterSubscriptionWriter;
    private readonly IUnitOfWork _unitOfWork;

    public AddMemberToChapter(
        IUnitOfWork unitOfWork,
        IAuthorizationService authorizationService,
        IMemberChapterSubscriptionWriter memberChapterSubscriptionWriter)
    {
        _authorizationService = authorizationService;
        _memberChapterSubscriptionWriter = memberChapterSubscriptionWriter;
        _unitOfWork = unitOfWork;
    }

    public static string Description => "adds the member to the group";

    public static StepKind Kind => StepKind.Write;

    public Task<StepOutcome> Execute(AccountContext context, CancellationToken cancellationToken)
    {
        var member = context.RequiredMember;
        var now = DateTime.UtcNow;

        _unitOfWork.MemberChapterRepository.Add(new MemberChapter
        {
            Approved = !context.ApprovalRequired,
            ChapterId = context.ChapterId,
            CreatedUtc = now,
            MemberId = member.Id
        });

        var membershipSettings = context.MembershipSettings;
        var hasSubscriptions = _authorizationService.ChapterHasAccess(
            context.OwnerSubscriptionFeatures, SiteFeatureType.MemberSubscriptions);
        if (hasSubscriptions && membershipSettings?.Enabled == true)
        {
            var trial = membershipSettings.TrialPeriodMonths > 0;
            _memberChapterSubscriptionWriter.MakeRecordCurrent(
                newRecord: new MemberSubscriptionRecord
                {
                    ChapterId = context.ChapterId,
                    ExpiresUtc = trial ? now.AddMonths(membershipSettings.TrialPeriodMonths) : null,
                    MemberId = member.Id,
                    PurchasedUtc = now,
                    Type = trial ? SubscriptionType.Trial : SubscriptionType.Free
                },
                existingCurrent: null);
        }

        _unitOfWork.MemberPropertyRepository.AddMany(context.MemberProperties);

        return Task.FromResult(StepOutcome.Continue());
    }
}
