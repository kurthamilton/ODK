using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ODK.Core.Chapters;
using ODK.Core.Cryptography;
using ODK.Core.Images;
using ODK.Core.Mail;
using ODK.Core.Members;
using ODK.Services.Authentication;
using ODK.Services.Authorization;
using ODK.Services.Exceptions;
using ODK.Services.Mails;

namespace ODK.Services.Members
{
    public class MemberService : IMemberService
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IChapterRepository _chapterRepository;
        private readonly IMailService _mailService;
        private readonly IMemberRepository _memberRepository;
        private readonly AuthenticationSettings _settings;

        public MemberService(IMemberRepository memberRepository, IChapterRepository chapterRepository, IAuthorizationService authorizationService,
            IMailService mailService, AuthenticationSettings settings)
        {
            _authorizationService = authorizationService;
            _chapterRepository = chapterRepository;
            _mailService = mailService;
            _memberRepository = memberRepository;
            _settings = settings;
        }

        public async Task CreateMember(Guid chapterId, CreateMemberProfile profile)
        {
            await ValidateMemberProfile(chapterId, profile);

            Member existing = await _memberRepository.FindMemberByEmailAddress(profile.EmailAddress);
            if (existing != null)
            {
                return;
            }

            Member create = new Member(Guid.Empty, chapterId, profile.EmailAddress, profile.EmailOptIn, profile.FirstName, profile.LastName, DateTime.UtcNow, false, false);
            Guid id = await _memberRepository.CreateMember(create);

            IEnumerable<MemberProperty> memberProperties = profile.Properties
                .Select(x => new MemberProperty(Guid.Empty, id, x.ChapterPropertyId, x.Value));

            await _memberRepository.UpdateMemberProperties(id, memberProperties);

            string activationToken = RandomStringGenerator.Generate(64);

            await _memberRepository.AddActivationToken(new MemberActivationToken(id, activationToken));

            string url = _settings.ActivateAccountUrl.Replace("{token}", activationToken);

            await _mailService.SendMail(create, EmailType.ActivateAccount, new Dictionary<string, string>
            {
                { "url", url }
            });
        }

        public async Task<MemberImage> GetMemberImage(Guid currentMemberId, Guid memberId)
        {
            Member member = await GetMember(currentMemberId, memberId);
            return await _memberRepository.GetMemberImage(member.Id);
        }

        public async Task<MemberProfile> GetMemberProfile(Guid currentMemberId, Guid memberId)
        {
            Member member = await GetMember(currentMemberId, memberId);
            return await GetMemberProfile(member);
        }

        public async Task<IReadOnlyCollection<Member>> GetMembers(Guid currentMemberId, Guid chapterId)
        {
            await _authorizationService.AssertMemberIsChapterMember(currentMemberId, chapterId);
            return await _memberRepository.GetMembers(chapterId);
        }

        public async Task<MemberImage> UpdateMemberImage(Guid id, UpdateMemberImage image)
        {
            await _authorizationService.AssertMemberIsCurrent(id);

            MemberImage update = new MemberImage(id, image.ImageData, image.MimeType);
            AssertFileIsImage(update);
            await _memberRepository.UpdateMemberImage(update);
            return await _memberRepository.GetMemberImage(id);
        }

        public async Task<MemberProfile> UpdateMemberProfile(Guid id, UpdateMemberProfile profile)
        {
            Member member = await GetMember(id);
            await ValidateMemberProfile(member.ChapterId, profile);

            MemberProfile existing = await GetMemberProfile(id);
            UpdateMemberProfile(existing, profile);

            await _memberRepository.UpdateMember(id, existing.EmailAddress, existing.EmailOptIn, existing.FirstName, existing.LastName);
            await _memberRepository.UpdateMemberProperties(id, existing.MemberProperties);

            return existing;
        }

        private static void AssertFileIsImage(MemberImage image)
        {
            if (!ImageValidator.IsValidMimeType(image.MimeType) || !ImageValidator.IsValidData(image.ImageData))
            {
                throw new OdkServiceException("File is not a valid image");
            }
        }

        private static IEnumerable<string> GetMissingMemberProfileProperties(CreateMemberProfile profile, IEnumerable<ChapterProperty> chapterProperties,
            IEnumerable<UpdateMemberProperty> memberProperties)
        {
            if (string.IsNullOrWhiteSpace(profile.EmailAddress))
            {
                yield return "Email address";
            }

            foreach (string property in GetMissingMemberProfileProperties(profile as UpdateMemberProfile, chapterProperties, memberProperties))
            {
                yield return property;
            }
        }

        private static IEnumerable<string> GetMissingMemberProfileProperties(UpdateMemberProfile profile, IEnumerable<ChapterProperty> chapterProperties,
            IEnumerable<UpdateMemberProperty> memberProperties)
        {
            if (string.IsNullOrWhiteSpace(profile.FirstName))
            {
                yield return "First name";
            }

            if (string.IsNullOrWhiteSpace(profile.LastName))
            {
                yield return "First name";
            }

            IDictionary<Guid, string> memberPropertyDictionary = memberProperties.ToDictionary(x => x.ChapterPropertyId, x => x.Value);
            foreach (ChapterProperty chapterProperty in chapterProperties.Where(x => x.Required))
            {
                string value = memberPropertyDictionary.ContainsKey(chapterProperty.Id)
                    ? memberPropertyDictionary[chapterProperty.Id]
                    : null;

                if (string.IsNullOrWhiteSpace(value))
                {
                    yield return chapterProperty.Name;
                }
            }
        }

        private static void UpdateMemberProfile(MemberProfile existing, UpdateMemberProfile update)
        {
            existing.Update(update.EmailOptIn, update.FirstName, update.LastName);

            foreach (MemberProperty memberProperty in existing.MemberProperties)
            {
                UpdateMemberProperty updateProperty = update.Properties?.FirstOrDefault(x => x.ChapterPropertyId == memberProperty.ChapterPropertyId);
                if (updateProperty == null)
                {
                    continue;
                }

                string value = updateProperty.Value;
                memberProperty.Update(value);
            }
        }

        private async Task<Member> GetMember(Guid currentMemberId, Guid memberId)
        {
            Member member = await _memberRepository.GetMember(memberId);
            await _authorizationService.AssertMemberIsChapterMember(currentMemberId, member.ChapterId);
            return member;
        }

        private async Task<MemberProfile> GetMemberProfile(Guid memberId)
        {
            return await GetMemberProfile(memberId, memberId);
        }

        private async Task<MemberProfile> GetMemberProfile(Member member)
        {
            IReadOnlyCollection<ChapterProperty> chapterProperties = await _chapterRepository.GetChapterProperties(member.ChapterId);
            IReadOnlyCollection<MemberProperty> memberProperties = await _memberRepository.GetMemberProperties(member.Id);
            IDictionary<Guid, MemberProperty> memberPropertyDictionary = memberProperties.ToDictionary(x => x.ChapterPropertyId, x => x);

            IEnumerable<MemberProperty> allMemberProperties = chapterProperties.Select(x =>
                memberPropertyDictionary.ContainsKey(x.Id) ? memberPropertyDictionary[x.Id] : new MemberProperty(Guid.Empty, member.Id, x.Id, null));

            return new MemberProfile(member, allMemberProperties);
        }

        private async Task<Member> GetMember(Guid memberId)
        {
            return await GetMember(memberId, memberId);
        }

        private async Task ValidateMemberProfile(Guid chapterId, UpdateMemberProfile profile)
        {
            IReadOnlyCollection<ChapterProperty> chapterProperties = await _chapterRepository.GetChapterProperties(chapterId);
            IReadOnlyCollection<string> missingProperties = GetMissingMemberProfileProperties(profile, chapterProperties, profile.Properties).ToArray();

            if (missingProperties.Count > 0)
            {
                throw new OdkServiceException($"The following properties are required: {string.Join(", ", missingProperties)}");
            }
        }

        private async Task ValidateMemberProfile(Guid chapterId, CreateMemberProfile profile)
        {
            IReadOnlyCollection<ChapterProperty> chapterProperties = await _chapterRepository.GetChapterProperties(chapterId);
            IReadOnlyCollection<string> missingProperties = GetMissingMemberProfileProperties(profile, chapterProperties, profile.Properties).ToArray();

            if (missingProperties.Count > 0)
            {
                throw new OdkServiceException($"The following properties are required: {string.Join(", ", missingProperties)}");
            }
        }
    }
}
