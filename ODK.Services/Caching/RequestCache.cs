using ODK.Core;
using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Core.Platforms;
using ODK.Data.Core;

namespace ODK.Services.Caching;

public class RequestCache : IRequestCache
{
    private readonly IPlatformProvider _platformProvider;
    private readonly IUnitOfWork _unitOfWork;

    private readonly IDictionary<string, Chapter> _chapters;
    private readonly IDictionary<Guid, ChapterMembershipSettings?> _chapterMembershipSettings;
    private readonly IDictionary<Guid, Member> _members;
    private readonly IDictionary<Guid, MemberSubscription?> _memberSubscriptions;

    public RequestCache(IUnitOfWork unitOfWork, IPlatformProvider platformProvider)
    {
        _platformProvider = platformProvider;
        _unitOfWork = unitOfWork;

        _chapters = new Dictionary<string, Chapter>(StringComparer.InvariantCultureIgnoreCase);
        _chapterMembershipSettings = new Dictionary<Guid, ChapterMembershipSettings?>();
        _members = new Dictionary<Guid, Member>();
        _memberSubscriptions = new Dictionary<Guid, MemberSubscription?>();
    }

    public async Task<Chapter> GetChapterAsync(Guid chapterId)
    {
        var chapters = await GetChaptersAsync();
        var chapter = chapters.FirstOrDefault(x => x.Id == chapterId);
        return OdkAssertions.Exists(chapter);
    }

    public async Task<Chapter> GetChapterAsync(string name)
    {
        var chapters = await GetChaptersAsync();
        var chapter = chapters.FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.InvariantCultureIgnoreCase));
        return OdkAssertions.Exists(chapter, $"Chapter not found: '{name}'");
    }

    public async Task<IReadOnlyCollection<Chapter>> GetChaptersAsync()
    {
        if (_chapters.Count > 0)
        {
            return _chapters.Values.ToArray();
        }

        var chapters = await _unitOfWork.ChapterRepository
            .GetAll()
            .Run();

        var platform = _platformProvider.GetPlatform();
        if (platform != PlatformType.Default)
        {
            chapters = chapters
                .Where(x => x.Platform == platform)
                .ToArray();
        }

        foreach (var chapter in chapters)
        {
            _chapters[chapter.Name] = chapter;
        }

        return _chapters.Values.ToArray();
    }

    public async Task<ChapterMembershipSettings?> GetChapterMembershipSettingsAsync(Guid chapterId)
    {
        if (!_chapterMembershipSettings.ContainsKey(chapterId))
        {
            var settings = await _unitOfWork.ChapterMembershipSettingsRepository.GetByChapterId(chapterId).Run();
            _chapterMembershipSettings[chapterId] = settings;
        }

        return _chapterMembershipSettings[chapterId];
    }

    public async Task<Member?> GetMemberAsync(Guid memberId)
    {
        if (_members.TryGetValue(memberId, out Member? member))
        {
            return member;
        }

        member = await _unitOfWork.MemberRepository.GetByIdOrDefault(memberId).Run();        
        if (member != null)
        {
            _members[memberId] = member;
        }
        
        return member;
    }

    public async Task<MemberSubscription?> GetMemberSubscriptionAsync(Guid memberId, Guid chapterId)
    {
        if (!_memberSubscriptions.ContainsKey(memberId))
        {
            try
            {
                var subscription = await _unitOfWork.MemberSubscriptionRepository.GetByMemberId(memberId, chapterId).Run();
                _memberSubscriptions[memberId] = subscription;
            }            
            catch
            {
                return null;
            }
        }

        return _memberSubscriptions[memberId];
    }
}
