using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ODK.Core.Chapters;
using ODK.Core.Members;
using ODK.Services.Caching;
using ODK.Services.Exceptions;
using ODK.Services.Files;

namespace ODK.Services.Members
{
    public class MemberAdminService : OdkAdminServiceBase, IMemberAdminService
    {
        private readonly ICacheService _cacheService;
        private readonly IMemberRepository _memberRepository;
        private readonly IMemberService _memberService;

        public MemberAdminService(IChapterRepository chapterRepository, IMemberRepository memberRepository,
            IMemberService memberService, ICacheService cacheService)
            : base(chapterRepository)
        {
            _cacheService = cacheService;
            _memberRepository = memberRepository;
            _memberService = memberService;
        }

        public async Task DeleteMember(Guid currentMemberId, Guid memberId)
        {
            Member member = await GetMember(currentMemberId, memberId);

            await _memberRepository.DeleteMember(member.Id);

            _cacheService.RemoveVersionedCollection<Member>(member.ChapterId);
            _cacheService.RemoveVersionedItem<Member>(memberId);
        }

        public async Task DisableMember(Guid currentMemberId, Guid id)
        {
            Member member = await GetMember(currentMemberId, id);

            await _memberRepository.DisableMember(member.Id);
        }

        public async Task EnableMember(Guid currentMemberId, Guid id)
        {
            Member member = await GetMember(currentMemberId, id);

            await _memberRepository.EnableMember(member.Id);
        }

        public async Task<Member> GetMember(Guid currentMemberId, Guid memberId)
        {
            return await GetMember(currentMemberId, memberId, false);
        }

        public async Task<CsvFile> GetMemberImportFile(Guid currentMemberId, Guid chapterId)
        {
            await AssertMemberIsChapterAdmin(currentMemberId, chapterId);

            IReadOnlyCollection<ChapterProperty> properties = await ChapterRepository.GetChapterProperties(chapterId);

            MemberCsvFileHeaders headers = new MemberCsvFileHeaders(properties);

            CsvFile file = new CsvFile();
            file.Header.AddValues(headers.ToFields());
            return file;
        }

        public async Task<IReadOnlyCollection<Member>> GetMembers(Guid currentMemberId, Guid chapterId)
        {
            await AssertMemberIsChapterAdmin(currentMemberId, chapterId);

            return await _memberRepository.GetMembers(chapterId, true);
        }

        public async Task<MemberSubscription> GetMemberSubscription(Guid currentMemberId, Guid memberId)
        {
            Member member = await GetMember(currentMemberId, memberId);

            return await _memberRepository.GetMemberSubscription(member.Id);
        }

        public async Task<IReadOnlyCollection<MemberSubscription>> GetMemberSubscriptions(Guid currentMemberId, Guid chapterId)
        {
            await AssertMemberIsChapterAdmin(currentMemberId, chapterId);

            return await _memberRepository.GetMemberSubscriptions(chapterId);
        }

        public async Task ImportMembers(Guid currentMemberId, Guid chapterId, CsvFile file)
        {
            await AssertMemberIsChapterAdmin(currentMemberId, chapterId);

            IReadOnlyCollection<ChapterProperty> chapterProperties = await ChapterRepository.GetChapterProperties(chapterId);

            MemberCsvFileHeaders headers = new MemberCsvFileHeaders(chapterProperties);

            IDictionary<string, int> headerIndexes = file.GetColumnIndexes();

            IDictionary<int, Member> members = new Dictionary<int, Member>();

            IDictionary<int, MemberSubscription> membersSubscriptions = new Dictionary<int, MemberSubscription>();

            IDictionary<int, IReadOnlyCollection<MemberProperty>> membersProperties =
                new Dictionary<int, IReadOnlyCollection<MemberProperty>>();

            for (int i = 0; i < file.Rows.Count; i++)
            {
                CsvRow row = file.Rows.ElementAt(i);

                members.Add(i, new Member(Guid.Empty, chapterId,
                    row.Value(headerIndexes[MemberCsvFileHeaders.Email]),
                    true,
                    row.Value(headerIndexes[MemberCsvFileHeaders.FirstName]),
                    row.Value(headerIndexes[MemberCsvFileHeaders.LastName]),
                    DateTime.UtcNow,
                    true,
                    false,
                    0));

                if (DateTime.TryParse(row.Value(headerIndexes[MemberCsvFileHeaders.SubscriptionExpiry]), out DateTime subscriptionExpiry))
                {
                    membersSubscriptions.Add(i, new MemberSubscription(Guid.Empty, SubscriptionType.Full, subscriptionExpiry));
                }

                membersProperties.Add(i, chapterProperties
                    .Select(x => new MemberProperty(Guid.Empty, Guid.Empty, x.Id, row.Value(headerIndexes[x.Label])))
                    .ToArray());
            }

            // TODO: validate members

            for (int i = 0; i < file.Rows.Count; i++)
            {
                Guid memberId = await _memberRepository.CreateMember(members[i]);

                await _memberRepository.UpdateMemberProperties(memberId, membersProperties[i]);

                if (membersSubscriptions.ContainsKey(i))
                {
                    MemberSubscription subscription = new MemberSubscription(memberId, membersSubscriptions[i].Type, membersSubscriptions[i].ExpiryDate);
                    await _memberRepository.UpdateMemberSubscription(subscription);
                }
            }
        }

        public async Task<MemberImage> RotateMemberImage(Guid currentMemberId, Guid memberId, int degrees)
        {
            Member member = await GetMember(currentMemberId, memberId);

            MemberImage rotated = await _memberService.RotateMemberImage(member.Id, degrees);

            _cacheService.RemoveVersionedItem<MemberImage>(memberId);

            return rotated;
        }

        public async Task<MemberImage> UpdateMemberImage(Guid currentMemberId, Guid memberId, UpdateMemberImage image)
        {
            Member member = await GetMember(currentMemberId, memberId, true);

            MemberImage updated = await _memberService.UpdateMemberImage(member.Id, image);

            _cacheService.RemoveVersionedItem<MemberImage>(memberId);

            return updated;
        }

        public async Task<MemberSubscription> UpdateMemberSubscription(Guid currentMemberId, Guid memberId,
            UpdateMemberSubscription subscription)
        {
            Member member = await GetMember(currentMemberId, memberId);

            DateTime? expiryDate = subscription.Type == SubscriptionType.Alum ? new DateTime?() : subscription.ExpiryDate;

            MemberSubscription update = new MemberSubscription(member.Id, subscription.Type, expiryDate);

            ValidateMemberSubscription(update);

            await _memberRepository.UpdateMemberSubscription(update);

            _cacheService.RemoveVersionedItem<MemberSubscription>(memberId);
            _cacheService.RemoveVersionedCollection<Member>(member.ChapterId);

            return update;
        }

        private async Task<Member> GetMember(Guid currentMemberId, Guid id, bool superAdmin)
        {
            Member member = await _memberRepository.GetMember(id, true);
            if (member == null)
            {
                throw new OdkNotFoundException();
            }

            if (superAdmin)
            {
                await AssertMemberIsChapterSuperAdmin(currentMemberId, member.ChapterId);
            }
            else
            {
                await AssertMemberIsChapterAdmin(currentMemberId, member.ChapterId);
            }

            return member;
        }

        private void ValidateMemberSubscription(MemberSubscription subscription)
        {
            if (!Enum.IsDefined(typeof(SubscriptionType), subscription.Type) || subscription.Type == SubscriptionType.None)
            {
                throw new OdkServiceException("Invalid type");
            }

            if (subscription.Type == SubscriptionType.Alum && subscription.ExpiryDate != null)
            {
                throw new OdkServiceException("Alum should not have expiry date");
            }
            else if (subscription.Type != SubscriptionType.Alum && subscription.ExpiryDate < DateTime.UtcNow.Date)
            {
                throw new OdkServiceException("Expiry date should not be in the past");
            }
        }
    }
}
