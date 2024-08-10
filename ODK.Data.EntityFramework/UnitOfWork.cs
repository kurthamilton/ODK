using ODK.Core.Settings;
using ODK.Core.SocialMedia;
using ODK.Data.Core;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Repositories;

namespace ODK.Data.EntityFramework;
public class UnitOfWork : IUnitOfWork
{
    private readonly OdkContext _context;

    private readonly Lazy<IChapterAdminMemberRepository> _chapterAdminMemberRepository;
    private readonly Lazy<IChapterEmailRepository> _chapterEmailRepository;
    private readonly Lazy<IChapterEventSettingsRepository> _chapterEventSettingsRepository;
    private readonly Lazy<IChapterLinksRepository> _chapterLinksRepository;
    private readonly Lazy<IChapterLocationRepository> _chapterLocationRepository;
    private readonly Lazy<IChapterMembershipSettingsRepository> _chapterMembershipSettingsRepository;
    private readonly Lazy<IChapterPaymentSettingsRepository> _chapterPaymentSettingsRepository;
    private readonly Lazy<IChapterPropertyOptionRepository> _chapterPropertyOptionRepository;
    private readonly Lazy<IChapterPropertyRepository> _chapterPropertyRepository;
    private readonly Lazy<IChapterQuestionRepository> _chapterQuestionRepository;
    private readonly Lazy<IChapterRepository> _chapterRepository;
    private readonly Lazy<IChapterSubscriptionRepository> _chapterSubscriptionRepository;
    private readonly Lazy<IChapterTextsRepository> _chapterTextsRepository;
    private readonly Lazy<IContactRequestRepository> _contactRequestRepository;
    private readonly Lazy<ICountryRepository> _countryRepository;
    private readonly Lazy<ICurrencyRepository> _currencyRepository;
    private readonly Lazy<IEmailProviderRepository> _emailProviderRepository;
    private readonly Lazy<IEmailRepository> _emailRepository;
    private readonly Lazy<IErrorPropertyRepository> _errorPropertyRepository;
    private readonly Lazy<IErrorRepository> _errorRepository;
    private readonly Lazy<IEventCommentRepository> _eventCommentRepository;
    private readonly Lazy<IEventEmailRepository> _eventEmailRepository;
    private readonly Lazy<IEventHostRepository> _eventHostRepository;
    private readonly Lazy<IEventInviteRepository> _eventInviteRepository;
    private readonly Lazy<IEventRepository> _eventRepository;
    private readonly Lazy<IEventResponseRepository> _eventResponseRepository;
    private readonly Lazy<IFeatureRepository> _featureRepository;
    private readonly Lazy<IInstagramImageRepository> _instagramImageRepository;
    private readonly Lazy<IInstagramPostRepository> _instagramPostRepository;
    private readonly Lazy<IMemberActivationTokenRepository> _memberActivationTokenRepository;
    private readonly Lazy<IMemberAvatarRepository> _memberAvatarRepository;
    private readonly Lazy<IMemberEmailAddressUpdateTokenRepository> _memberEmailAddressUpdateTokenRepository;
    private readonly Lazy<IMemberChapterRepository> _memberChapterRepository;
    private readonly Lazy<IMemberImageRepository> _memberImageRepository;
    private readonly Lazy<IMemberLocationRepository> _memberLocationRepository;
    private readonly Lazy<IMemberPasswordRepository> _memberPasswordRepository;
    private readonly Lazy<IMemberPasswordResetRequestRepository> _memberPasswordResetRequestRepository;
    private readonly Lazy<IMemberPrivacySettingsRepository> _memberPrivacySettingsRepository;
    private readonly Lazy<IMemberPropertyRepository> _memberPropertyRepository;
    private readonly Lazy<IMemberRepository> _memberRepository;
    private readonly Lazy<IMemberSiteSubscriptionRepository> _memberSiteSubscriptionRepository;
    private readonly Lazy<IMemberSubscriptionRepository> _memberSubscriptionRepository;
    private readonly Lazy<IPaymentRepository> _paymentRepository;    
    private readonly Lazy<ISiteEmailSettingsRepository> _siteEmailSettingsRepository;
    private readonly Lazy<ISiteSettingsRepository> _siteSettingsRepository;
    private readonly Lazy<ISiteSubscriptionPriceRepository> _siteSubscriptionPriceRepository;
    private readonly Lazy<ISiteSubscriptionRepository> _siteSubscriptionRepository;
    private readonly Lazy<IVenueRepository> _venueRepository;

    public UnitOfWork(OdkContext context)
    {
        _context = context;

        _chapterAdminMemberRepository = new(() => new ChapterAdminMemberRepository(_context));        
        _chapterEmailRepository = new(() => new ChapterEmailRepository(_context));
        _chapterEventSettingsRepository = new(() => new ChapterEventSettingsRepository(_context));
        _chapterLinksRepository = new(() => new ChapterLinksRepository(_context));
        _chapterLocationRepository = new(() => new ChapterLocationRepository(_context));
        _chapterMembershipSettingsRepository = new(() => new ChapterMembershipSettingsRepository(_context));
        _chapterPaymentSettingsRepository = new(() => new ChapterPaymentSettingsRepository(_context));
        _chapterPropertyOptionRepository = new(() => new ChapterPropertyOptionRepository(_context));
        _chapterPropertyRepository = new(() => new ChapterPropertyRepository(_context));
        _chapterQuestionRepository = new(() => new ChapterQuestionRepository(_context));
        _chapterRepository = new(() => new ChapterRepository(_context));
        _chapterSubscriptionRepository = new(() => new ChapterSubscriptionRepository(_context));
        _chapterTextsRepository = new(() => new ChapterTextsRepository(_context));
        _contactRequestRepository = new(() => new ContactRequestRepository(_context));
        _countryRepository = new(() => new CountryRepository(_context));
        _currencyRepository = new(() => new CurrencyRepository(_context));
        _emailProviderRepository = new(() => new EmailProviderRepository(_context));
        _emailRepository = new(() => new EmailRepository(_context));
        _errorPropertyRepository = new(() => new ErrorPropertyRepository(_context));
        _errorRepository = new(() => new ErrorRepository(_context));
        _eventCommentRepository = new(() => new EventCommentRepository(_context));
        _eventEmailRepository = new(() => new EventEmailRepository(_context));
        _eventHostRepository = new(() => new EventHostRepository(_context));
        _eventInviteRepository = new(() => new EventInviteRepository(_context));
        _eventRepository = new(() => new EventRepository(_context));
        _eventResponseRepository = new(() => new EventResponseRepository(_context));
        _featureRepository = new(() => new FeatureRepository(_context));
        _instagramImageRepository = new(() => new InstagramImageRepository(_context));
        _instagramPostRepository = new(() => new InstagramPostRepository(_context));
        _memberActivationTokenRepository = new(() => new MemberActivationTokenRepository(_context));
        _memberAvatarRepository = new(() => new MemberAvatarRepository(_context));
        _memberChapterRepository = new(() => new MemberChapterRepository(_context));
        _memberEmailAddressUpdateTokenRepository = new(() => new MemberEmailAddressUpdateTokenRepository(_context));
        _memberImageRepository = new(() => new MemberImageRepository(_context));
        _memberLocationRepository = new(() => new MemberLocationRepository(_context));
        _memberPasswordRepository = new(() => new MemberPasswordRepository(_context));
        _memberPasswordResetRequestRepository = new(() => new MemberPasswordResetRequestRepository(_context));
        _memberPrivacySettingsRepository = new(() => new MemberPrivacySettingsRepository(_context));
        _memberPropertyRepository = new(() => new MemberPropertyRepository(_context));
        _memberRepository = new(() => new MemberRepository(_context));
        _memberSiteSubscriptionRepository = new(() => new MemberSiteSubscriptionRepository(_context));
        _memberSubscriptionRepository = new(() => new MemberSubscriptionRepository(_context));
        _paymentRepository = new(() => new PaymentRepository(_context));        
        _siteEmailSettingsRepository = new(() => new SiteEmailSettingsRepository(_context));
        _siteSettingsRepository = new(() => new SiteSettingsRepository(_context));
        _siteSubscriptionPriceRepository = new(() => new SiteSubscriptionPriceRepository(_context));
        _siteSubscriptionRepository = new(() => new SiteSubscriptionRepository(_context));
        _venueRepository = new(() => new VenueRepository(_context));
    }

    public IChapterAdminMemberRepository ChapterAdminMemberRepository => _chapterAdminMemberRepository.Value;
    public IChapterEmailRepository ChapterEmailRepository => _chapterEmailRepository.Value;
    public IChapterEventSettingsRepository ChapterEventSettingsRepository => _chapterEventSettingsRepository.Value;
    public IChapterLinksRepository ChapterLinksRepository => _chapterLinksRepository.Value;
    public IChapterLocationRepository ChapterLocationRepository => _chapterLocationRepository.Value;
    public IChapterMembershipSettingsRepository ChapterMembershipSettingsRepository => _chapterMembershipSettingsRepository.Value;
    public IChapterPaymentSettingsRepository ChapterPaymentSettingsRepository => _chapterPaymentSettingsRepository.Value;
    public IChapterPropertyOptionRepository ChapterPropertyOptionRepository => _chapterPropertyOptionRepository.Value;
    public IChapterPropertyRepository ChapterPropertyRepository => _chapterPropertyRepository.Value;
    public IChapterQuestionRepository ChapterQuestionRepository => _chapterQuestionRepository.Value;
    public IChapterRepository ChapterRepository => _chapterRepository.Value;
    public IChapterSubscriptionRepository ChapterSubscriptionRepository => _chapterSubscriptionRepository.Value;
    public IChapterTextsRepository ChapterTextsRepository => _chapterTextsRepository.Value;
    public IContactRequestRepository ContactRequestRepository => _contactRequestRepository.Value;
    public ICountryRepository CountryRepository => _countryRepository.Value;
    public ICurrencyRepository CurrencyRepository => _currencyRepository.Value;
    public IEmailProviderRepository EmailProviderRepository => _emailProviderRepository.Value;
    public IEmailRepository EmailRepository => _emailRepository.Value;
    public IErrorPropertyRepository ErrorPropertyRepository => _errorPropertyRepository.Value;
    public IErrorRepository ErrorRepository => _errorRepository.Value;
    public IEventCommentRepository EventCommentRepository => _eventCommentRepository.Value;
    public IEventEmailRepository EventEmailRepository => _eventEmailRepository.Value;
    public IEventHostRepository EventHostRepository => _eventHostRepository.Value;
    public IEventInviteRepository EventInviteRepository => _eventInviteRepository.Value;
    public IEventRepository EventRepository => _eventRepository.Value;
    public IEventResponseRepository EventResponseRepository => _eventResponseRepository.Value;
    public IFeatureRepository FeatureRepository => _featureRepository.Value;
    public IInstagramImageRepository InstagramImageRepository => _instagramImageRepository.Value;
    public IInstagramPostRepository InstagramPostRepository => _instagramPostRepository.Value;
    public IMemberActivationTokenRepository MemberActivationTokenRepository => _memberActivationTokenRepository.Value;
    public IMemberAvatarRepository MemberAvatarRepository => _memberAvatarRepository.Value;
    public IMemberChapterRepository MemberChapterRepository => _memberChapterRepository.Value;
    public IMemberEmailAddressUpdateTokenRepository MemberEmailAddressUpdateTokenRepository => _memberEmailAddressUpdateTokenRepository.Value;
    public IMemberImageRepository MemberImageRepository => _memberImageRepository.Value;
    public IMemberLocationRepository MemberLocationRepository => _memberLocationRepository.Value;
    public IMemberPasswordRepository MemberPasswordRepository => _memberPasswordRepository.Value;
    public IMemberPasswordResetRequestRepository MemberPasswordResetRequestRepository => _memberPasswordResetRequestRepository.Value;
    public IMemberPrivacySettingsRepository MemberChapterPrivacySettingsRepository => _memberPrivacySettingsRepository.Value;
    public IMemberPropertyRepository MemberPropertyRepository => _memberPropertyRepository.Value;
    public IMemberRepository MemberRepository => _memberRepository.Value;
    public IMemberSiteSubscriptionRepository MemberSiteSubscriptionRepository => _memberSiteSubscriptionRepository.Value;
    public IMemberSubscriptionRepository MemberSubscriptionRepository => _memberSubscriptionRepository.Value;
    public IPaymentRepository PaymentRepository => _paymentRepository.Value;
    public ISiteEmailSettingsRepository SiteEmailSettingsRepository => _siteEmailSettingsRepository.Value;
    public ISiteSettingsRepository SiteSettingsRepository => _siteSettingsRepository.Value;
    public ISiteSubscriptionPriceRepository SiteSubscriptionPriceRepository => _siteSubscriptionPriceRepository.Value;
    public ISiteSubscriptionRepository SiteSubscriptionRepository => _siteSubscriptionRepository.Value;
    public IVenueRepository VenueRepository => _venueRepository.Value;

    public async Task<(T1, T2)> RunAsync<T1, T2>(
        Func<IUnitOfWork, IDeferredQuery<T1>> query1, 
        Func<IUnitOfWork, IDeferredQuery<T2>> query2)
    {
        var q1 = query1(this);
        var q2 = query2(this);
        return (
            await q1.RunAsync(),
            await q2.RunAsync());
    }

    public async Task<(T1, T2, T3)> RunAsync<T1, T2, T3>(
        Func<IUnitOfWork, IDeferredQuery<T1>> query1,
        Func<IUnitOfWork, IDeferredQuery<T2>> query2,
        Func<IUnitOfWork, IDeferredQuery<T3>> query3)
    {
        var q1 = query1(this);
        var q2 = query2(this);
        var q3 = query3(this);

        return (
            await q1.RunAsync(),
            await q2.RunAsync(),
            await q3.RunAsync());
    }

    public async Task<(T1, T2, T3, T4)> RunAsync<T1, T2, T3, T4>(
        Func<IUnitOfWork, IDeferredQuery<T1>> query1,
        Func<IUnitOfWork, IDeferredQuery<T2>> query2,
        Func<IUnitOfWork, IDeferredQuery<T3>> query3,
        Func<IUnitOfWork, IDeferredQuery<T4>> query4)
    {
        var q1 = query1(this);
        var q2 = query2(this);
        var q3 = query3(this);
        var q4 = query4(this);

        return (
            await q1.RunAsync(),
            await q2.RunAsync(),
            await q3.RunAsync(),
            await q4.RunAsync());
    }

    public async Task<(T1, T2, T3, T4, T5)> RunAsync<T1, T2, T3, T4, T5>(
        Func<IUnitOfWork, IDeferredQuery<T1>> query1,
        Func<IUnitOfWork, IDeferredQuery<T2>> query2,
        Func<IUnitOfWork, IDeferredQuery<T3>> query3,
        Func<IUnitOfWork, IDeferredQuery<T4>> query4,
        Func<IUnitOfWork, IDeferredQuery<T5>> query5)
    {
        var q1 = query1(this);
        var q2 = query2(this);
        var q3 = query3(this);
        var q4 = query4(this);
        var q5 = query5(this);

        return (
            await q1.RunAsync(),
            await q2.RunAsync(),
            await q3.RunAsync(),
            await q4.RunAsync(),
            await q5.RunAsync());
    }

    public async Task<(T1, T2, T3, T4, T5, T6)> RunAsync<T1, T2, T3, T4, T5, T6>(
        Func<IUnitOfWork, IDeferredQuery<T1>> query1,
        Func<IUnitOfWork, IDeferredQuery<T2>> query2,
        Func<IUnitOfWork, IDeferredQuery<T3>> query3,
        Func<IUnitOfWork, IDeferredQuery<T4>> query4,
        Func<IUnitOfWork, IDeferredQuery<T5>> query5,
        Func<IUnitOfWork, IDeferredQuery<T6>> query6)
    {
        var q1 = query1(this);
        var q2 = query2(this);
        var q3 = query3(this);
        var q4 = query4(this);
        var q5 = query5(this);
        var q6 = query6(this);

        return (
            await q1.RunAsync(),
            await q2.RunAsync(),
            await q3.RunAsync(),
            await q4.RunAsync(),
            await q5.RunAsync(),
            await q6.RunAsync());
    }

    public async Task<(T1, T2, T3, T4, T5, T6, T7)> RunAsync<T1, T2, T3, T4, T5, T6, T7>(
        Func<IUnitOfWork, IDeferredQuery<T1>> query1,
        Func<IUnitOfWork, IDeferredQuery<T2>> query2,
        Func<IUnitOfWork, IDeferredQuery<T3>> query3,
        Func<IUnitOfWork, IDeferredQuery<T4>> query4,
        Func<IUnitOfWork, IDeferredQuery<T5>> query5,
        Func<IUnitOfWork, IDeferredQuery<T6>> query6,
        Func<IUnitOfWork, IDeferredQuery<T7>> query7)
    {
        var q1 = query1(this);
        var q2 = query2(this);
        var q3 = query3(this);
        var q4 = query4(this);
        var q5 = query5(this);
        var q6 = query6(this);
        var q7 = query7(this);

        return (
            await q1.RunAsync(),
            await q2.RunAsync(),
            await q3.RunAsync(),
            await q4.RunAsync(),
            await q5.RunAsync(),
            await q6.RunAsync(),
            await q7.RunAsync());
    }

    public async Task<(T1, T2, T3, T4, T5, T6, T7, T8)> RunAsync<T1, T2, T3, T4, T5, T6, T7, T8>(
        Func<IUnitOfWork, IDeferredQuery<T1>> query1,
        Func<IUnitOfWork, IDeferredQuery<T2>> query2,
        Func<IUnitOfWork, IDeferredQuery<T3>> query3,
        Func<IUnitOfWork, IDeferredQuery<T4>> query4,
        Func<IUnitOfWork, IDeferredQuery<T5>> query5,
        Func<IUnitOfWork, IDeferredQuery<T6>> query6,
        Func<IUnitOfWork, IDeferredQuery<T7>> query7,
        Func<IUnitOfWork, IDeferredQuery<T8>> query8)
    {
        var q1 = query1(this);
        var q2 = query2(this);
        var q3 = query3(this);
        var q4 = query4(this);
        var q5 = query5(this);
        var q6 = query6(this);
        var q7 = query7(this);
        var q8 = query8(this);

        return (
            await q1.RunAsync(),
            await q2.RunAsync(),
            await q3.RunAsync(),
            await q4.RunAsync(),
            await q5.RunAsync(),
            await q6.RunAsync(),
            await q7.RunAsync(),
            await q8.RunAsync());
    }

    public async Task<(T1, T2, T3, T4, T5, T6, T7, T8, T9)> RunAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9>(
        Func<IUnitOfWork, IDeferredQuery<T1>> query1,
        Func<IUnitOfWork, IDeferredQuery<T2>> query2,
        Func<IUnitOfWork, IDeferredQuery<T3>> query3,
        Func<IUnitOfWork, IDeferredQuery<T4>> query4,
        Func<IUnitOfWork, IDeferredQuery<T5>> query5,
        Func<IUnitOfWork, IDeferredQuery<T6>> query6,
        Func<IUnitOfWork, IDeferredQuery<T7>> query7,
        Func<IUnitOfWork, IDeferredQuery<T8>> query8,
        Func<IUnitOfWork, IDeferredQuery<T9>> query9)
    {
        var q1 = query1(this);
        var q2 = query2(this);
        var q3 = query3(this);
        var q4 = query4(this);
        var q5 = query5(this);
        var q6 = query6(this);
        var q7 = query7(this);
        var q8 = query8(this);
        var q9 = query9(this);

        return (
            await q1.RunAsync(),
            await q2.RunAsync(),
            await q3.RunAsync(),
            await q4.RunAsync(),
            await q5.RunAsync(),
            await q6.RunAsync(),
            await q7.RunAsync(),
            await q8.RunAsync(),
            await q9.RunAsync());
    }

    public Task SaveChangesAsync() => _context.SaveChangesAsync();
}
