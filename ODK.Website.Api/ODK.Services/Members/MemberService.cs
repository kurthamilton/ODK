using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using ODK.Core.Chapters;
using ODK.Core.Cryptography;
using ODK.Core.Emails;
using ODK.Core.Images;
using ODK.Core.Members;
using ODK.Core.Utils;
using ODK.Services.Authorization;
using ODK.Services.Caching;
using ODK.Services.Emails;
using ODK.Services.Exceptions;
using ODK.Services.Imaging;
using ODK.Services.Payments;

namespace ODK.Services.Members
{
    public class MemberService : IMemberService
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly ICacheService _cacheService;
        private readonly IChapterRepository _chapterRepository;
        private readonly IEmailService _emailService;
        private readonly IImageService _imageService;
        private readonly IMemberRepository _memberRepository;
        private readonly IPaymentService _paymentService;
        private readonly MemberServiceSettings _settings;

        public MemberService(IMemberRepository memberRepository, IChapterRepository chapterRepository, IAuthorizationService authorizationService,
            IEmailService emailService, MemberServiceSettings settings, IImageService imageService, IPaymentService paymentService,
            ICacheService cacheService)
        {
            _authorizationService = authorizationService;
            _cacheService = cacheService;
            _chapterRepository = chapterRepository;
            _emailService = emailService;
            _imageService = imageService;
            _memberRepository = memberRepository;
            _paymentService = paymentService;
            _settings = settings;
        }

        public async Task ConfirmEmailAddressUpdate(Guid memberId, string confirmationToken)
        {
            Member member = await GetMember(memberId, memberId);

            MemberEmailAddressUpdateToken token = await _memberRepository.GetMemberEmailAddressUpdateToken(member.Id);
            if (token == null)
            {
                return;
            }

            if (token.ConfirmationToken != confirmationToken)
            {
                throw new OdkServiceException("Token mismatch");
            }

            Member existing = await _memberRepository.FindMemberByEmailAddress(token.NewEmailAddress);
            if (existing != null)
            {
                await _memberRepository.DeleteEmailAddressUpdateToken(member.Id);
                throw new OdkServiceException("Error updating email address: email address is already registered to an account");
            }

            await _memberRepository.UpdateMemberEmailAddress(member.Id, token.NewEmailAddress);
            _cacheService.RemoveVersionedItem<Member>(member.Id);

            await _memberRepository.DeleteEmailAddressUpdateToken(member.Id);
        }

        public async Task CreateMember(Guid chapterId, CreateMemberProfile profile)
        {
            await ValidateMemberProfile(chapterId, profile);
            ValidateMemberImage(profile.Image.MimeType, profile.Image.ImageData);

            Member existing = await _memberRepository.FindMemberByEmailAddress(profile.EmailAddress);
            if (existing != null)
            {
                return;
            }

            Member create = new Member(Guid.Empty, chapterId, profile.EmailAddress, profile.EmailOptIn, profile.FirstName, profile.LastName,
                DateTime.UtcNow, false, false, 0);

            Guid id = await _memberRepository.CreateMember(create);

            ChapterMembershipSettings membershipSettings = await _chapterRepository.GetChapterMembershipSettings(chapterId);

            MemberSubscription subscription = new MemberSubscription(id, SubscriptionType.Trial, create.CreatedDate.AddMonths(membershipSettings.TrialPeriodMonths));
            await _memberRepository.UpdateMemberSubscription(subscription);

            MemberImage image = CreateMemberImage(id, profile.Image.MimeType, profile.Image.ImageData);
            await _memberRepository.AddMemberImage(image);

            IEnumerable<MemberProperty> memberProperties = profile.Properties
                .Select(x => new MemberProperty(Guid.Empty, id, x.ChapterPropertyId, x.Value));

            await _memberRepository.UpdateMemberProperties(id, memberProperties);

            string activationToken = RandomStringGenerator.Generate(64);

            await _memberRepository.AddActivationToken(new MemberActivationToken(id, activationToken));

            Chapter chapter = await _chapterRepository.GetChapter(chapterId);

            string url = _settings.ActivateAccountUrl.Interpolate(new Dictionary<string, string>
            {
                { "chapter.name", chapter.Name },
                { "token", HttpUtility.UrlEncode(activationToken) }
            });

            await _emailService.SendEmail(chapter, create.EmailAddress, EmailType.ActivateAccount, new Dictionary<string, string>
            {
                { "chapter.name", chapter.Name },
                { "url", url }
            });
        }

        public async Task DeleteMember(Guid memberId)
        {
            Member member = await _memberRepository.GetMember(memberId, true);
            if (member == null)
            {
                return;
            }

            await _memberRepository.DeleteMember(memberId);

            _cacheService.RemoveVersionedItem<Member>(memberId);
            _cacheService.RemoveVersionedCollection<Member>(member.ChapterId);
        }

        public async Task<VersionedServiceResult<IReadOnlyCollection<Member>>> GetLatestMembers(long? currentVersion,
            Guid currentMemberId, Guid chapterId)
        {
            await _authorizationService.AssertMemberIsChapterMember(currentMemberId, chapterId);

            VersionedServiceResult<IReadOnlyCollection<Member>> members = await GetMembers(currentVersion, currentMemberId, chapterId);
            if (members.Value == null)
            {
                return members;
            }

            IReadOnlyCollection<Member> latestMembers = members
                .Value
                .OrderByDescending(x => x.CreatedDate)
                .Take(8)
                .ToArray();

            return new VersionedServiceResult<IReadOnlyCollection<Member>>(members.Version, latestMembers);
        }

        public async Task<VersionedServiceResult<MemberImage>> GetMemberImage(long? currentVersion, Guid memberId, int? size)
        {
            VersionedServiceResult<MemberImage> result = await _cacheService.GetOrSetVersionedItem(
                () => _memberRepository.GetMemberImage(memberId),
                memberId,
                currentVersion);

            if (currentVersion == result.Version)
            {
                return result;
            }

            MemberImage image = result.Value;
            if (size != null)
            {
                byte[] imageData = _imageService.Crop(image.ImageData, size.Value, size.Value);
                image = new MemberImage(image.MemberId, imageData, image.MimeType, image.Version);
            }

            return new VersionedServiceResult<MemberImage>(image.Version, image);
        }

        public async Task<MemberProfile> GetMemberProfile(Guid currentMemberId, Guid memberId)
        {
            Member member = await GetMember(currentMemberId, memberId);
            return await GetMemberProfile(member);
        }

        public async Task<VersionedServiceResult<IReadOnlyCollection<Member>>> GetMembers(long? currentVersion, Guid currentMemberId,
            Guid chapterId)
        {
            await _authorizationService.AssertMemberIsChapterMember(currentMemberId, chapterId);

            return await _cacheService.GetOrSetVersionedCollection(
                () => _memberRepository.GetMembers(chapterId),
                () => _memberRepository.GetMembersVersion(chapterId),
                currentVersion,
                chapterId);
        }

        public async Task<MemberSubscription> GetMemberSubscription(Guid memberId)
        {
            await _authorizationService.AssertMemberIsCurrent(memberId);

            return await _memberRepository.GetMemberSubscription(memberId);
        }

        public async Task<MemberSubscription> PurchaseSubscription(Guid memberId, Guid chapterSubscriptionId, string cardToken)
        {
            ChapterSubscription chapterSubscription = await _chapterRepository.GetChapterSubscription(chapterSubscriptionId);
            if (chapterSubscription == null)
            {
                throw new OdkServiceException("Subscription not found");
            }

            Member member = await GetMember(memberId);
            _authorizationService.AssertMemberIsChapterMember(member, chapterSubscription.ChapterId);

            await _paymentService.MakePayment(member, chapterSubscription.Amount, cardToken, chapterSubscription.Title);

            MemberSubscription memberSubscription = await _memberRepository.GetMemberSubscription(member.Id);

            DateTime expiryDate = (memberSubscription?.ExpiryDate ?? DateTime.UtcNow).AddMonths(chapterSubscription.Months);
            memberSubscription = new MemberSubscription(member.Id, chapterSubscription.Type, expiryDate);

            await _memberRepository.UpdateMemberSubscription(memberSubscription);

            MemberSubscriptionRecord record = new MemberSubscriptionRecord(memberId, chapterSubscription.Type, DateTime.UtcNow);
            await _memberRepository.AddMemberSubscriptionRecord(record);

            _cacheService.UpdatedVersionedItem(memberSubscription, memberId);

            return memberSubscription;
        }

        public async Task<MemberImage> RotateMemberImage(Guid memberId, int degrees)
        {
            Member member = await GetMember(memberId, memberId);

            MemberImage image = await _memberRepository.GetMemberImage(memberId);
            if (image == null)
            {
                return null;
            }

            byte[] data = _imageService.Rotate(image.ImageData, degrees);
            return await UpdateMemberImage(member, data, image.MimeType);
        }

        public async Task RequestMemberEmailAddressUpdate(Guid memberId, string newEmailAddress)
        {
            Member member = await GetMember(memberId, memberId);
            if (member.EmailAddress.Equals(newEmailAddress, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            if (!MailUtils.ValidEmailAddress(newEmailAddress))
            {
                throw new OdkServiceException("Invalid email address format");
            }

            MemberEmailAddressUpdateToken existingToken = await _memberRepository.GetMemberEmailAddressUpdateToken(member.Id);
            if (existingToken != null)
            {
                await _memberRepository.DeleteEmailAddressUpdateToken(member.Id);
            }

            string activationToken = RandomStringGenerator.Generate(64);

            MemberEmailAddressUpdateToken token = new MemberEmailAddressUpdateToken(member.Id, newEmailAddress, activationToken);
            await _memberRepository.AddEmailAddressUpdateToken(token);

            Chapter chapter = await _chapterRepository.GetChapter(member.ChapterId);

            string url = _settings.ConfirmEmailAddressUpdateUrl.Interpolate(new Dictionary<string, string>
            {
                { "chapter.name", chapter.Name },
                { "token", HttpUtility.UrlEncode(activationToken) }
            });

            await _emailService.SendEmail(chapter, newEmailAddress, EmailType.EmailAddressUpdate, new Dictionary<string, string>
            {
                { "chapter.name", chapter.Name },
                { "url", url }
            });
        }

        public async Task UpdateMemberEmailOptIn(Guid memberId, bool optIn)
        {
            Member member = await GetMember(memberId, memberId);
            if (member.EmailOptIn == optIn)
            {
                return;
            }

            await _memberRepository.UpdateMember(member.Id, optIn, member.FirstName, member.LastName);

            _cacheService.RemoveVersionedItem<Member>(memberId);
        }

        public async Task<MemberImage> UpdateMemberImage(Guid id, UpdateMemberImage image)
        {
            Member member = await GetMember(id, id);

            return await UpdateMemberImage(member, image.ImageData, image.MimeType);
        }

        public async Task<MemberProfile> UpdateMemberProfile(Guid id, UpdateMemberProfile profile)
        {
            Member member = await GetMember(id);
            await ValidateMemberProfile(member.ChapterId, profile);

            MemberProfile existing = await GetMemberProfile(id);
            UpdateMemberProfile(existing, profile);

            await _memberRepository.UpdateMember(id, existing.EmailOptIn, existing.FirstName, existing.LastName);
            await _memberRepository.UpdateMemberProperties(id, existing.MemberProperties);

            IReadOnlyCollection<Member> members = await _memberRepository.GetMembers(member.ChapterId);
            long version = await _memberRepository.GetMembersVersion(member.ChapterId);
            _cacheService.UpdatedVersionedCollection(members, version, member.ChapterId);

            _cacheService.RemoveVersionedItem<Member>(member.Id);

            return existing;
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
                    yield return chapterProperty.Label;
                }
            }
        }

        private static void UpdateMemberProfile(MemberProfile existing, UpdateMemberProfile update)
        {
            existing.EmailOptIn = update.EmailOptIn;
            existing.FirstName = update.FirstName.Trim();
            existing.LastName = update.LastName.Trim();

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

        private static void ValidateMemberImage(string mimeType, byte[] data)
        {
            if (!ImageValidator.IsValidMimeType(mimeType) || !ImageValidator.IsValidData(data))
            {
                throw new OdkServiceException("File is not a valid image");
            }
        }

        private MemberImage CreateMemberImage(Guid memberId, string mimeType, byte[] imageData)
        {
            ValidateMemberImage(mimeType, imageData);

            byte[] data = EnforceMaxImageSize(imageData);
            return new MemberImage(memberId, data, mimeType, 0);
        }

        private byte[] EnforceMaxImageSize(byte[] imageData)
        {
            return _imageService.Reduce(imageData, _settings.MaxImageSize, _settings.MaxImageSize);
        }

        private async Task<Member> GetMember(Guid currentMemberId, Guid memberId)
        {
            VersionedServiceResult<Member> member = await _cacheService.GetOrSetVersionedItem(() => _memberRepository.GetMember(memberId), memberId, null);
            await _authorizationService.AssertMemberIsChapterMember(currentMemberId, member.Value.ChapterId);
            return member.Value;
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

        private async Task<MemberImage> UpdateMemberImage(Member member, byte[] imageData, string mimeType)
        {
            MemberImage update = CreateMemberImage(member.Id, mimeType, imageData);

            await _memberRepository.UpdateMemberImage(update);

            MemberImage updated = await _memberRepository.GetMemberImage(member.Id);

            _cacheService.UpdatedVersionedItem(updated, member.Id);

            return updated;
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

            if (!MailUtils.ValidEmailAddress(profile.EmailAddress))
            {
                throw new OdkServiceException("Invalid email address format");
            }
        }
    }
}
