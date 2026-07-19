using ODK.Data.Core;

namespace ODK.Services.Members.Tasks;

public class MemberTaskService : IMemberTaskService
{
    private readonly IReadOnlyCollection<IMemberTaskProvider> _providers;
    private readonly IUnitOfWork _unitOfWork;

    public MemberTaskService(IUnitOfWork unitOfWork, IEnumerable<IMemberTaskProvider> providers)
    {
        _providers = providers.ToArray();
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyCollection<MemberTask>> GetOutstandingTasks(IMemberServiceRequest request)
    {
        var (platform, member) = (request.Platform, request.CurrentMember);

        var (chapters, avatarVersion) = await _unitOfWork.RunAsync(
            x => x.ChapterRepository.GetByMemberId(platform, member.Id),
            x => x.MemberAvatarRepository.GetVersionDtoByMemberId(member.Id));

        var chapterIds = chapters.Select(x => x.Id).ToArray();

        var (chapterProperties, memberProperties) = await _unitOfWork.RunAsync(
            x => x.ChapterPropertyRepository.GetByChapterIds(chapterIds),
            x => x.MemberPropertyRepository.GetByMemberId(member.Id));

        var context = new MemberTaskContext
        {
            Chapters = chapters,
            ChapterProperties = chapterProperties,
            HasAvatar = avatarVersion != null,
            Member = member,
            MemberProperties = memberProperties,
            Platform = platform
        };

        return _providers
            .SelectMany(x => x.GetTasks(context))
            .ToArray();
    }
}
