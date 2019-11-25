using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ODK.Core.Chapters;
using ODK.Core.Images;
using ODK.Core.Members;
using ODK.Services.Exceptions;

namespace ODK.Services.Members
{
    public class MemberService : IMemberService
    {
        private readonly IChapterRepository _chapterRepository;
        private readonly IMemberRepository _memberRepository;

        public MemberService(IMemberRepository memberRepository, IChapterRepository chapterRepository)
        {
            _chapterRepository = chapterRepository;
            _memberRepository = memberRepository;
        }

        public async Task<Member> GetMember(Guid currentMemberId, Guid memberId)
        {
            Member member = await _memberRepository.GetMember(memberId);
            await AssertMemberPermission(currentMemberId, member);
            return member;
        }

        public async Task<MemberImage> GetMemberImage(Guid currentMemberId, Guid memberId)
        {
            await AssertMemberPermission(currentMemberId, memberId);
            MemberImage image = await _memberRepository.GetMemberImage(memberId);
            return image;
        }

        public async Task<MemberProfile> GetMemberProfile(Guid currentMemberId, Guid memberId)
        {
            Member member = await GetMember(currentMemberId, memberId);

            IReadOnlyCollection<ChapterProperty> chapterProperties = await _chapterRepository.GetChapterProperties(member.ChapterId);
            IReadOnlyCollection<MemberProperty> memberProperties = await _memberRepository.GetMemberProperties(member.Id);
            IDictionary<Guid, MemberProperty> memberPropertyDictionary = memberProperties.ToDictionary(x => x.ChapterPropertyId, x => x);

            IEnumerable<MemberProperty> allMemberProperties = chapterProperties.Select(x =>
                memberPropertyDictionary.ContainsKey(x.Id) ? memberPropertyDictionary[x.Id] : new MemberProperty(Guid.Empty, member.Id, x.Id, null));

            return new MemberProfile(member, allMemberProperties);
        }

        public async Task<IReadOnlyCollection<Member>> GetMembers(Guid currentMemberId, Guid chapterId)
        {
            IReadOnlyCollection<Member> members = await _memberRepository.GetMembers(chapterId);
            if (members.Any(x => x.Id == currentMemberId))
            {
                return members;
            }

            Member currentMember = await _memberRepository.GetMember(currentMemberId);
            AssertMemberChapterPermission(currentMember, chapterId);
            return members;
        }

        public async Task UpdateMemberImage(MemberImage image)
        {
            AssertFileIsImage(image);
            await _memberRepository.UpdateMemberImage(image);
        }

        public async Task UpdateMemberProfile(UpdateMemberProfile profile)
        {
            MemberProfile existing = await GetMemberProfile(profile.Id, profile.Id);

            existing.Update(profile.EmailAddress, profile.EmailOptIn, profile.FirstName, profile.LastName);

            foreach (MemberProperty memberProperty in existing.MemberProperties)
            {
                UpdateMemberProperty updateProperty = profile.Properties?.FirstOrDefault(x => x.ChapterPropertyId == memberProperty.ChapterPropertyId);
                if (updateProperty == null)
                {
                    continue;
                }

                string value = updateProperty.Value;
                memberProperty.Update(value);
            }

            await ValidateMemberProfile(profile.Id, existing);

            await _memberRepository.UpdateMember(profile.Id, existing.EmailAddress, existing.EmailOptIn, existing.FirstName, existing.LastName);
            await _memberRepository.UpdateMemberProperties(profile.Id, existing.MemberProperties);
        }

        private static void AssertFileIsImage(MemberImage image)
        {
            if (!ImageValidator.IsValidMimeType(image.MimeType) || !ImageValidator.IsValidData(image.ImageData))
            {
                throw new OdkServiceException("File is not a valid image");
            }
        }

        private static void AssertMemberChapterPermission(Member currentMember, Guid chapterId)
        {
            if (currentMember?.ChapterId != chapterId)
            {
                throw new OdkNotAuthorizedException();
            }
        }

        private async Task AssertMemberPermission(Guid currentMemberId, Guid memberId)
        {
            Member member = await _memberRepository.GetMember(memberId);
            await AssertMemberPermission(currentMemberId, member);
        }

        private async Task AssertMemberPermission(Guid currentMemberId, Member member)
        {
            if (member == null)
            {
                throw new OdkNotFoundException();
            }

            if (currentMemberId == member.Id)
            {
                return;
            }

            Member currentMember = await _memberRepository.GetMember(currentMemberId);
            AssertMemberChapterPermission(currentMember, member.ChapterId);
        }

        private async Task ValidateMemberProfile(Guid memberId, MemberProfile profile)
        {
            Member member = await GetMember(memberId, memberId);
            IReadOnlyCollection<ChapterProperty> chapterProperties = await _chapterRepository.GetChapterProperties(member.ChapterId);
            IReadOnlyCollection<MemberProperty> requiredProperties = chapterProperties
                .Where(x => x.Required)
                .Select(chapterProperty => profile.MemberProperties.FirstOrDefault(x => x.ChapterPropertyId == chapterProperty.Id))
                .ToArray();

            if (string.IsNullOrWhiteSpace(profile.EmailAddress) ||
                string.IsNullOrWhiteSpace(profile.FirstName) ||
                string.IsNullOrWhiteSpace(profile.LastName) ||
                requiredProperties.Any(x => string.IsNullOrWhiteSpace(x?.Value)))
            {
                throw new OdkServiceException("Some required properties do not have values");
            }
        }
    }
}
