using ODK.Core.Members;
using ODK.Core.Workflows;
using ODK.Data.Core;

namespace ODK.Services.Members.Workflows.Account.Steps;

/// <summary>
/// Re-raises the invitations the discarded account held, each keeping its own token so a link already emailed
/// still works - the same reason its activation token is reused. The group being joined is left out: the
/// membership is now the record that they joined, so re-raising it would list them as invited to a group they
/// are in.
/// </summary>
/// <remarks>
/// The original CreatedUtc is not recoverable and does not matter: nothing reads the date, and an invitation's
/// job is to say the member was asked to join, not when.
/// </remarks>
public sealed class CarryOverInvitations : IStep<AccountContext>
{
    private readonly IUnitOfWork _unitOfWork;

    public CarryOverInvitations(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public static string Description => "re-raises invitations to other groups";

    public static StepKind Kind => StepKind.Write;

    public Task<StepOutcome> Execute(AccountContext context, CancellationToken cancellationToken)
    {
        /* A sign-up to the site joins nothing, so every invitation carries over. A group sign-up leaves out
           the group it just joined. */
        var joined = context.Chapter?.Id;

        foreach (var invite in context.CarriedOverInvites.Where(x => x.ChapterId != joined))
        {
            _unitOfWork.MemberChapterInviteRepository.Add(new MemberChapterInvite
            {
                ChapterId = invite.ChapterId,
                CreatedUtc = DateTime.UtcNow,
                MemberId = context.RequiredNewMember.Id,
                Token = invite.Token
            });
        }

        return Task.FromResult(StepOutcome.Continue());
    }
}
