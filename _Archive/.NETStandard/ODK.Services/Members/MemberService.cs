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

        public async Task<ServiceResult> ConfirmEmailAddressUpdate(Guid memberId, string confirmationToken)
        {
            MemberEmailAddressUpdateToken? token = await _memberRepository.GetMemberEmailAddressUpdateToken(memberId);
            if (token == null)
            {
                return ServiceResult.Failure("Invalid link");
            }

            if (token.ConfirmationToken != confirmationToken)
            {
                return ServiceResult.Failure("Invalid link");
            }

            Member? existing = await _memberRepository.FindMemberByEmailAddress(token.NewEmailAddress);
            if (existing != null)
            {
                await _memberRepository.DeleteEmailAddressUpdateToken(memberId);
                return ServiceResult.Failure("Email not updated: new email address is already in use");
            }

            await _memberRepository.UpdateMemberEmailAddress(memberId, token.NewEmailAddress);
            _cacheService.RemoveVersionedItem<Member>(memberId);

            await _memberRepository.DeleteEmailAddressUpdateToken(memberId);

            return ServiceResult.Successful();
        }
        
        public async Task<ServiceResult> CreateMember(Guid chapterId, CreateMemberProfile profile)
        {
            ServiceResult validationResult = await ValidateMemberProfile(chapterId, profile);
            if (!validationResult.Success)
            {
                return validationResult;
            }

            ServiceResult imageResult = ValidateMemberImage(profile.Image.MimeType, profile.Image.ImageData);
            if (!imageResult.Success)
            {
                return imageResult;
            }

            Member? existing = await _memberRepository.FindMemberByEmailAddress(profile.EmailAddress);
            if (existing != null)
            {
                return ServiceResult.Failure("Email address already in use");
            }

            Member create = new Member(Guid.Empty, chapterId, profile.EmailAddress, profile.EmailOptIn ?? false, profile.FirstName, profile.LastName,
                DateTime.UtcNow, false, false, 0);

            Guid id = await _memberRepository.CreateMember(create);

            ChapterMembershipSettings? membershipSettings = await _chapterRepository.GetChapterMembershipSettings(chapterId);
            if (membershipSettings == null)
            {
                return ServiceResult.Failure("An error has occurred");
            }

            MemberSubscription subscription = new MemberSubscription(id, SubscriptionType.Trial, create.CreatedDate.AddMonths(membershipSettings.TrialPeriodMonths));
            await _memberRepository.UpdateMemberSubscription(subscription);

            MemberImage image = CreateMemberImage(id, profile.Image.MimeType, profile.Image.ImageData);
            await _memberRepository.AddMemberImage(image);

            IEnumerable<MemberProperty> memberProperties = profile.Properties
                .Select(x => new MemberProperty(Guid.Empty, id, x.ChapterPropertyId, x.Value));

            await _memberRepository.UpdateMemberProperties(id, memberProperties);

            string activationToken = RandomStringGenerator.Generate(64);

            await _memberRepository.AddActivationToken(new MemberActivationToken(id, activationToken));

            Chapter? chapter = await _chapterRepository.GetChapter(chapterId);

            string url = _settings.ActivateAccountUrl.Interpolate(new Dictionary<string, string>
            {
                { "chapter.name", chapter!.Name },
                { "token", HttpUtility.UrlEncode(activationToken) }
            });

            await _emailService.SendEmail(chapter, create.GetEmailAddressee(), EmailType.ActivateAccount, new Dictionary<string, string>
            {
                { "chapter.name", chapter.Name },
                { "url", url }
            });

            return ServiceResult.Successful();
        }

        public async Task DeleteMember(Guid memberId)
        {
            Member? member = await _memberRepository.GetMember(memberId, true);
            if (member == null)
            {
                return;
            }

            await _memberRepository.DeleteMember(memberId);

            _cacheService.RemoveVersionedItem<Member>(memberId);
            _cacheService.RemoveVersionedCollection<Member>(member.ChapterId);
        }

        public async Task<IReadOnlyCollection<Member>> GetLatestMembers(Member currentMember, Guid chapterId)
        {
            await _authorizationService.AssertMemberIsChapterMember(currentMember.Id, chapterId);

            IReadOnlyCollection<Member> members = await GetMembers(currentMember, chapterId);

            return members
                .OrderByDescending(x => x.CreatedDate)
                .Take(8)
                .ToArray();
        }

        public async Task<Member?> GetMember(Guid memberId)
        {
            return await GetMember(memberId, memberId);
        }

        public async Task<VersionedServiceResult<MemberImage>> GetMemberImage(long? currentVersion, Guid memberId, int? size)
        {
            return await GetMemberImage(currentVersion, memberId, size, size);
        }

        public async Task<VersionedServiceResult<MemberImage>> GetMemberImage(long? currentVersion, Guid memberId,
            int? width, int? height)
        {
            VersionedServiceResult<MemberImage> result = await _cacheService.GetOrSetVersionedItem(
                () => _memberRepository.GetMemberImage(memberId),
                memberId,
                currentVersion);

            if (currentVersion == result.Version)
            {
                return result;
            }

            MemberImage? image = result.Value;
            if (image == null)
            {
                return new VersionedServiceResult<MemberImage>(0, null);
            }

            if (width > 0 || height > 0)
            {
                int w = NumberUtils.FirstPositive(width, height) ?? 0;
                int h = NumberUtils.FirstPositive(height, width) ?? 0;

                byte[] imageData = _imageService.Crop(image.ImageData, w, h);
                image = new MemberImage(image.MemberId, imageData, image.MimeType, image.Version);
            }

            return new VersionedServiceResult<MemberImage>(image.Version, image);
        }

        public async Task<MemberProfile?> GetMemberProfile(Member currentMember, Member? member)
        {
            if (member == null || !member.CanBeViewedBy(currentMember))
            {
                return null;
            }

            return await GetMemberProfile(member);
        }

        public async Task<IReadOnlyCollection<MemberProperty>> GetMemberProperties(Guid memberId)
        {
            return await _memberRepository.GetMemberProperties(memberId);
        }
        
        public async Task<IReadOnlyCollection<Member>> GetMembers(Member? currentMember, Guid chapterId)
        {
            if (currentMember == null || currentMember.ChapterId != chapterId)
            {
                return Array.Empty<Member>();
            }

            return await _memberRepository.GetMembers(chapterId);
        }

        public async Task<MemberSubscription?> GetMemberSubscription(Guid memberId)
        {
            return await _memberRepository.GetMemberSubscription(memberId);
        }

        public async Task<ServiceResult> PurchaseSubscription(Guid memberId, Guid chapterSubscriptionId,
            string cardToken)
        {
            ChapterSubscription? chapterSubscription = await _chapterRepository.GetChapterSubscription(chapterSubscriptionId);
            if (chapterSubscription == null)
            {
                return ServiceResult.Failure("Payment not made: subscription not found");
            }

            Member? member = await GetMember(memberId);
            if (member == null || member.ChapterId != chapterSubscription.ChapterId)
            {
                return ServiceResult.Failure("Payment not made: you are not a member of this subscription's chapter");
            }
            
            ServiceResult paymentResult = await _paymentService.MakePayment(member, chapterSubscription.Amount, cardToken, 
                chapterSubscription.Title);
            if (!paymentResult.Success)
            {
                return ServiceResult.Failure($"Payment not made: {paymentResult.Message}");
            }

            MemberSubscription? memberSubscription = await _memberRepository.GetMemberSubscription(member.Id);

            DateTime expiryDate = (memberSubscription?.ExpiryDate ?? DateTime.UtcNow).AddMonths(chapterSubscription.Months);
            memberSubscription = new MemberSubscription(member.Id, chapterSubscription.Type, expiryDate);

            await _memberRepository.UpdateMemberSubscription(memberSubscription);

            MemberSubscriptionRecord record = new MemberSubscriptionRecord(memberId, chapterSubscription.Type, DateTime.UtcNow,
                chapterSubscription.Amount, chapterSubscription.Months);
            await _memberRepository.AddMemberSubscriptionRecord(record);

            _cacheService.UpdatedVersionedItem(memberSubscription, memberId);
            _cacheService.RemoveVersionedCollection<Member>(member.ChapterId);

            return ServiceResult.Successful();
        }
        
        public async Task RotateMemberImage(Guid memberId, int degrees)
        {
            Member? member = await GetMember(memberId, memberId);
            if (member == null)
            {
                return;
            }

            MemberImage? image = await _memberRepository.GetMemberImage(memberId);
            if (image == null)
            {
                return;
            }

            byte[] data = _imageService.Rotate(image.ImageData, degrees);
            await UpdateMemberImage(member, data, image.MimeType);
        }

        public async Task<ServiceResult> RequestMemberEmailAddressUpdate(Guid memberId, string newEmailAddress)
        {
            Member? member = await GetMember(memberId, memberId);
            if (member == null || member.EmailAddress.Equals(newEmailAddress, StringComparison.OrdinalIgnoreCase))
            {
                return ServiceResult.Successful("New email address matches old email address");
            }

            if (!MailUtils.ValidEmailAddress(newEmailAddress))
            {
                return ServiceResult.Failure("Invalid email address format");
            }

            MemberEmailAddressUpdateToken? existingToken = await _memberRepository.GetMemberEmailAddressUpdateToken(member.Id);
            if (existingToken != null)
            {
                await _memberRepository.DeleteEmailAddressUpdateToken(member.Id);
            }

            string activationToken = RandomStringGenerator.Generate(64);

            MemberEmailAddressUpdateToken token = new MemberEmailAddressUpdateToken(member.Id, newEmailAddress, activationToken);
            await _memberRepository.AddEmailAddressUpdateToken(token);

            Chapter? chapter = await _chapterRepository.GetChapter(member.ChapterId);

            string url = _settings.ConfirmEmailAddressUpdateUrl.Interpolate(new Dictionary<string, string>
            {
                { "chapter.name", chapter!.Name },
                { "token", HttpUtility.UrlEncode(activationToken) }
            });

            await _emailService.SendEmail(chapter, new EmailAddressee(newEmailAddress, member.FullName), EmailType.EmailAddressUpdate, 
                new Dictionary<string, string>
                {
                    { "chapter.name", chapter.Name },
                    { "url", url }
                });

            return ServiceResult.Successful();
        }

        public async Task UpdateMemberEmailOptIn(Guid memberId, bool optIn)
        {
            Member? member = await GetMember(memberId, memberId);
            if (member == null || member.EmailOptIn == optIn)
            {
                return;
            }

            await _memberRepository.UpdateMember(member.Id, optIn, member.FirstName, member.LastName);

            _cacheService.RemoveVersionedItem<Member>(memberId);
        }

        public async Task UpdateMemberImage(Guid id, UpdateMemberImage image)
        {
            Member? member = await GetMember(id, id);
            if (member == null)
            {
                return;
            }

            await UpdateMemberImage(member, image.ImageData, image.MimeType);
        }

        public async Task<ServiceResult> UpdateMemberProfile(Guid id, UpdateMemberProfile profile)
        {
            Member? member = await GetMember(id);
            if (member == null)
            {
                return ServiceResult.Failure("Member not found");
            }

            ServiceResult validationResult = await ValidateMemberProfile(member.ChapterId, profile);
            if (!validationResult.Success)
            {
                return validationResult;
            }

            MemberProfile? existing = await GetMemberProfile(id);
            UpdateMemberProfile(existing!, profile);

            await _memberRepository.UpdateMember(id, existing!.EmailOptIn, existing.FirstName, existing.LastName);
            await _memberRepository.UpdateMemberProperties(id, existing.MemberProperties);

            IReadOnlyCollection<Member> members = await _memberRepository.GetMembers(member.ChapterId);
            long version = await _memberRepository.GetMembersVersion(member.ChapterId);
            _cacheService.UpdatedVersionedCollection(members, version, member.ChapterId);

            _cacheService.RemoveVersionedItem<Member>(member.Id);

            return ServiceResult.Successful();
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
                memberPropertyDictionary.TryGetValue(chapterProperty.Id, out string? value);

                if (string.IsNullOrWhiteSpace(value))
                {
                    yield return chapterProperty.Label;
                }
            }
        }

        private static void UpdateMemberProfile(MemberProfile existing, UpdateMemberProfile update)
        {
            if (update.EmailOptIn != null)
            {
                existing.EmailOptIn = update.EmailOptIn.Value;
            }

            existing.FirstName = update.FirstName.Trim();
            existing.LastName = update.LastName.Trim();

            foreach (MemberProperty memberProperty in existing.MemberProperties)
            {
                UpdateMemberProperty? updateProperty = update.Properties?.FirstOrDefault(x => x.ChapterPropertyId == memberProperty.ChapterPropertyId);
                if (updateProperty == null)
                {
                    continue;
                }

                string value = updateProperty.Value;
                memberProperty.Update(value);
            }
        }

        private static ServiceResult ValidateMemberImage(string mimeType, byte[] data)
        {
            if (!ImageValidator.IsValidMimeType(mimeType) || !ImageValidator.IsValidData(data))
            {
                return ServiceResult.Failure("File is not a valid image");
            }

            return ServiceResult.Successful();
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

        private async Task<Member?> GetMember(Guid currentMemberId, Guid memberId)
        {
            Member? member = await _memberRepository.GetMember(memberId);
            if (member == null)
            {
                return null;
            }

            await _authorizationService.AssertMemberIsChapterMember(currentMemberId, member.ChapterId);
            return member;
        }

        private async Task<MemberProfile?> GetMemberProfile(Guid memberId)
        {
            return await GetMemberProfile(memberId, memberId);
        }

        private async Task<MemberProfile?> GetMemberProfile(Guid currentMemberId, Guid memberId)
        {
            Member? member = await GetMember(currentMemberId, memberId);
            if (member == null)
            {
                return null;
            }

            return await GetMemberProfile(member);
        }

        private async Task<MemberProfile> GetMemberProfile(Member member)
        {
            IReadOnlyCollection<ChapterProperty> chapterProperties = await _chapterRepository.GetChapterProperties(member.ChapterId);
            IReadOnlyCollection<MemberProperty> memberProperties = await _memberRepository.GetMemberProperties(member.Id);
            IDictionary<Guid, MemberProperty> memberPropertyDictionary = memberProperties.ToDictionary(x => x.ChapterPropertyId, x => x);

            IEnumerable<MemberProperty> allMemberProperties = chapterProperties.Select(x =>
                memberPropertyDictionary.ContainsKey(x.Id) ? memberPropertyDictionary[x.Id] : new MemberProperty(Guid.Empty, member.Id, x.Id, null));

            return new MemberProfile(member, allMemberProperties, chapterProperties);
        }
        
        private async Task UpdateMemberImage(Member member, byte[] imageData, string mimeType)
        {
            MemberImage update = CreateMemberImage(member.Id, mimeType, imageData);

            await _memberRepository.UpdateMemberImage(update);
        }
        
        private async Task<ServiceResult> ValidateMemberProfile(Guid chapterId, UpdateMemberProfile profile)
        {
            IReadOnlyCollection<ChapterProperty> chapterProperties = await _chapterRepository.GetChapterProperties(chapterId);
            IReadOnlyCollection<string> missingProperties = GetMissingMemberProfileProperties(profile, chapterProperties, profile.Properties).ToArray();

            if (missingProperties.Count > 0)
            {
                return ServiceResult.Failure($"The following properties are required: {string.Join(", ", missingProperties)}");
            }

            return ServiceResult.Successful();
        }

        private async Task<ServiceResult> ValidateMemberProfile(Guid chapterId, CreateMemberProfile profile)
        {
            IReadOnlyCollection<ChapterProperty> chapterProperties = await _chapterRepository.GetChapterProperties(chapterId);
            IReadOnlyCollection<string> missingProperties = GetMissingMemberProfileProperties(profile, chapterProperties, profile.Properties).ToArray();

            if (missingProperties.Count > 0)
            {
                return ServiceResult.Failure($"The following properties are required: {string.Join(", ", missingProperties)}");
            }

            if (!MailUtils.ValidEmailAddress(profile.EmailAddress))
            {
                return ServiceResult.Failure("Invalid email address format");
            }

            return ServiceResult.Successful();
        }
    }
}
