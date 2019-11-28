using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Web;
using ODK.Data.Repositories;
using ODK.Umbraco.Content;
using ODK.Umbraco.Emails;
using ODK.Umbraco.Settings;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Web;
using OdkMember = ODK.Core.Members.IMember;
using OdkMemberGroup = ODK.Core.Members.MemberGroup;
using UmbracoMember = Umbraco.Core.Models.IMember;
using ODK.Core.Members;
using System.Threading.Tasks;

namespace ODK.Umbraco.Members
{
    public class OdkMemberService
    {
        private readonly OdkEmailService _emailService = new OdkEmailService();
        private readonly MemberRepository _membersDataService;
        private readonly IMediaService _umbracoMediaService;
        private readonly IMemberService _umbracoMemberService;

        public OdkMemberService(IMediaService umbracoMediaService, IMemberService umbracoMemberService, MemberRepository membersDataService)
        {
            _membersDataService = membersDataService;
            _umbracoMediaService = umbracoMediaService;
            _umbracoMemberService = umbracoMemberService;
        }

        public async Task<ServiceResult> AddMemberGroup(int chapterId, string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return new ServiceResult(nameof(name), "Group cannot be empty");
            }

            IReadOnlyCollection<OdkMemberGroup> memberGroups = await _membersDataService.GetMemberGroups(chapterId);
            if (memberGroups.Any(x => x.Name.Equals(name)))
            {
                return new ServiceResult(nameof(name), $"Group {name} already exists");
            }

            await _membersDataService.AddMemberGroup(chapterId, name);

            return new ServiceResult(true);
        }

        public ServiceResult ChangePassword(int id, ChangePasswordModel model)
        {
            UmbracoMember member = _umbracoMemberService.GetById(id);
            if (member == null)
            {
                return new ServiceResult("", "Member not found");
            }

            _umbracoMemberService.SavePassword(member, model.NewPassword);

            return new ServiceResult(true);
        }

        public void CreatePasswordRequest(SiteSettings siteSettings, string email, string url, UmbracoHelper helper)
        {
            IPublishedContent umbracoMember = helper.MembershipHelper.GetByEmail(email);
            if (umbracoMember == null)
            {
                return;
            }

            OdkMember member = new MemberModel(umbracoMember);

            Guid token = CreateCryptographicallySecureGuid();
            // await _membersDataService.AddPasswordResetRequest(member.Id, DateTime.Now, DateTime.Now.AddMinutes(30), token.ToString());            

            url = url.Replace("{token}", token.ToString());

            IPublishedContent chapter = helper.Content(member.Chapter.Id);
            string body = chapter.GetPropertyValue<string>("resetPasswordEmailBody");
            body = body.Replace("{{resetPasswordUrl}}", url);
            SendMemberEmail(siteSettings, member, chapter, "Password reset", body, true, null);
        }

        public async Task<ServiceResult> DeleteMemberGroup(int groupId)
        {
            await _membersDataService.DeleteMemberGroup(groupId);

            return new ServiceResult(true);
        }

        public OdkMember GetMember(int id, UmbracoHelper helper)
        {
            if (!IsAllowed(helper))
            {
                return null;
            }

            IPublishedContent member = helper.TypedMember(id);
            return GetMember(member);
        }

        public async Task<IDictionary<int, IReadOnlyCollection<MemberGroupModel>>> GetMemberGroupMembers(int chapterId)
        {
            var memberGroupMembers = await _membersDataService.GetMemberGroupMembers(chapterId);
            return memberGroupMembers.ToDictionary(
                x => x.Key,
                x => (IReadOnlyCollection<MemberGroupModel>)(x.Value.Select(v => new MemberGroupModel(v.GroupId, v.Name)).ToArray()));
        }

        public async Task<IReadOnlyCollection<MemberGroupModel>> GetMemberGroups(int chapterId)
        {
            var memberGroups = await _membersDataService.GetMemberGroups(chapterId);

            return memberGroups.Select(x => new MemberGroupModel(x.GroupId, x.Name)).ToArray();
        }

        public IReadOnlyCollection<OdkMember> GetMembers(MemberSearchCriteria criteria, UmbracoHelper helper)
        {
            if (!IsAllowed(helper))
            {
                return new OdkMember[] { };
            }

            IEnumerable<OdkMember> models = _umbracoMemberService
                .GetAllMembers()
                .Select(x => helper.TypedMember(x.Id))
                .Select(x => new MemberModel(x))
                .Where(x => x.Chapter.Id == criteria.ChapterId);

            if (!criteria.ShowAll)
            {
                models = models.Where(x => !x.Disabled);
            }

            if (criteria.Types != null)
            {
                models = models.Where(x => criteria.Types.Contains(x.Type));
            }

            if (criteria.Sort != null)
            {
                models = criteria.Sort(models);
            }

            if (criteria.MaxItems > 0)
            {
                models = models.Take(criteria.MaxItems.Value);
            }

            return models.ToArray();
        }

        public async Task<bool> IsValidPasswordResetRequestToken(string token)
        {
            var resetRequest = await _membersDataService.GetPasswordResetRequest(token);
            return resetRequest?.Expires > DateTime.Now;
        }

        public ServiceResult Register(SiteSettings siteSettings, RegisterMemberModel member, UmbracoHelper helper)
        {
            if (_umbracoMemberService.GetByUsername(member.Email) != null)
            {
                return new ServiceResult(nameof(member.Email), "Email already registered");
            }

            IDictionary<string, string> validationMessages = ValidateModel(member, member.UploadedPicture, helper);
            if (validationMessages.Any())
            {
                return new ServiceResult(validationMessages);
            }

            string memberType = _umbracoMemberService.GetDefaultMemberType();

            UmbracoMember umbracoMember = _umbracoMemberService.CreateMember(member.Email, member.Email, member.FullName, memberType);

            umbracoMember.SetValue(MemberPropertyNames.ChapterId, helper.GetPublishedContentAsPropertyValue(member.Chapter.Id));

            IMedia picture = SaveImage(member.UploadedPicture, member);

            UpdateMemberProperties(umbracoMember, member, helper, picture);

            IPublishedContent chapter = helper.Content(member.Chapter.Id);
            int trialPeriodMonths = chapter.GetPropertyValue<int>("trialPeriodMonths");
            UpdateMemberSubscriptionProperties(umbracoMember, member, MemberTypes.Trial, DateTime.Today.AddMonths(trialPeriodMonths) - DateTime.Today);

            _umbracoMemberService.Save(umbracoMember);
            _umbracoMemberService.SavePassword(umbracoMember, member.Password);

            // Send emails
            SendNewMemberAdminEmail(siteSettings, member, chapter);
            SendNewMemberEmail(siteSettings, member, chapter);

            return new ServiceResult(true);
        }

        public async Task<ServiceResult> ResetPassword(string password, string token)
        {
            MemberPasswordResetRequest request = await _membersDataService.GetPasswordResetRequest(token);
            if (request == null)
            {
                return new ServiceResult(false, "Request not found");
            }

            // UmbracoMember member = _umbracoMemberService.GetById(request.MemberId);
            UmbracoMember member = null;

            string message = null;
            if (request.Expires < DateTime.Now)
            {
                message = "Request expired";
            }
            else if (member == null)
            {
                message = "User not found";
            }

            if (message == null)
            {
                _umbracoMemberService.SavePassword(member, password);
            }

            // await _membersDataService.DeletePasswordResetRequest(request.PasswordResetRequestId);

            return new ServiceResult(message == null, message);
        }

        public void SendMemberEmails(SiteSettings siteSettings, IEnumerable<OdkMember> members, string subject, string body, bool overrideOptIn, string from, 
            UmbracoHelper helper)
        {
            foreach (OdkMember member in members)
            {
                IPublishedContent chapter = helper.Content(member.Chapter.Id);
                SendMemberEmail(siteSettings, member, chapter, subject, body, overrideOptIn, from);
            }
        }

        public ServiceResult Update(int id, UpdateMemberModel model, UmbracoHelper helper)
        {
            UmbracoMember member = _umbracoMemberService.GetById(id);
            if (member == null)
            {
                return new ServiceResult("", "Member not found");
            }

            IDictionary<string, string> validationMessages = ValidateModel(model, model.UploadedPicture, helper);
            if (validationMessages.Any())
            {
                return new ServiceResult(validationMessages);
            }

            IMedia picture = null;
            if (model.UploadedPicture != null)
            {
                picture = SaveImage(model.UploadedPicture, model);
            }

            UpdateMemberProperties(member, model, helper, picture);

            _umbracoMemberService.Save(member);

            return new ServiceResult(true);
        }

        public async Task<ServiceResult> UpdateMemberGroups(int memberId, IReadOnlyCollection<int> groupIds)
        {
            IReadOnlyCollection<OdkMemberGroup> existing = await _membersDataService.GetMemberGroupsForMember(memberId);

            groupIds = groupIds ?? new int[] { };

            // add new
            foreach (int groupId in groupIds.Where(x => !existing.Any(g => g.GroupId == x)))
            {
                await _membersDataService.AddMemberToGroup(memberId, groupId);
            }

            // remove old
            foreach (OdkMemberGroup group in existing.Where(x => !groupIds.Any(g => g == x.GroupId)))
            {
                await _membersDataService.RemoveMemberFromGroup(memberId, group.GroupId);
            }

            return new ServiceResult(true);
        }

        public ServiceResult UpdateSubscription(OdkMember model, MemberTypes type, TimeSpan length, double amount)
        {
            UmbracoMember member = _umbracoMemberService.GetById(model.Id);
            if (member == null)
            {
                return new ServiceResult("", "Member not found");
            }

            UpdateMemberSubscriptionProperties(member, model, type, length, amount);

            _umbracoMemberService.Save(member);

            return new ServiceResult(true);
        }

        private static Guid CreateCryptographicallySecureGuid()
        {
            using (var provider = new RNGCryptoServiceProvider())
            {
                var bytes = new byte[16];
                provider.GetBytes(bytes);

                return new Guid(bytes);
            }
        }

        private static string ReplaceMemberProperties(string text, OdkMember member)
        {
            return text
                .Replace($"{{{{{MemberPropertyNames.Email}}}}}", member.Email)
                .Replace($"{{{{{MemberPropertyNames.FavouriteBeverage}}}}}", member.FavouriteBeverage)
                .Replace($"{{{{{MemberPropertyNames.FirstName}}}}}", member.FirstName)
                .Replace($"{{{{{MemberPropertyNames.Hometown}}}}}", member.Hometown)
                .Replace($"{{{{{MemberPropertyNames.KnittingExperience}}}}}", member.KnittingExperience)
                .Replace($"{{{{{MemberPropertyNames.KnittingExperienceOther}}}}}", member.KnittingExperienceOther)
                .Replace($"{{{{{MemberPropertyNames.LastName}}}}}", member.LastName)
                .Replace($"{{{{{MemberPropertyNames.Neighbourhood}}}}}", member.Neighbourhood)
                .Replace($"{{{{{MemberPropertyNames.Reason}}}}}", member.Reason);
        }

        private static bool IsAllowed(UmbracoHelper helper)
        {
            return !string.IsNullOrEmpty(helper.MembershipHelper.CurrentUserName);
        }

        private static void UpdateMemberProperties(UmbracoMember member, OdkMember model, UmbracoHelper helper, IMedia picture)
        {
            IEnumerable<KeyValuePair<int, string>> knittingExperienceOptions = helper.GetKnittingExperienceOptions();

            member.SetValue(MemberPropertyNames.EmailOptIn, model.EmailOptIn);
            member.SetValue(MemberPropertyNames.FacebookProfile, model.FacebookProfile);
            member.SetValue(MemberPropertyNames.FavouriteBeverage, model.FavouriteBeverage);
            member.SetValue(MemberPropertyNames.FirstName, model.FirstName);
            member.SetValue(MemberPropertyNames.Hometown, model.Hometown);
            member.SetValue(MemberPropertyNames.KnittingExperience, knittingExperienceOptions.First(x => x.Value == model.KnittingExperience).Key);
            member.SetValue(MemberPropertyNames.KnittingExperienceOther, model.KnittingExperienceOther);
            member.SetValue(MemberPropertyNames.LastName, model.LastName);
            member.SetValue(MemberPropertyNames.Neighbourhood, model.Neighbourhood);
            member.SetValue(MemberPropertyNames.Reason, model.Reason);

            if (picture != null)
            {
                member.SetValue(MemberPropertyNames.Picture, picture.ToPropertyValue());
            }
        }

        private static void UpdateMemberSubscriptionProperties(UmbracoMember member, OdkMember model, MemberTypes type, TimeSpan length, double amount = 0)
        {
            DateTime subscriptionEndDate = DateTime.Today.Add(length);
            if (model.SubscriptionEndDate != null)
            {
                subscriptionEndDate = model.SubscriptionEndDate.Value.Add(length);
            }

            if (amount > 0)
            {
                member.SetValue(MemberPropertyNames.LastPaymentAmount, amount);
                member.SetValue(MemberPropertyNames.LastPaymentDate, DateTime.Today.ToString("yyyy-MM-dd"));
            }

            member.SetValue(MemberPropertyNames.SubscriptionEndDate, subscriptionEndDate);
            member.SetValue(MemberPropertyNames.Type, (int)type);
        }

        private OdkMember GetMember(IPublishedContent member)
        {
            if (member == null)
            {
                return null;
            }

            return new MemberModel(member);
        }

        private IMedia SaveImage(HttpPostedFileBase file, OdkMember member)
        {
            IMedia peopleFolder = _umbracoMediaService.GetRootMedia().FirstOrDefault(x => x.Name == "People");

            IMedia chapterFolder = peopleFolder.Children().FirstOrDefault(x => x.Name == member.Chapter.Name);
            if (chapterFolder == null)
            {
                chapterFolder = _umbracoMediaService.CreateMedia(member.Chapter.Name, peopleFolder, "Folder");
            }

            string key = member.FullName;
            IMedia existing = chapterFolder.Children().FirstOrDefault(x => x.Name == member.FullName);
            if (existing != null)
            {
                existing.Name = member.FullName + "-deleted";
                _umbracoMediaService.Save(existing);
            }

            IMedia memberImage = _umbracoMediaService.CreateMedia(member.FullName, chapterFolder, "Image");
            memberImage.SetValue("umbracoFile", file);

            _umbracoMediaService.Save(memberImage);

            if (existing != null)
            {
                _umbracoMediaService.Delete(existing);
            }

            return memberImage;
        }

        private void SendMemberEmail(SiteSettings siteSettings, OdkMember model, IPublishedContent chapter, string subject, string body, bool overrideOptIn, string from)
        {
            if (!overrideOptIn && !model.EmailOptIn)
            {
                return;
            }

            body = ReplaceMemberProperties(body, model);

            _emailService.SendEmail(siteSettings.SiteUrl, chapter, subject, body, new string[] { model.Email }, from);
        }

        private void SendNewMemberAdminEmail(SiteSettings siteSettings, OdkMember member, IPublishedContent chapter)
        {
            string subject = chapter.GetPropertyValue<string>("newMemberEmailSubjectAdmin");
            string body = chapter.GetPropertyValue<string>("newMemberEmailBodyAdmin");
            body = ReplaceMemberProperties(body, member);
            _emailService.SendAdminEmail(siteSettings.SiteUrl, chapter, subject, body);
        }

        private void SendNewMemberEmail(SiteSettings siteSettings, OdkMember member, IPublishedContent chapter)
        {
            string subject = chapter.GetPropertyValue<string>("newMemberEmailSubject");
            string body = chapter.GetPropertyValue<string>("newMemberEmailBody");

            SendMemberEmail(siteSettings, member, chapter, subject, body, false, null);
        }

        private IDictionary<string, string> ValidateModel<T>(T model, HttpPostedFileBase image, UmbracoHelper helper) where T : OdkMember, IMemberPictureUpload
        {
            Dictionary<string, string> messages = new Dictionary<string, string>();

            if (helper == null)
            {
                return messages;
            }

            IEnumerable<KeyValuePair<int, string>> knittingExperienceOptions = helper.GetKnittingExperienceOptions();
            if (!knittingExperienceOptions.Any(x => x.Value == model.KnittingExperience))
            {
                messages.Add(nameof(model.KnittingExperience), "Knitting know-how required");
            }

            if (model.KnittingExperience == knittingExperienceOptions.Last().Value && string.IsNullOrWhiteSpace(model.KnittingExperienceOther))
            {
                messages.Add(nameof(model.KnittingExperienceOther), "Knitting know-how 'other' required");
            }

            if (image != null && !image.ContentType.StartsWith("image/"))
            {
                messages.Add(nameof(model.UploadedPicture), "File type not allowed. Please upload an image.");
            }

            return messages;
        }
    }
}
