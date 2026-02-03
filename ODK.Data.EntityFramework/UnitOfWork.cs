using ODK.Core.Platforms;
using ODK.Data.Core;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Repositories;
using ODK.Data.EntityFramework.Repositories;

namespace ODK.Data.EntityFramework;

public class UnitOfWork : IUnitOfWork
{
    private readonly OdkContext _context;
    private readonly IPlatformProvider _platformProvider;

    private readonly Lazy<IChapterAdminMemberRepository> _chapterAdminMemberRepository;
    private readonly Lazy<IChapterContactMessageReplyRepository> _chapterContactMessageReplyRepository;
    private readonly Lazy<IChapterContactMessageRepository> _chapterContactMessageRepository;
    private readonly Lazy<IChapterConversationMessageRepository> _chapterConversationMessageRepository;
    private readonly Lazy<IChapterConversationRepository> _chapterConversationRepository;
    private readonly Lazy<IChapterEmailRepository> _chapterEmailRepository;
    private readonly Lazy<IChapterEventSettingsRepository> _chapterEventSettingsRepository;
    private readonly Lazy<IChapterImageRepository> _chapterImageRepository;
    private readonly Lazy<IChapterLinksRepository> _chapterLinksRepository;
    private readonly Lazy<IChapterLocationRepository> _chapterLocationRepository;
    private readonly Lazy<IChapterMembershipSettingsRepository> _chapterMembershipSettingsRepository;
    private readonly Lazy<IChapterPageRepository> _chapterPageRepository;
    private readonly Lazy<IChapterPaymentAccountRepository> _chapterPaymentAccountRepository;
    private readonly Lazy<IChapterPaymentSettingsRepository> _chapterPaymentSettingsRepository;
    private readonly Lazy<IChapterPrivacySettingsRepository> _chapterPrivacySettingsRepository;
    private readonly Lazy<IChapterPropertyOptionRepository> _chapterPropertyOptionRepository;
    private readonly Lazy<IChapterPropertyRepository> _chapterPropertyRepository;
    private readonly Lazy<IChapterQuestionRepository> _chapterQuestionRepository;
    private readonly Lazy<IChapterRepository> _chapterRepository;
    private readonly Lazy<IChapterSubscriptionRepository> _chapterSubscriptionRepository;
    private readonly Lazy<IChapterTextsRepository> _chapterTextsRepository;
    private readonly Lazy<IChapterTopicRepository> _chapterTopicRepository;
    private readonly Lazy<ICountryRepository> _countryRepository;
    private readonly Lazy<ICurrencyRepository> _currencyRepository;
    private readonly Lazy<IDistanceUnitRepository> _distanceUnitRepository;
    private readonly Lazy<IEmailRepository> _emailRepository;
    private readonly Lazy<IErrorPropertyRepository> _errorPropertyRepository;
    private readonly Lazy<IErrorRepository> _errorRepository;
    private readonly Lazy<IEventCommentRepository> _eventCommentRepository;
    private readonly Lazy<IEventEmailRepository> _eventEmailRepository;
    private readonly Lazy<IEventHostRepository> _eventHostRepository;
    private readonly Lazy<IEventInviteRepository> _eventInviteRepository;
    private readonly Lazy<IEventRepository> _eventRepository;
    private readonly Lazy<IEventResponseRepository> _eventResponseRepository;
    private readonly Lazy<IEventTicketPaymentRepository> _eventTicketPaymentRepository;
    private readonly Lazy<IEventTicketSettingsRepository> _eventTicketSettingsRepository;
    private readonly Lazy<IEventTopicRepository> _eventTopicRepository;
    private readonly Lazy<IEventWaitlistMemberRepository> _eventWaitlistMemberRepository;
    private readonly Lazy<IFeatureRepository> _featureRepository;
    private readonly Lazy<IInstagramFetchLogEntryRepository> _instagramFetchLogEntryRepository;
    private readonly Lazy<IInstagramImageRepository> _instagramImageRepository;
    private readonly Lazy<IInstagramPostRepository> _instagramPostRepository;
    private readonly Lazy<IIssueMessageRepository> _issueMessageRepository;
    private readonly Lazy<IIssueRepository> _issueRepository;
    private readonly Lazy<IMemberActivationTokenRepository> _memberActivationTokenRepository;
    private readonly Lazy<IMemberAvatarRepository> _memberAvatarRepository;
    private readonly Lazy<IMemberEmailAddressUpdateTokenRepository> _memberEmailAddressUpdateTokenRepository;
    private readonly Lazy<IMemberChapterRepository> _memberChapterRepository;
    private readonly Lazy<IMemberEmailPreferenceRepository> _memberEmailPreferenceRepository;
    private readonly Lazy<IMemberImageRepository> _memberImageRepository;
    private readonly Lazy<IMemberLocationRepository> _memberLocationRepository;
    private readonly Lazy<IMemberNotificationSettingsRepository> _memberNotificationSettingsRepository;
    private readonly Lazy<IMemberPasswordRepository> _memberPasswordRepository;
    private readonly Lazy<IMemberPasswordResetRequestRepository> _memberPasswordResetRequestRepository;
    private readonly Lazy<IMemberPaymentSettingsRepository> _memberPaymentSettingsRepository;
    private readonly Lazy<IMemberPreferencesRepository> _memberPreferencesRepository;
    private readonly Lazy<IMemberPropertyRepository> _memberPropertyRepository;
    private readonly Lazy<IMemberRepository> _memberRepository;
    private readonly Lazy<IMemberSiteSubscriptionRepository> _memberSiteSubscriptionRepository;
    private readonly Lazy<IMemberSubscriptionRecordRepository> _memberSubscriptionRecordRepository;
    private readonly Lazy<IMemberSubscriptionRepository> _memberSubscriptionRepository;
    private readonly Lazy<IMemberTopicRepository> _memberTopicRepository;
    private readonly Lazy<INewChapterTopicRepository> _newChapterTopicRepository;
    private readonly Lazy<INewMemberTopicRepository> _newMemberTopicRepository;
    private readonly Lazy<INotificationRepository> _notificationRepository;
    private readonly Lazy<IPaymentCheckoutSessionRepository> _paymentCheckoutSessionRepository;
    private readonly Lazy<IPaymentProviderWebhookEventRepository> _paymentProviderWebhookEventRepository;
    private readonly Lazy<IPaymentRepository> _paymentRepository;
    private readonly Lazy<IQueuedEmailRecipientRepository> _queuedEmailRecipientRepository;
    private readonly Lazy<IQueuedEmailRepository> _queuedEmailRepository;
    private readonly Lazy<ISentEmailEventRepository> _sentEmailEventRepository;
    private readonly Lazy<ISentEmailRepository> _sentEmailRepository;
    private readonly Lazy<ISiteContactMessageReplyRepository> _siteContactMessageReplyRepository;
    private readonly Lazy<ISiteContactMessageRepository> _siteContactMessageRepository;
    private readonly Lazy<ISiteEmailSettingsRepository> _siteEmailSettingsRepository;
    private readonly Lazy<ISitePaymentSettingsRepository> _sitePaymentSettingsRepository;
    private readonly Lazy<ISiteSubscriptionFeatureRepository> _siteSubscriptionFeatureRepository;
    private readonly Lazy<ISiteSubscriptionPriceRepository> _siteSubscriptionPriceRepository;
    private readonly Lazy<ISiteSubscriptionRepository> _siteSubscriptionRepository;
    private readonly Lazy<ITopicGroupRepository> _topicGroupRepository;
    private readonly Lazy<ITopicRepository> _topicRepository;
    private readonly Lazy<IVenueLocationRepository> _venueLocationRepository;
    private readonly Lazy<IVenueRepository> _venueRepository;

    public UnitOfWork(OdkContext context, IPlatformProvider platformProvider)
    {
        _context = context;
        _platformProvider = platformProvider;

        _chapterAdminMemberRepository = new(() => new ChapterAdminMemberRepository(_context, _platformProvider));
        _chapterContactMessageReplyRepository = new(() => new ChapterContactMessageReplyRepository(_context));
        _chapterContactMessageRepository = new(() => new ChapterContactMessageRepository(_context));
        _chapterConversationMessageRepository = new(() => new ChapterConversationMessageRepository(_context));
        _chapterConversationRepository = new(() => new ChapterConversationRepository(_context));
        _chapterEmailRepository = new(() => new ChapterEmailRepository(_context));
        _chapterEventSettingsRepository = new(() => new ChapterEventSettingsRepository(_context));
        _chapterImageRepository = new(() => new ChapterImageRepository(_context));
        _chapterLinksRepository = new(() => new ChapterLinksRepository(_context));
        _chapterLocationRepository = new(() => new ChapterLocationRepository(_context));
        _chapterMembershipSettingsRepository = new(() => new ChapterMembershipSettingsRepository(_context));
        _chapterPageRepository = new(() => new ChapterPageRepository(_context));
        _chapterPaymentAccountRepository = new(() => new ChapterPaymentAccountRepository(_context));
        _chapterPaymentSettingsRepository = new(() => new ChapterPaymentSettingsRepository(_context));
        _chapterPrivacySettingsRepository = new(() => new ChapterPrivacySettingsRepository(_context));
        _chapterPropertyOptionRepository = new(() => new ChapterPropertyOptionRepository(_context));
        _chapterPropertyRepository = new(() => new ChapterPropertyRepository(_context));
        _chapterQuestionRepository = new(() => new ChapterQuestionRepository(_context));
        _chapterRepository = new(() => new ChapterRepository(_context, _platformProvider));
        _chapterSubscriptionRepository = new(() => new ChapterSubscriptionRepository(_context));
        _chapterTextsRepository = new(() => new ChapterTextsRepository(_context));
        _chapterTopicRepository = new(() => new ChapterTopicRepository(_context));
        _countryRepository = new(() => new CountryRepository(_context));
        _currencyRepository = new(() => new CurrencyRepository(_context));
        _distanceUnitRepository = new(() => new DistanceUnitRepository(_context));
        _emailRepository = new(() => new EmailRepository(_context));
        _errorPropertyRepository = new(() => new ErrorPropertyRepository(_context));
        _errorRepository = new(() => new ErrorRepository(_context));
        _eventCommentRepository = new(() => new EventCommentRepository(_context));
        _eventEmailRepository = new(() => new EventEmailRepository(_context));
        _eventHostRepository = new(() => new EventHostRepository(_context));
        _eventInviteRepository = new(() => new EventInviteRepository(_context));
        _eventRepository = new(() => new EventRepository(_context));
        _eventResponseRepository = new(() => new EventResponseRepository(_context));
        _eventTicketPaymentRepository = new(() => new EventTicketPaymentRepository(_context));
        _eventTicketSettingsRepository = new(() => new EventTicketSettingsRepository(_context));
        _eventTopicRepository = new(() => new EventTopicRepository(_context));
        _eventWaitlistMemberRepository = new(() => new EventWaitlistMemberRepository(_context));
        _featureRepository = new(() => new FeatureRepository(_context));
        _instagramFetchLogEntryRepository = new(() => new InstagramFetchLogEntryRepository(_context));
        _instagramImageRepository = new(() => new InstagramImageRepository(_context));
        _instagramPostRepository = new(() => new InstagramPostRepository(_context));
        _issueMessageRepository = new(() => new IssueMessageRepository(_context));
        _issueRepository = new(() => new IssueRepository(_context));
        _memberActivationTokenRepository = new(() => new MemberActivationTokenRepository(_context));
        _memberAvatarRepository = new(() => new MemberAvatarRepository(_context));
        _memberChapterRepository = new(() => new MemberChapterRepository(_context));
        _memberEmailAddressUpdateTokenRepository = new(() => new MemberEmailAddressUpdateTokenRepository(_context));
        _memberEmailPreferenceRepository = new(() => new MemberEmailPreferenceRepository(_context));
        _memberImageRepository = new(() => new MemberImageRepository(_context));
        _memberLocationRepository = new(() => new MemberLocationRepository(_context));
        _memberNotificationSettingsRepository = new(() => new MemberNotificationSettingsRepository(_context));
        _memberPasswordRepository = new(() => new MemberPasswordRepository(_context));
        _memberPasswordResetRequestRepository = new(() => new MemberPasswordResetRequestRepository(_context));
        _memberPaymentSettingsRepository = new(() => new MemberPaymentSettingsRepository(_context));
        _memberPreferencesRepository = new(() => new MemberPreferencesRepository(_context));
        _memberPropertyRepository = new(() => new MemberPropertyRepository(_context));
        _memberRepository = new(() => new MemberRepository(_context));
        _memberSiteSubscriptionRepository = new(() => new MemberSiteSubscriptionRepository(_context));
        _memberSubscriptionRecordRepository = new(() => new MemberSubscriptionRecordRepository(_context));
        _memberSubscriptionRepository = new(() => new MemberSubscriptionRepository(_context));
        _memberTopicRepository = new(() => new MemberTopicRepository(_context));
        _newChapterTopicRepository = new(() => new NewChapterTopicRepository(_context));
        _newMemberTopicRepository = new(() => new NewMemberTopicRepository(_context));
        _notificationRepository = new(() => new NotificationRepository(_context));
        _paymentCheckoutSessionRepository = new(() => new PaymentCheckoutSessionRepository(_context));
        _paymentProviderWebhookEventRepository = new(() => new PaymentProviderWebhookEventRepository(_context));
        _paymentRepository = new(() => new PaymentRepository(_context));
        _queuedEmailRecipientRepository = new(() => new QueuedEmailRecipientRepository(_context));
        _queuedEmailRepository = new(() => new QueuedEmailRepository(_context));
        _sentEmailEventRepository = new(() => new SentEmailEventRepository(context));
        _sentEmailRepository = new(() => new SentEmailRepository(_context));
        _siteContactMessageReplyRepository = new(() => new SiteContactMessageReplyRepository(_context));
        _siteContactMessageRepository = new(() => new SiteContactMessageRepository(_context));
        _siteEmailSettingsRepository = new(() => new SiteEmailSettingsRepository(_context));
        _sitePaymentSettingsRepository = new(() => new SitePaymentSettingsRepository(_context));
        _siteSubscriptionFeatureRepository = new(() => new SiteSubscriptionFeatureRepository(_context));
        _siteSubscriptionPriceRepository = new(() => new SiteSubscriptionPriceRepository(_context));
        _siteSubscriptionRepository = new(() => new SiteSubscriptionRepository(_context));
        _topicGroupRepository = new(() => new TopicGroupRepository(_context));
        _topicRepository = new(() => new TopicRepository(_context));
        _venueLocationRepository = new(() => new VenueLocationRepository(_context));
        _venueRepository = new(() => new VenueRepository(_context));
    }

    public IChapterAdminMemberRepository ChapterAdminMemberRepository => _chapterAdminMemberRepository.Value;
    public IChapterContactMessageReplyRepository ChapterContactMessageReplyRepository => _chapterContactMessageReplyRepository.Value;
    public IChapterContactMessageRepository ChapterContactMessageRepository => _chapterContactMessageRepository.Value;
    public IChapterConversationMessageRepository ChapterConversationMessageRepository => _chapterConversationMessageRepository.Value;
    public IChapterConversationRepository ChapterConversationRepository => _chapterConversationRepository.Value;
    public IChapterEmailRepository ChapterEmailRepository => _chapterEmailRepository.Value;
    public IChapterEventSettingsRepository ChapterEventSettingsRepository => _chapterEventSettingsRepository.Value;
    public IChapterImageRepository ChapterImageRepository => _chapterImageRepository.Value;
    public IChapterLinksRepository ChapterLinksRepository => _chapterLinksRepository.Value;
    public IChapterLocationRepository ChapterLocationRepository => _chapterLocationRepository.Value;
    public IChapterMembershipSettingsRepository ChapterMembershipSettingsRepository => _chapterMembershipSettingsRepository.Value;
    public IChapterPageRepository ChapterPageRepository => _chapterPageRepository.Value;
    public IChapterPaymentAccountRepository ChapterPaymentAccountRepository => _chapterPaymentAccountRepository.Value;
    public IChapterPaymentSettingsRepository ChapterPaymentSettingsRepository => _chapterPaymentSettingsRepository.Value;
    public IChapterPrivacySettingsRepository ChapterPrivacySettingsRepository => _chapterPrivacySettingsRepository.Value;
    public IChapterPropertyOptionRepository ChapterPropertyOptionRepository => _chapterPropertyOptionRepository.Value;
    public IChapterPropertyRepository ChapterPropertyRepository => _chapterPropertyRepository.Value;
    public IChapterQuestionRepository ChapterQuestionRepository => _chapterQuestionRepository.Value;
    public IChapterRepository ChapterRepository => _chapterRepository.Value;
    public IChapterSubscriptionRepository ChapterSubscriptionRepository => _chapterSubscriptionRepository.Value;
    public IChapterTextsRepository ChapterTextsRepository => _chapterTextsRepository.Value;
    public IChapterTopicRepository ChapterTopicRepository => _chapterTopicRepository.Value;
    public ICountryRepository CountryRepository => _countryRepository.Value;
    public ICurrencyRepository CurrencyRepository => _currencyRepository.Value;
    public IDistanceUnitRepository DistanceUnitRepository => _distanceUnitRepository.Value;
    public IEmailRepository EmailRepository => _emailRepository.Value;
    public IErrorPropertyRepository ErrorPropertyRepository => _errorPropertyRepository.Value;
    public IErrorRepository ErrorRepository => _errorRepository.Value;
    public IEventCommentRepository EventCommentRepository => _eventCommentRepository.Value;
    public IEventEmailRepository EventEmailRepository => _eventEmailRepository.Value;
    public IEventHostRepository EventHostRepository => _eventHostRepository.Value;
    public IEventInviteRepository EventInviteRepository => _eventInviteRepository.Value;
    public IEventRepository EventRepository => _eventRepository.Value;
    public IEventResponseRepository EventResponseRepository => _eventResponseRepository.Value;
    public IEventTicketPaymentRepository EventTicketPaymentRepository => _eventTicketPaymentRepository.Value;
    public IEventTicketSettingsRepository EventTicketSettingsRepository => _eventTicketSettingsRepository.Value;
    public IEventTopicRepository EventTopicRepository => _eventTopicRepository.Value;
    public IEventWaitlistMemberRepository EventWaitlistMemberRepository => _eventWaitlistMemberRepository.Value;
    public IFeatureRepository FeatureRepository => _featureRepository.Value;
    public IInstagramFetchLogEntryRepository InstagramFetchLogEntryRepository => _instagramFetchLogEntryRepository.Value;
    public IInstagramImageRepository InstagramImageRepository => _instagramImageRepository.Value;
    public IInstagramPostRepository InstagramPostRepository => _instagramPostRepository.Value;
    public IIssueMessageRepository IssueMessageRepository => _issueMessageRepository.Value;
    public IIssueRepository IssueRepository => _issueRepository.Value;
    public IMemberActivationTokenRepository MemberActivationTokenRepository => _memberActivationTokenRepository.Value;
    public IMemberAvatarRepository MemberAvatarRepository => _memberAvatarRepository.Value;
    public IMemberChapterRepository MemberChapterRepository => _memberChapterRepository.Value;
    public IMemberEmailAddressUpdateTokenRepository MemberEmailAddressUpdateTokenRepository => _memberEmailAddressUpdateTokenRepository.Value;
    public IMemberEmailPreferenceRepository MemberEmailPreferenceRepository => _memberEmailPreferenceRepository.Value;
    public IMemberImageRepository MemberImageRepository => _memberImageRepository.Value;
    public IMemberLocationRepository MemberLocationRepository => _memberLocationRepository.Value;
    public IMemberNotificationSettingsRepository MemberNotificationSettingsRepository => _memberNotificationSettingsRepository.Value;
    public IMemberPasswordRepository MemberPasswordRepository => _memberPasswordRepository.Value;
    public IMemberPasswordResetRequestRepository MemberPasswordResetRequestRepository => _memberPasswordResetRequestRepository.Value;
    public IMemberPaymentSettingsRepository MemberPaymentSettingsRepository => _memberPaymentSettingsRepository.Value;
    public IMemberPreferencesRepository MemberPreferencesRepository => _memberPreferencesRepository.Value;
    public IMemberPropertyRepository MemberPropertyRepository => _memberPropertyRepository.Value;
    public IMemberRepository MemberRepository => _memberRepository.Value;
    public IMemberSiteSubscriptionRepository MemberSiteSubscriptionRepository => _memberSiteSubscriptionRepository.Value;
    public IMemberSubscriptionRecordRepository MemberSubscriptionRecordRepository => _memberSubscriptionRecordRepository.Value;
    public IMemberSubscriptionRepository MemberSubscriptionRepository => _memberSubscriptionRepository.Value;
    public IMemberTopicRepository MemberTopicRepository => _memberTopicRepository.Value;
    public INewChapterTopicRepository NewChapterTopicRepository => _newChapterTopicRepository.Value;
    public INewMemberTopicRepository NewMemberTopicRepository => _newMemberTopicRepository.Value;
    public INotificationRepository NotificationRepository => _notificationRepository.Value;
    public IPaymentCheckoutSessionRepository PaymentCheckoutSessionRepository => _paymentCheckoutSessionRepository.Value;
    public IPaymentProviderWebhookEventRepository PaymentProviderWebhookEventRepository => _paymentProviderWebhookEventRepository.Value;
    public IPaymentRepository PaymentRepository => _paymentRepository.Value;
    public IQueuedEmailRecipientRepository QueuedEmailRecipientRepository => _queuedEmailRecipientRepository.Value;
    public IQueuedEmailRepository QueuedEmailRepository => _queuedEmailRepository.Value;
    public ISentEmailEventRepository SentEmailEventRepository => _sentEmailEventRepository.Value;
    public ISentEmailRepository SentEmailRepository => _sentEmailRepository.Value;
    public ISiteContactMessageReplyRepository SiteContactMessageReplyRepository => _siteContactMessageReplyRepository.Value;
    public ISiteContactMessageRepository SiteContactMessageRepository => _siteContactMessageRepository.Value;
    public ISiteEmailSettingsRepository SiteEmailSettingsRepository => _siteEmailSettingsRepository.Value;
    public ISitePaymentSettingsRepository SitePaymentSettingsRepository => _sitePaymentSettingsRepository.Value;
    public ISiteSubscriptionFeatureRepository SiteSubscriptionFeatureRepository => _siteSubscriptionFeatureRepository.Value;
    public ISiteSubscriptionPriceRepository SiteSubscriptionPriceRepository => _siteSubscriptionPriceRepository.Value;
    public ISiteSubscriptionRepository SiteSubscriptionRepository => _siteSubscriptionRepository.Value;
    public ITopicGroupRepository TopicGroupRepository => _topicGroupRepository.Value;
    public ITopicRepository TopicRepository => _topicRepository.Value;
    public IVenueLocationRepository VenueLocationRepository => _venueLocationRepository.Value;
    public IVenueRepository VenueRepository => _venueRepository.Value;

    public async Task<(T1, T2)> RunAsync<T1, T2>(
        Func<IUnitOfWork, IDeferredQuery<T1>> query1,
        Func<IUnitOfWork, IDeferredQuery<T2>> query2)
    {
        var q1 = query1(this);
        var q2 = query2(this);
        return (
            await q1.Run(),
            await q2.Run());
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
            await q1.Run(),
            await q2.Run(),
            await q3.Run());
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
            await q1.Run(),
            await q2.Run(),
            await q3.Run(),
            await q4.Run());
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
            await q1.Run(),
            await q2.Run(),
            await q3.Run(),
            await q4.Run(),
            await q5.Run());
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
            await q1.Run(),
            await q2.Run(),
            await q3.Run(),
            await q4.Run(),
            await q5.Run(),
            await q6.Run());
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
            await q1.Run(),
            await q2.Run(),
            await q3.Run(),
            await q4.Run(),
            await q5.Run(),
            await q6.Run(),
            await q7.Run());
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
            await q1.Run(),
            await q2.Run(),
            await q3.Run(),
            await q4.Run(),
            await q5.Run(),
            await q6.Run(),
            await q7.Run(),
            await q8.Run());
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
            await q1.Run(),
            await q2.Run(),
            await q3.Run(),
            await q4.Run(),
            await q5.Run(),
            await q6.Run(),
            await q7.Run(),
            await q8.Run(),
            await q9.Run());
    }

    public async Task<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10)> RunAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
        Func<IUnitOfWork, IDeferredQuery<T1>> query1,
        Func<IUnitOfWork, IDeferredQuery<T2>> query2,
        Func<IUnitOfWork, IDeferredQuery<T3>> query3,
        Func<IUnitOfWork, IDeferredQuery<T4>> query4,
        Func<IUnitOfWork, IDeferredQuery<T5>> query5,
        Func<IUnitOfWork, IDeferredQuery<T6>> query6,
        Func<IUnitOfWork, IDeferredQuery<T7>> query7,
        Func<IUnitOfWork, IDeferredQuery<T8>> query8,
        Func<IUnitOfWork, IDeferredQuery<T9>> query9,
        Func<IUnitOfWork, IDeferredQuery<T10>> query10)
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
        var q10 = query10(this);

        return (
            await q1.Run(),
            await q2.Run(),
            await q3.Run(),
            await q4.Run(),
            await q5.Run(),
            await q6.Run(),
            await q7.Run(),
            await q8.Run(),
            await q9.Run(),
            await q10.Run());
    }

    public async Task<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11)> RunAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(
        Func<IUnitOfWork, IDeferredQuery<T1>> query1,
        Func<IUnitOfWork, IDeferredQuery<T2>> query2,
        Func<IUnitOfWork, IDeferredQuery<T3>> query3,
        Func<IUnitOfWork, IDeferredQuery<T4>> query4,
        Func<IUnitOfWork, IDeferredQuery<T5>> query5,
        Func<IUnitOfWork, IDeferredQuery<T6>> query6,
        Func<IUnitOfWork, IDeferredQuery<T7>> query7,
        Func<IUnitOfWork, IDeferredQuery<T8>> query8,
        Func<IUnitOfWork, IDeferredQuery<T9>> query9,
        Func<IUnitOfWork, IDeferredQuery<T10>> query10,
        Func<IUnitOfWork, IDeferredQuery<T11>> query11)
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
        var q10 = query10(this);
        var q11 = query11(this);

        return (
            await q1.Run(),
            await q2.Run(),
            await q3.Run(),
            await q4.Run(),
            await q5.Run(),
            await q6.Run(),
            await q7.Run(),
            await q8.Run(),
            await q9.Run(),
            await q10.Run(),
            await q11.Run());
    }

    public async Task<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12)> RunAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(
        Func<IUnitOfWork, IDeferredQuery<T1>> query1,
        Func<IUnitOfWork, IDeferredQuery<T2>> query2,
        Func<IUnitOfWork, IDeferredQuery<T3>> query3,
        Func<IUnitOfWork, IDeferredQuery<T4>> query4,
        Func<IUnitOfWork, IDeferredQuery<T5>> query5,
        Func<IUnitOfWork, IDeferredQuery<T6>> query6,
        Func<IUnitOfWork, IDeferredQuery<T7>> query7,
        Func<IUnitOfWork, IDeferredQuery<T8>> query8,
        Func<IUnitOfWork, IDeferredQuery<T9>> query9,
        Func<IUnitOfWork, IDeferredQuery<T10>> query10,
        Func<IUnitOfWork, IDeferredQuery<T11>> query11,
        Func<IUnitOfWork, IDeferredQuery<T12>> query12)
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
        var q10 = query10(this);
        var q11 = query11(this);
        var q12 = query12(this);

        return (
            await q1.Run(),
            await q2.Run(),
            await q3.Run(),
            await q4.Run(),
            await q5.Run(),
            await q6.Run(),
            await q7.Run(),
            await q8.Run(),
            await q9.Run(),
            await q10.Run(),
            await q11.Run(),
            await q12.Run());
    }

    public async Task<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13)> RunAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(
        Func<IUnitOfWork, IDeferredQuery<T1>> query1,
        Func<IUnitOfWork, IDeferredQuery<T2>> query2,
        Func<IUnitOfWork, IDeferredQuery<T3>> query3,
        Func<IUnitOfWork, IDeferredQuery<T4>> query4,
        Func<IUnitOfWork, IDeferredQuery<T5>> query5,
        Func<IUnitOfWork, IDeferredQuery<T6>> query6,
        Func<IUnitOfWork, IDeferredQuery<T7>> query7,
        Func<IUnitOfWork, IDeferredQuery<T8>> query8,
        Func<IUnitOfWork, IDeferredQuery<T9>> query9,
        Func<IUnitOfWork, IDeferredQuery<T10>> query10,
        Func<IUnitOfWork, IDeferredQuery<T11>> query11,
        Func<IUnitOfWork, IDeferredQuery<T12>> query12,
        Func<IUnitOfWork, IDeferredQuery<T13>> query13)
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
        var q10 = query10(this);
        var q11 = query11(this);
        var q12 = query12(this);
        var q13 = query13(this);

        return (
            await q1.Run(),
            await q2.Run(),
            await q3.Run(),
            await q4.Run(),
            await q5.Run(),
            await q6.Run(),
            await q7.Run(),
            await q8.Run(),
            await q9.Run(),
            await q10.Run(),
            await q11.Run(),
            await q12.Run(),
            await q13.Run());
    }

    public async Task<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14)> RunAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(
        Func<IUnitOfWork, IDeferredQuery<T1>> query1,
        Func<IUnitOfWork, IDeferredQuery<T2>> query2,
        Func<IUnitOfWork, IDeferredQuery<T3>> query3,
        Func<IUnitOfWork, IDeferredQuery<T4>> query4,
        Func<IUnitOfWork, IDeferredQuery<T5>> query5,
        Func<IUnitOfWork, IDeferredQuery<T6>> query6,
        Func<IUnitOfWork, IDeferredQuery<T7>> query7,
        Func<IUnitOfWork, IDeferredQuery<T8>> query8,
        Func<IUnitOfWork, IDeferredQuery<T9>> query9,
        Func<IUnitOfWork, IDeferredQuery<T10>> query10,
        Func<IUnitOfWork, IDeferredQuery<T11>> query11,
        Func<IUnitOfWork, IDeferredQuery<T12>> query12,
        Func<IUnitOfWork, IDeferredQuery<T13>> query13,
        Func<IUnitOfWork, IDeferredQuery<T14>> query14)
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
        var q10 = query10(this);
        var q11 = query11(this);
        var q12 = query12(this);
        var q13 = query13(this);
        var q14 = query14(this);

        return (
            await q1.Run(),
            await q2.Run(),
            await q3.Run(),
            await q4.Run(),
            await q5.Run(),
            await q6.Run(),
            await q7.Run(),
            await q8.Run(),
            await q9.Run(),
            await q10.Run(),
            await q11.Run(),
            await q12.Run(),
            await q13.Run(),
            await q14.Run());
    }

    public async Task<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15)> RunAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(
        Func<IUnitOfWork, IDeferredQuery<T1>> query1,
        Func<IUnitOfWork, IDeferredQuery<T2>> query2,
        Func<IUnitOfWork, IDeferredQuery<T3>> query3,
        Func<IUnitOfWork, IDeferredQuery<T4>> query4,
        Func<IUnitOfWork, IDeferredQuery<T5>> query5,
        Func<IUnitOfWork, IDeferredQuery<T6>> query6,
        Func<IUnitOfWork, IDeferredQuery<T7>> query7,
        Func<IUnitOfWork, IDeferredQuery<T8>> query8,
        Func<IUnitOfWork, IDeferredQuery<T9>> query9,
        Func<IUnitOfWork, IDeferredQuery<T10>> query10,
        Func<IUnitOfWork, IDeferredQuery<T11>> query11,
        Func<IUnitOfWork, IDeferredQuery<T12>> query12,
        Func<IUnitOfWork, IDeferredQuery<T13>> query13,
        Func<IUnitOfWork, IDeferredQuery<T14>> query14,
        Func<IUnitOfWork, IDeferredQuery<T15>> query15)
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
        var q10 = query10(this);
        var q11 = query11(this);
        var q12 = query12(this);
        var q13 = query13(this);
        var q14 = query14(this);
        var q15 = query15(this);

        return (
            await q1.Run(),
            await q2.Run(),
            await q3.Run(),
            await q4.Run(),
            await q5.Run(),
            await q6.Run(),
            await q7.Run(),
            await q8.Run(),
            await q9.Run(),
            await q10.Run(),
            await q11.Run(),
            await q12.Run(),
            await q13.Run(),
            await q14.Run(),
            await q15.Run());
    }

    public async Task<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16)> RunAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(
        Func<IUnitOfWork, IDeferredQuery<T1>> query1,
        Func<IUnitOfWork, IDeferredQuery<T2>> query2,
        Func<IUnitOfWork, IDeferredQuery<T3>> query3,
        Func<IUnitOfWork, IDeferredQuery<T4>> query4,
        Func<IUnitOfWork, IDeferredQuery<T5>> query5,
        Func<IUnitOfWork, IDeferredQuery<T6>> query6,
        Func<IUnitOfWork, IDeferredQuery<T7>> query7,
        Func<IUnitOfWork, IDeferredQuery<T8>> query8,
        Func<IUnitOfWork, IDeferredQuery<T9>> query9,
        Func<IUnitOfWork, IDeferredQuery<T10>> query10,
        Func<IUnitOfWork, IDeferredQuery<T11>> query11,
        Func<IUnitOfWork, IDeferredQuery<T12>> query12,
        Func<IUnitOfWork, IDeferredQuery<T13>> query13,
        Func<IUnitOfWork, IDeferredQuery<T14>> query14,
        Func<IUnitOfWork, IDeferredQuery<T15>> query15,
        Func<IUnitOfWork, IDeferredQuery<T16>> query16)
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
        var q10 = query10(this);
        var q11 = query11(this);
        var q12 = query12(this);
        var q13 = query13(this);
        var q14 = query14(this);
        var q15 = query15(this);
        var q16 = query16(this);

        return (
            await q1.Run(),
            await q2.Run(),
            await q3.Run(),
            await q4.Run(),
            await q5.Run(),
            await q6.Run(),
            await q7.Run(),
            await q8.Run(),
            await q9.Run(),
            await q10.Run(),
            await q11.Run(),
            await q12.Run(),
            await q13.Run(),
            await q14.Run(),
            await q15.Run(),
            await q16.Run());
    }

    public async Task<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17)> RunAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17>(
        Func<IUnitOfWork, IDeferredQuery<T1>> query1,
        Func<IUnitOfWork, IDeferredQuery<T2>> query2,
        Func<IUnitOfWork, IDeferredQuery<T3>> query3,
        Func<IUnitOfWork, IDeferredQuery<T4>> query4,
        Func<IUnitOfWork, IDeferredQuery<T5>> query5,
        Func<IUnitOfWork, IDeferredQuery<T6>> query6,
        Func<IUnitOfWork, IDeferredQuery<T7>> query7,
        Func<IUnitOfWork, IDeferredQuery<T8>> query8,
        Func<IUnitOfWork, IDeferredQuery<T9>> query9,
        Func<IUnitOfWork, IDeferredQuery<T10>> query10,
        Func<IUnitOfWork, IDeferredQuery<T11>> query11,
        Func<IUnitOfWork, IDeferredQuery<T12>> query12,
        Func<IUnitOfWork, IDeferredQuery<T13>> query13,
        Func<IUnitOfWork, IDeferredQuery<T14>> query14,
        Func<IUnitOfWork, IDeferredQuery<T15>> query15,
        Func<IUnitOfWork, IDeferredQuery<T16>> query16,
        Func<IUnitOfWork, IDeferredQuery<T17>> query17)
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
        var q10 = query10(this);
        var q11 = query11(this);
        var q12 = query12(this);
        var q13 = query13(this);
        var q14 = query14(this);
        var q15 = query15(this);
        var q16 = query16(this);
        var q17 = query17(this);

        return (
            await q1.Run(),
            await q2.Run(),
            await q3.Run(),
            await q4.Run(),
            await q5.Run(),
            await q6.Run(),
            await q7.Run(),
            await q8.Run(),
            await q9.Run(),
            await q10.Run(),
            await q11.Run(),
            await q12.Run(),
            await q13.Run(),
            await q14.Run(),
            await q15.Run(),
            await q16.Run(),
            await q17.Run());
    }

    public async Task<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18)> RunAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18>(
        Func<IUnitOfWork, IDeferredQuery<T1>> query1,
        Func<IUnitOfWork, IDeferredQuery<T2>> query2,
        Func<IUnitOfWork, IDeferredQuery<T3>> query3,
        Func<IUnitOfWork, IDeferredQuery<T4>> query4,
        Func<IUnitOfWork, IDeferredQuery<T5>> query5,
        Func<IUnitOfWork, IDeferredQuery<T6>> query6,
        Func<IUnitOfWork, IDeferredQuery<T7>> query7,
        Func<IUnitOfWork, IDeferredQuery<T8>> query8,
        Func<IUnitOfWork, IDeferredQuery<T9>> query9,
        Func<IUnitOfWork, IDeferredQuery<T10>> query10,
        Func<IUnitOfWork, IDeferredQuery<T11>> query11,
        Func<IUnitOfWork, IDeferredQuery<T12>> query12,
        Func<IUnitOfWork, IDeferredQuery<T13>> query13,
        Func<IUnitOfWork, IDeferredQuery<T14>> query14,
        Func<IUnitOfWork, IDeferredQuery<T15>> query15,
        Func<IUnitOfWork, IDeferredQuery<T16>> query16,
        Func<IUnitOfWork, IDeferredQuery<T17>> query17,
        Func<IUnitOfWork, IDeferredQuery<T18>> query18)
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
        var q10 = query10(this);
        var q11 = query11(this);
        var q12 = query12(this);
        var q13 = query13(this);
        var q14 = query14(this);
        var q15 = query15(this);
        var q16 = query16(this);
        var q17 = query17(this);
        var q18 = query18(this);

        return (
            await q1.Run(),
            await q2.Run(),
            await q3.Run(),
            await q4.Run(),
            await q5.Run(),
            await q6.Run(),
            await q7.Run(),
            await q8.Run(),
            await q9.Run(),
            await q10.Run(),
            await q11.Run(),
            await q12.Run(),
            await q13.Run(),
            await q14.Run(),
            await q15.Run(),
            await q16.Run(),
            await q17.Run(),
            await q18.Run());
    }

    public Task SaveChangesAsync() => _context.SaveChangesAsync();
}