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

        var (chapters, ownedChapters, avatarVersion) = await _unitOfWork.Run(
            x => x.ChapterRepository.GetByMemberId(platform, member.Id),
            x => x.ChapterRepository.GetByOwnerId(platform, member.Id),
            x => x.MemberAvatarRepository.GetVersionDtoByMemberId(member.Id));

        var chapterIds = chapters.Select(x => x.Id).ToArray();
        var ownedChapterIds = ownedChapters.Select(x => x.Id).ToArray();

        var (chapterProperties, memberProperties, chapterImages) = await _unitOfWork.Run(
            x => x.ChapterPropertyRepository.GetByChapterIds(chapterIds),
            x => x.MemberPropertyRepository.GetByMemberId(member.Id),
            x => x.ChapterImageRepository.GetVersionDtosByChapterIds(ownedChapterIds));

        var context = new MemberTaskContext
        {
            Chapters = chapters,
            ChapterProperties = chapterProperties,
            ChaptersWithImage = chapterImages.Select(x => x.ChapterId).ToArray(),
            HasAvatar = avatarVersion != null,
            Member = member,
            MemberProperties = memberProperties,
            OwnedChapters = ownedChapters,
            Platform = platform
        };

        return _providers
            .SelectMany(x => x.GetTasks(context))
            .ToArray();
    }
}
