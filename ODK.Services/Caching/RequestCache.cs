using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Services.Chapters;
using ODK.Services.Members;

namespace ODK.Services.Caching
{
    public class RequestCache : IRequestCache
    {
        private readonly IChapterService _chapterService;
        private readonly IMemberService _memberService;

        private readonly IDictionary<string, Chapter> _chapters = new Dictionary<string, Chapter>();
        private readonly IDictionary<Guid, Member> _members = new Dictionary<Guid, Member>();

        public RequestCache(IChapterService chapterService, IMemberService memberService)
        {
            _chapterService = chapterService;
            _memberService = memberService;
        }

        public async Task<Chapter?> GetChapter(Guid chapterId)
        {
            IReadOnlyCollection<Chapter> chapters = await GetChapters();
            return chapters
                .FirstOrDefault(x => x.Id == chapterId);
        }

        public async Task<Chapter?> GetChapter(string name)
        {
            IReadOnlyCollection<Chapter> chapters = await GetChapters();
            return chapters
                .FirstOrDefault(x => string.Equals(x.Name, name, StringComparison.InvariantCultureIgnoreCase));
        }

        public async Task<IReadOnlyCollection<Chapter>> GetChapters()
        {
            if (_chapters.Count > 0)
            {
                return _chapters.Values.ToArray();
            }

            IReadOnlyCollection<Chapter> chapters = await _chapterService.GetChapters();
            foreach (Chapter chapter in chapters)
            {
                _chapters[chapter.Name] = chapter;
            }

            return _chapters.Values.ToArray();
        }

        public async Task<Member?> GetMember(Guid memberId)
        {
            if (_members.TryGetValue(memberId, out Member? member))
            {
                return member;
            }

            member = await _memberService.GetMember(memberId);
            
            if (member != null)
            {
                _members[memberId] = member;
            }
            
            return member;
        }
    }
}
