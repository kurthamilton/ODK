using ODK.Core.Members;
using ODK.Core.Workflows;
using ODK.Data.Core;

namespace ODK.Services.Members.Workflows.Account.Steps;

/// <summary>
/// A group sign-up has no location of its own to give, so the member starts where the group is.
/// </summary>
public sealed class AddMemberLocationFromChapter : IStep<AccountContext>
{
    private readonly IUnitOfWork _unitOfWork;

    public AddMemberLocationFromChapter(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public static string Description => "places the member where the group is";

    public static StepKind Kind => StepKind.Write;

    public Task<StepOutcome> Execute(AccountContext context, CancellationToken cancellationToken)
    {
        var chapterLocation = context.ChapterLocation;
        if (chapterLocation != null)
        {
            _unitOfWork.MemberLocationRepository.Add(new MemberLocation
            {
                CountryId = context.RequiredChapter.CountryId,
                Latitude = chapterLocation.Latitude,
                Longitude = chapterLocation.Longitude,
                MemberId = context.RequiredNewMember.Id,
                Name = chapterLocation.Name
            });
        }

        return Task.FromResult(StepOutcome.Continue());
    }
}
