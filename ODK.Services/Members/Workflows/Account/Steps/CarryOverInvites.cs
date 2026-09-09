using ODK.Core.Members;
using ODK.Core.Workflows;
using ODK.Data.Core;

namespace ODK.Services.Members.Workflows.Account.Steps;

/// <summary>
/// Re-raises the invites the discarded account held, each keeping its own token so a link already emailed
/// still works - the same reason its activation token is reused. The group being joined is left out: the
/// membership is now the record that they joined, so re-raising it would list them as invited to a group they
/// are in.
/// </summary>
/// <remarks>
/// The original CreatedUtc carries over: the retention period runs from when the details were received, so
/// re-raising an invite must not restart it. Whether it has been emailed carries over too, since this is the
/// same invite under a new account and a group publishing itself sends the ones it is holding.
/// </remarks>
public sealed class CarryOverInvites : IStep<AccountContext>
{
    private readonly IUnitOfWork _unitOfWork;

    public CarryOverInvites(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public static string Description => "re-raises invites to other groups";

    public static StepKind Kind => StepKind.Write;

    public Task<StepOutcome> Execute(AccountContext context, CancellationToken cancellationToken)
    {
        /* A sign-up to the site joins nothing, so every invite carries over. A group sign-up leaves out
           the group it just joined. */
        var joined = context.Chapter?.Id;

        foreach (var invite in context.CarriedOverInvites.Where(x => x.ChapterId != joined))
        {
            _unitOfWork.MemberChapterInviteRepository.Add(new MemberChapterInvite
            {
                ChapterId = invite.ChapterId,
                CreatedUtc = invite.CreatedUtc,
                MemberId = context.RequiredNewMember.Id,
                SentUtc = invite.SentUtc,
                Token = invite.Token
            });
        }

        return Task.FromResult(StepOutcome.Continue());
    }
}
