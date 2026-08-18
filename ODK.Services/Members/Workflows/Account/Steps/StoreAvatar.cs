using ODK.Core.Members;
using ODK.Core.Workflows;
using ODK.Data.Core;

namespace ODK.Services.Members.Workflows.Account.Steps;

/// <summary>
/// Resizes the submitted picture and stores it. The image was checked before anything was written, so a
/// failure here is a fault rather than a rejected submission.
/// </summary>
public sealed class StoreAvatar : IStep<AccountContext>
{
    private readonly IMemberImageService _memberImageService;
    private readonly IUnitOfWork _unitOfWork;

    public StoreAvatar(IUnitOfWork unitOfWork, IMemberImageService memberImageService)
    {
        _memberImageService = memberImageService;
        _unitOfWork = unitOfWork;
    }

    public static string Description => "stores the member's picture";

    public static StepKind Kind => StepKind.Write;

    public Task<StepOutcome> Execute(AccountContext context, CancellationToken cancellationToken)
    {
        var avatar = new MemberAvatar();

        var result = _memberImageService.UpdateMemberImage(avatar, context.RequiredProfile.ImageData);
        if (!result.Success)
        {
            return Task.FromResult(StepOutcome.Fail(result.Message ?? "Image could not be processed"));
        }

        avatar.MemberId = context.RequiredNewMember.Id;
        _unitOfWork.MemberAvatarRepository.Add(avatar);

        return Task.FromResult(StepOutcome.Continue());
    }
}
