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
    private readonly Lazy<IChapterEmailProviderRepository> _chapterEmailProviderRepository;
    private readonly Lazy<IChapterEmailRepository> _chapterEmailRepository;
    private readonly Lazy<IChapterLinksRepository> _chapterLinksRepository;
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
    private readonly Lazy<IEmailRepository> _emailRepository;
    private readonly Lazy<IErrorPropertyRepository> _errorPropertyRepository;
    private readonly Lazy<IErrorRepository> _errorRepository;
    private readonly Lazy<IEventEmailRepository> _eventEmailRepository;
    private readonly Lazy<IEventInviteRepository> _eventInviteRepository;
    private readonly Lazy<IEventRepository> _eventRepository;
    private readonly Lazy<IEventResponseRepository> _eventResponseRepository;
    private readonly Lazy<IInstagramImageRepository> _instagramImageRepository;
    private readonly Lazy<IInstagramPostRepository> _instagramPostRepository;
    private readonly Lazy<IMemberActivationTokenRepository> _memberActivationTokenRepository;
    private readonly Lazy<IMemberEmailAddressUpdateTokenRepository> _memberEmailAddressUpdateTokenRepository;
    private readonly Lazy<IMemberImageRepository> _memberImageRepository;
    private readonly Lazy<IMemberPasswordRepository> _memberPasswordRepository;
    private readonly Lazy<IMemberPasswordResetRequestRepository> _memberPasswordResetRequestRepository;
    private readonly Lazy<IMemberPropertyRepository> _memberPropertyRepository;
    private readonly Lazy<IMemberRepository> _memberRepository;
    private readonly Lazy<IMemberSubscriptionRepository> _memberSubscriptionRepository;
    private readonly Lazy<IPaymentRepository> _paymentRepository;
    private readonly Lazy<ISiteSettingsRepository> _siteSettingsRepository;
    private readonly Lazy<IVenueRepository> _venueRepository;

    public UnitOfWork(OdkContext context)
    {
        _context = context;

        _chapterAdminMemberRepository = new(() => new ChapterAdminMemberRepository(_context));
        _chapterEmailProviderRepository = new(() => new ChapterEmailProviderRepository(_context));
        _chapterEmailRepository = new(() => new ChapterEmailRepository(_context));
        _chapterLinksRepository = new(() => new ChapterLinksRepository(_context));
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
        _emailRepository = new(() => new EmailRepository(_context));
        _errorPropertyRepository = new(() => new ErrorPropertyRepository(_context));
        _errorRepository = new(() => new ErrorRepository(_context));
        _eventEmailRepository = new(() => new EventEmailRepository(_context));
        _eventInviteRepository = new(() => new EventInviteRepository(_context));
        _eventRepository = new(() => new EventRepository(_context));
        _eventResponseRepository = new(() => new EventResponseRepository(_context));
        _instagramImageRepository = new(() => new InstagramImageRepository(_context));
        _instagramPostRepository = new(() => new InstagramPostRepository(_context));
        _memberActivationTokenRepository = new(() => new MemberActivationTokenRepository(_context));
        _memberEmailAddressUpdateTokenRepository = new(() => new MemberEmailAddressUpdateTokenRepository(_context));
        _memberImageRepository = new(() => new MemberImageRepository(_context));
        _memberPasswordRepository = new(() => new MemberPasswordRepository(_context));
        _memberPasswordResetRequestRepository = new(() => new MemberPasswordResetRequestRepository(_context));
        _memberPropertyRepository = new(() => new MemberPropertyRepository(_context));
        _memberRepository = new(() => new MemberRepository(_context));
        _memberSubscriptionRepository = new(() => new MemberSubscriptionRepository(_context));
        _paymentRepository = new(() => new PaymentRepository(_context));
        _siteSettingsRepository = new(() => new SiteSettingsRepository(_context));
        _venueRepository = new(() => new VenueRepository(_context));
    }

    public IChapterAdminMemberRepository ChapterAdminMemberRepository => _chapterAdminMemberRepository.Value;
    public IChapterEmailProviderRepository ChapterEmailProviderRepository => _chapterEmailProviderRepository.Value;
    public IChapterEmailRepository ChapterEmailRepository => _chapterEmailRepository.Value;
    public IChapterLinksRepository ChapterLinksRepository => _chapterLinksRepository.Value;
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
    public IEmailRepository EmailRepository => _emailRepository.Value;
    public IErrorPropertyRepository ErrorPropertyRepository => _errorPropertyRepository.Value;
    public IErrorRepository ErrorRepository => _errorRepository.Value;
    public IEventEmailRepository EventEmailRepository => _eventEmailRepository.Value;
    public IEventInviteRepository EventInviteRepository => _eventInviteRepository.Value;
    public IEventRepository EventRepository => _eventRepository.Value;
    public IEventResponseRepository EventResponseRepository => _eventResponseRepository.Value;
    public IInstagramImageRepository InstagramImageRepository => _instagramImageRepository.Value;
    public IInstagramPostRepository InstagramPostRepository => _instagramPostRepository.Value;
    public IMemberActivationTokenRepository MemberActivationTokenRepository => _memberActivationTokenRepository.Value;
    public IMemberEmailAddressUpdateTokenRepository MemberEmailAddressUpdateTokenRepository => _memberEmailAddressUpdateTokenRepository.Value;
    public IMemberImageRepository MemberImageRepository => _memberImageRepository.Value;
    public IMemberPasswordRepository MemberPasswordRepository => _memberPasswordRepository.Value;
    public IMemberPasswordResetRequestRepository MemberPasswordResetRequestRepository => _memberPasswordResetRequestRepository.Value;
    public IMemberPropertyRepository MemberPropertyRepository => _memberPropertyRepository.Value;
    public IMemberRepository MemberRepository => _memberRepository.Value;
    public IMemberSubscriptionRepository MemberSubscriptionRepository => _memberSubscriptionRepository.Value;
    public IPaymentRepository PaymentRepository => _paymentRepository.Value;
    public ISiteSettingsRepository SiteSettingsRepository => _siteSettingsRepository.Value;
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

    public Task SaveChangesAsync() => _context.SaveChangesAsync();
}
