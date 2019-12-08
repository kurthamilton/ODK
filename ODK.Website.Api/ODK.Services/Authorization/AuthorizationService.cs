using System;
using System.Threading.Tasks;
using ODK.Core.Members;
using ODK.Services.Caching;
using ODK.Services.Exceptions;

namespace ODK.Services.Authorization
{
    public class AuthorizationService : IAuthorizationService
    {
        private readonly ICacheService _cacheService;
        private readonly IMemberRepository _memberRepository;

        public AuthorizationService(IMemberRepository memberRepository, ICacheService cacheService)
        {
            _cacheService = cacheService;
            _memberRepository = memberRepository;
        }

        public async Task AssertMemberIsChapterMember(Guid memberId, Guid chapterId)
        {
            Member member = await GetMember(memberId);
            AssertMemberIsChapterMember(member, chapterId);
        }

        public void AssertMemberIsChapterMember(Member member, Guid chapterId)
        {
            AssertMemberIsCurrent(member);
            if (member.ChapterId != chapterId)
            {
                throw new OdkNotAuthorizedException();
            }
        }

        public async Task AssertMemberIsCurrent(Guid memberId)
        {
            Member member = await GetMember(memberId);
            AssertMemberIsCurrent(member);
        }

        public void AssertMemberIsCurrent(Member member)
        {
            if (member == null || !member.Activated || member.Disabled)
            {
                throw new OdkNotFoundException();
            }
        }

        private async Task<Member> GetMember(Guid id)
        {
            VersionedServiceResult<Member> member = await _cacheService.GetOrSetVersionedItem(() => _memberRepository.GetMember(id), id, null);
            AssertMemberIsCurrent(member.Value);
            return member.Value;
        }
    }
}
