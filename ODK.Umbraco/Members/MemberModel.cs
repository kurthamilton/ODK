using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using ODK.Core.Chapters;
using ODK.Core.Members;
using Umbraco.Core.Models;
using Umbraco.Web;
using OdkMember = ODK.Core.Members.IMember;

namespace ODK.Umbraco.Members
{
    public class MemberModel : OdkMember
    {
        public const int DefaultKnittingExperienceOptionId = 0;

        private readonly Lazy<int?> _adminUserId;
        private readonly Lazy<bool> _disabled;
        private readonly MutableLazy<bool> _emailOptIn;
        private readonly MutableLazy<string> _favouriteBeverage;
        private readonly MutableLazy<string> _facebookProfile;
        private readonly MutableLazy<string> _firstName;
        private readonly MutableLazy<string> _hometown;
        private readonly MutableLazy<string> _knittingExperience;
        private readonly MutableLazy<string> _knittingExperienceOther;
        private readonly MutableLazy<string> _lastName;
        private readonly MutableLazy<double?> _lastPaymentAmount;
        private readonly MutableLazy<DateTime?> _lastPaymentDate;
        private readonly MutableLazy<string> _neighbourhood;
        private readonly Lazy<IPublishedContent> _picture;
        private readonly MutableLazy<string> _reason;
        private readonly MutableLazy<DateTime?> _subscriptionEndDate;
        private readonly Lazy<MemberTypes> _type;

        public MemberModel()
            : this(null)
        {
        }

        public MemberModel(IPublishedContent member)
        {
            if (member != null)
            {
                IPublishedContent chapter = member.GetPropertyValue<IPublishedContent>(MemberPropertyNames.ChapterId);
                if (chapter != null)
                {
                    Chapter = new ChapterModel(chapter.Id, chapter.Name);
                    ChapterContent = chapter;
                }
                
                Content = member;
                Email = member.GetPropertyValue<string>(MemberPropertyNames.Email);
                Id = member.Id;
                Joined = member.CreateDate;
            }

            if (Chapter == null)
            {
                Chapter = new ChapterModel(0, "");
            }

            _adminUserId = new Lazy<int?>(() => member?.GetPropertyValue<int?>(MemberPropertyNames.AdminUserId));
            _disabled = new Lazy<bool>(() => member != null ? !member.GetPropertyValue<bool>(MemberPropertyNames.Approved) : false);
            _emailOptIn = new MutableLazy<bool>(() => member != null ? member.GetPropertyValue<bool>(MemberPropertyNames.EmailOptIn) : false);
            _facebookProfile = new MutableLazy<string>(() => member?.GetPropertyValue<string>(MemberPropertyNames.FacebookProfile));
            _favouriteBeverage = new MutableLazy<string>(() => member?.GetPropertyValue<string>(MemberPropertyNames.FavouriteBeverage));
            _firstName = new MutableLazy<string>(() => member?.GetPropertyValue<string>(MemberPropertyNames.FirstName));
            _hometown = new MutableLazy<string>(() => member?.GetPropertyValue<string>(MemberPropertyNames.Hometown));
            _knittingExperience = new MutableLazy<string>(() => member?.GetPropertyValue<string>(MemberPropertyNames.KnittingExperience));
            _knittingExperienceOther = new MutableLazy<string>(() => member?.GetPropertyValue<string>(MemberPropertyNames.KnittingExperienceOther));
            _lastName = new MutableLazy<string>(() => member?.GetPropertyValue<string>(MemberPropertyNames.LastName));
            _lastPaymentAmount = new MutableLazy<double?>(() => member?.GetPropertyValue<double?>(MemberPropertyNames.LastPaymentAmount));
            _lastPaymentDate = new MutableLazy<DateTime?>(() => member?.GetPropertyValue<DateTime?>(MemberPropertyNames.LastPaymentDate));
            _neighbourhood = new MutableLazy<string>(() => member?.GetPropertyValue<string>(MemberPropertyNames.Neighbourhood));
            _picture = new Lazy<IPublishedContent>(() => member?.GetPropertyValue<IPublishedContent>(MemberPropertyNames.Picture));
            _reason = new MutableLazy<string>(() => member?.GetPropertyValue<string>(MemberPropertyNames.Reason));
            _subscriptionEndDate = new MutableLazy<DateTime?>(() => member?.GetPropertyValue<DateTime?>(MemberPropertyNames.SubscriptionEndDate));
            _type = new Lazy<MemberTypes>(() => member != null
                ? (MemberTypes)Enum.Parse(typeof(MemberTypes), member.GetPropertyValue<string>(MemberPropertyNames.Type))
                : MemberTypes.Trial);
        }

        public int? AdminUserId => _adminUserId.Value;

        public ChapterModel Chapter { get; private set; }

        public IPublishedContent ChapterContent { get; }

        public IPublishedContent Content { get; }

        public bool Disabled => _disabled.Value;

        [Required]
        [EmailAddress]
        [MaxLength(500, ErrorMessage = "Must not exceed 500 characters in length")]
        public string Email { get; set; }

        [DisplayName("Receive emails")]
        public bool EmailOptIn
        {
            get => _emailOptIn.Value;
            set => _emailOptIn.Value = value;
        }

        [DisplayName("Facebook profile")]
        [MaxLength(500, ErrorMessage = "Must not exceed 500 characters in length")]
        public string FacebookProfile
        {
            get => _facebookProfile?.Value;
            set => _facebookProfile.Value = value;
        }

        [DisplayName("Your favourite alcoholic beverage")]
        [Required]
        [MaxLength(500, ErrorMessage = "Must not exceed 500 characters in length")]
        public string FavouriteBeverage
        {
            get => _favouriteBeverage?.Value;
            set => _favouriteBeverage.Value = value;
        }

        [DisplayName("First Name")]
        [Required]
        [MaxLength(500, ErrorMessage = "Must not exceed 500 characters in length")]
        public string FirstName
        {
            get => _firstName?.Value;
            set => _firstName.Value = value;
        }

        public string FullName => FirstName + " " + LastName;

        [DisplayName("Where are you from?")]
        [MaxLength(500, ErrorMessage = "Must not exceed 500 characters in length")]
        public string Hometown
        {
            get => _hometown?.Value;
            set => _hometown.Value = value;
        }

        public int Id { get; }

        [DisplayName("Date joined")]
        public DateTime? Joined { get; }

        [DisplayName("What's your level of knitting know-how?")]
        public string KnittingExperience
        {
            get => _knittingExperience?.Value ?? "";
            set => _knittingExperience.Value = value;
        }

        [DisplayName("Please specify")]
        [MaxLength(500, ErrorMessage = "Must not exceed 500 characters in length")]
        public string KnittingExperienceOther
        {
            get { return _knittingExperienceOther?.Value; }
            set { _knittingExperienceOther.Value = value; }
        }

        [DisplayName("Last Name")]
        [Required]
        [MaxLength(500, ErrorMessage = "Must not exceed 500 characters in length")]
        public string LastName
        {
            get => _lastName?.Value;
            set => _lastName.Value = value;
        }

        public double? LastPaymentAmount
        {
            get => _lastPaymentAmount.Value;
            set => _lastPaymentAmount.Value = value;
        }

        public DateTime? LastPaymentDate
        {
            get => (_lastPaymentDate.Value != null && _lastPaymentDate.Value > DateTime.MinValue) ? _lastPaymentDate.Value : null;
            set => _lastPaymentDate.Value = value;
        }

        [MaxLength(500, ErrorMessage = "Must not exceed 500 characters in length")]
        public string Neighbourhood
        {
            get => _neighbourhood?.Value;
            set => _neighbourhood.Value = value;
        }

        public IPublishedContent Picture => _picture?.Value;

        public string PictureThumbnailUrl => _picture?.Value?.GetCropUrl(150, 150);

        public string PictureUrl => _picture?.Value?.GetCropUrl(height: 500);

        [DisplayName("Why are you a Drunken Knitwit?")]
        [Required]
        [MaxLength(500, ErrorMessage = "Must not exceed 500 characters in length")]
        public string Reason
        {
            get => _reason?.Value;
            set => _reason.Value = value;
        }

        [DisplayName("End date")]
        public DateTime? SubscriptionEndDate
        {
            get => _subscriptionEndDate.Value != null && _subscriptionEndDate.Value > DateTime.MinValue ? _subscriptionEndDate.Value : null;
            set => _subscriptionEndDate.Value = value;
        }

        public SubscriptionStatus SubscriptionStatus =>
            SubscriptionEndDate != null && SubscriptionEndDate.Value < DateTime.Today
            ? SubscriptionStatus.Expired
            : SubscriptionEndDate != null && (SubscriptionEndDate.Value - DateTime.Today).Days <= 30
            ? SubscriptionStatus.Expiring | SubscriptionStatus.Current
            : SubscriptionStatus.Current;

        [DisplayName("Membership type")]
        public MemberTypes Type => _type.Value;

        public void SetChapter(ChapterModel chapter)
        {
            if (Chapter != null)
            {
                return;
            }

            Chapter = chapter;
        }

        protected void CopyFrom(OdkMember other)
        {
            if (other == null)
            {
                return;
            }

            Email = other.Email;
            FacebookProfile = other.FacebookProfile;
            FavouriteBeverage = other.FavouriteBeverage;
            FirstName = other.FirstName;
            Hometown = other.Hometown;
            KnittingExperience = other.KnittingExperience;
            KnittingExperienceOther = other.KnittingExperienceOther;
            LastName = other.LastName;
            LastPaymentAmount = other.LastPaymentAmount;
            LastPaymentDate = other.LastPaymentDate;
            Neighbourhood = other.Neighbourhood;
            Reason = other.Reason;
            SubscriptionEndDate = other.SubscriptionEndDate;
        }
    }
}
