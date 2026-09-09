using ODK.Core.Cryptography;
using ODK.Core.Members;
using ODK.Core.Workflows;
using ODK.Data.Core;

namespace ODK.Services.Members.Workflows.ChapterMembership.Steps;

/// <summary>
/// Asks the member to join. An invite, not a membership: an imported member has no standing in the group
/// until they accept, so neither a membership row nor a subscription record is written here - which also means
/// a trial period starts when they join rather than when the file was uploaded.
/// </summary>
/// <remarks>
/// <para>
/// The token is what makes the invite usable by someone who cannot sign in yet, which on Drunken Knitwits
/// is everyone it is sent to.
/// </para>
/// <para>
/// Raised unsent, and left on the context for the caller to send: an unpublished group holds its invites
/// until it is published, so whether this one is emailed is not this step's to decide.
/// </para>
/// <para>
/// Dated from when the details were received rather than from now, so the retention period covers the
/// holding as a whole - see <see cref="ChapterMembershipContext.ReceivedUtc"/>.
/// </para>
/// </remarks>
public sealed class RaiseInvite : IStep<ChapterMembershipContext>
{
    private readonly IUnitOfWork _unitOfWork;

    public RaiseInvite(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public static string Description => "asks the member to join";

    public static StepKind Kind => StepKind.Write;

    public Task<StepOutcome> Execute(ChapterMembershipContext context, CancellationToken cancellationToken)
    {
        var invite = new MemberChapterInvite
        {
            ChapterId = context.ChapterId,
            CreatedUtc = context.RequiredReceivedUtc,
            MemberId = context.Member.Id,
            Token = TokenGenerator.GenerateBase64Token(64)
        };

        _unitOfWork.MemberChapterInviteRepository.Add(invite);

        context.RaisedInvite = invite;

        return Task.FromResult(StepOutcome.Continue());
    }
}
