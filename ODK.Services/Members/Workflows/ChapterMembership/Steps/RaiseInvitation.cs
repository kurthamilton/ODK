using ODK.Core.Cryptography;
using ODK.Core.Members;
using ODK.Core.Workflows;
using ODK.Data.Core;

namespace ODK.Services.Members.Workflows.ChapterMembership.Steps;

/// <summary>
/// Asks the member to join. An invitation, not a membership: an imported member has no standing in the group
/// until they accept, so neither a membership row nor a subscription record is written here - which also means
/// a trial period starts when they join rather than when the file was uploaded.
/// </summary>
/// <remarks>
/// The token is what makes the invitation usable by someone who cannot sign in yet, which on Drunken Knitwits
/// is everyone it is sent to.
/// </remarks>
public sealed class RaiseInvitation : IStep<ChapterMembershipContext>
{
    private readonly IUnitOfWork _unitOfWork;

    public RaiseInvitation(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public static string Description => "asks the member to join";

    public static StepKind Kind => StepKind.Write;

    public Task<StepOutcome> Execute(ChapterMembershipContext context, CancellationToken cancellationToken)
    {
        _unitOfWork.MemberChapterInviteRepository.Add(new MemberChapterInvite
        {
            ChapterId = context.ChapterId,
            CreatedUtc = DateTime.UtcNow,
            MemberId = context.Member.Id,
            Token = TokenGenerator.GenerateBase64Token(64)
        });

        return Task.FromResult(StepOutcome.Continue());
    }
}
