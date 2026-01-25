using ODK.Data.Core.Deferred;
using ODK.Data.Core.Repositories;

namespace ODK.Data.Core;

public interface IUnitOfWork
{
    IChapterAdminMemberRepository ChapterAdminMemberRepository { get; }
    IChapterContactMessageReplyRepository ChapterContactMessageReplyRepository { get; }
    IChapterContactMessageRepository ChapterContactMessageRepository { get; }
    IChapterConversationMessageRepository ChapterConversationMessageRepository { get; }
    IChapterConversationRepository ChapterConversationRepository { get; }
    IChapterEmailRepository ChapterEmailRepository { get; }
    IChapterEventSettingsRepository ChapterEventSettingsRepository { get; }
    IChapterImageRepository ChapterImageRepository { get; }
    IChapterLinksRepository ChapterLinksRepository { get; }
    IChapterLocationRepository ChapterLocationRepository { get; }
    IChapterMembershipSettingsRepository ChapterMembershipSettingsRepository { get; }
    IChapterPageRepository ChapterPageRepository { get; }
    IChapterPaymentAccountRepository ChapterPaymentAccountRepository { get; }
    IChapterPaymentSettingsRepository ChapterPaymentSettingsRepository { get; }
    IChapterPrivacySettingsRepository ChapterPrivacySettingsRepository { get; }
    IChapterPropertyOptionRepository ChapterPropertyOptionRepository { get; }
    IChapterPropertyRepository ChapterPropertyRepository { get; }
    IChapterQuestionRepository ChapterQuestionRepository { get; }
    IChapterRepository ChapterRepository { get; }
    IChapterSubscriptionRepository ChapterSubscriptionRepository { get; }
    IChapterTextsRepository ChapterTextsRepository { get; }
    IChapterTopicRepository ChapterTopicRepository { get; }
    ICountryRepository CountryRepository { get; }
    ICurrencyRepository CurrencyRepository { get; }
    IDistanceUnitRepository DistanceUnitRepository { get; }
    IEmailRepository EmailRepository { get; }
    IErrorPropertyRepository ErrorPropertyRepository { get; }
    IErrorRepository ErrorRepository { get; }
    IEventCommentRepository EventCommentRepository { get; }
    IEventEmailRepository EventEmailRepository { get; }
    IEventHostRepository EventHostRepository { get; }
    IEventInviteRepository EventInviteRepository { get; }
    IEventRepository EventRepository { get; }
    IEventResponseRepository EventResponseRepository { get; }
    IEventTicketPaymentRepository EventTicketPaymentRepository { get; }
    IEventTicketSettingsRepository EventTicketSettingsRepository { get; }
    IEventTopicRepository EventTopicRepository { get; }
    IFeatureRepository FeatureRepository { get; }
    IInstagramFetchLogEntryRepository InstagramFetchLogEntryRepository { get; }
    IInstagramImageRepository InstagramImageRepository { get; }
    IInstagramPostRepository InstagramPostRepository { get; }
    IIssueMessageRepository IssueMessageRepository { get; }
    IIssueRepository IssueRepository { get; }
    IMemberActivationTokenRepository MemberActivationTokenRepository { get; }
    IMemberAvatarRepository MemberAvatarRepository { get; }
    IMemberChapterRepository MemberChapterRepository { get; }
    IMemberEmailAddressUpdateTokenRepository MemberEmailAddressUpdateTokenRepository { get; }
    IMemberEmailPreferenceRepository MemberEmailPreferenceRepository { get; }
    IMemberImageRepository MemberImageRepository { get; }
    IMemberLocationRepository MemberLocationRepository { get; }
    IMemberNotificationSettingsRepository MemberNotificationSettingsRepository { get; }
    IMemberPasswordRepository MemberPasswordRepository { get; }
    IMemberPasswordResetRequestRepository MemberPasswordResetRequestRepository { get; }
    IMemberPaymentSettingsRepository MemberPaymentSettingsRepository { get; }
    IMemberPreferencesRepository MemberPreferencesRepository { get; }
    IMemberPrivacySettingsRepository MemberPrivacySettingsRepository { get; }
    IMemberPropertyRepository MemberPropertyRepository { get; }
    IMemberRepository MemberRepository { get; }
    IMemberSiteSubscriptionRepository MemberSiteSubscriptionRepository { get; }
    IMemberSubscriptionRecordRepository MemberSubscriptionRecordRepository { get; }
    IMemberSubscriptionRepository MemberSubscriptionRepository { get; }
    IMemberTopicRepository MemberTopicRepository { get; }
    INewChapterTopicRepository NewChapterTopicRepository { get; }
    INewMemberTopicRepository NewMemberTopicRepository { get; }
    INotificationRepository NotificationRepository { get; }
    IPaymentCheckoutSessionRepository PaymentCheckoutSessionRepository { get; }
    IPaymentProviderWebhookEventRepository PaymentProviderWebhookEventRepository { get; }
    IPaymentRepository PaymentRepository { get; }
    IQueuedEmailRecipientRepository QueuedEmailRecipientRepository { get; }
    IQueuedEmailRepository QueuedEmailRepository { get; }
    ISentEmailEventRepository SentEmailEventRepository { get; }
    ISentEmailRepository SentEmailRepository { get; }
    ISiteContactMessageReplyRepository SiteContactMessageReplyRepository { get; }
    ISiteContactMessageRepository SiteContactMessageRepository { get; }
    ISiteEmailSettingsRepository SiteEmailSettingsRepository { get; }
    ISitePaymentSettingsRepository SitePaymentSettingsRepository { get; }
    ISiteSubscriptionFeatureRepository SiteSubscriptionFeatureRepository { get; }
    ISiteSubscriptionPriceRepository SiteSubscriptionPriceRepository { get; }
    ISiteSubscriptionRepository SiteSubscriptionRepository { get; }
    ITopicGroupRepository TopicGroupRepository { get; }
    ITopicRepository TopicRepository { get; }
    IVenueLocationRepository VenueLocationRepository { get; }
    IVenueRepository VenueRepository { get; }

    Task<(T1, T2)> RunAsync<T1, T2>(
        Func<IUnitOfWork, IDeferredQuery<T1>> query1,
        Func<IUnitOfWork, IDeferredQuery<T2>> query2);

    Task<(T1, T2, T3)> RunAsync<T1, T2, T3>(
        Func<IUnitOfWork, IDeferredQuery<T1>> query1,
        Func<IUnitOfWork, IDeferredQuery<T2>> query2,
        Func<IUnitOfWork, IDeferredQuery<T3>> query3);

    Task<(T1, T2, T3, T4)> RunAsync<T1, T2, T3, T4>(
        Func<IUnitOfWork, IDeferredQuery<T1>> query1,
        Func<IUnitOfWork, IDeferredQuery<T2>> query2,
        Func<IUnitOfWork, IDeferredQuery<T3>> query3,
        Func<IUnitOfWork, IDeferredQuery<T4>> query4);

    Task<(T1, T2, T3, T4, T5)> RunAsync<T1, T2, T3, T4, T5>(
        Func<IUnitOfWork, IDeferredQuery<T1>> query1,
        Func<IUnitOfWork, IDeferredQuery<T2>> query2,
        Func<IUnitOfWork, IDeferredQuery<T3>> query3,
        Func<IUnitOfWork, IDeferredQuery<T4>> query4,
        Func<IUnitOfWork, IDeferredQuery<T5>> query5);

    Task<(T1, T2, T3, T4, T5, T6)> RunAsync<T1, T2, T3, T4, T5, T6>(
        Func<IUnitOfWork, IDeferredQuery<T1>> query1,
        Func<IUnitOfWork, IDeferredQuery<T2>> query2,
        Func<IUnitOfWork, IDeferredQuery<T3>> query3,
        Func<IUnitOfWork, IDeferredQuery<T4>> query4,
        Func<IUnitOfWork, IDeferredQuery<T5>> query5,
        Func<IUnitOfWork, IDeferredQuery<T6>> query6);

    Task<(T1, T2, T3, T4, T5, T6, T7)> RunAsync<T1, T2, T3, T4, T5, T6, T7>(
        Func<IUnitOfWork, IDeferredQuery<T1>> query1,
        Func<IUnitOfWork, IDeferredQuery<T2>> query2,
        Func<IUnitOfWork, IDeferredQuery<T3>> query3,
        Func<IUnitOfWork, IDeferredQuery<T4>> query4,
        Func<IUnitOfWork, IDeferredQuery<T5>> query5,
        Func<IUnitOfWork, IDeferredQuery<T6>> query6,
        Func<IUnitOfWork, IDeferredQuery<T7>> query7);

    Task<(T1, T2, T3, T4, T5, T6, T7, T8)> RunAsync<T1, T2, T3, T4, T5, T6, T7, T8>(
        Func<IUnitOfWork, IDeferredQuery<T1>> query1,
        Func<IUnitOfWork, IDeferredQuery<T2>> query2,
        Func<IUnitOfWork, IDeferredQuery<T3>> query3,
        Func<IUnitOfWork, IDeferredQuery<T4>> query4,
        Func<IUnitOfWork, IDeferredQuery<T5>> query5,
        Func<IUnitOfWork, IDeferredQuery<T6>> query6,
        Func<IUnitOfWork, IDeferredQuery<T7>> query7,
        Func<IUnitOfWork, IDeferredQuery<T8>> query8);

    Task<(T1, T2, T3, T4, T5, T6, T7, T8, T9)> RunAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9>(
        Func<IUnitOfWork, IDeferredQuery<T1>> query1,
        Func<IUnitOfWork, IDeferredQuery<T2>> query2,
        Func<IUnitOfWork, IDeferredQuery<T3>> query3,
        Func<IUnitOfWork, IDeferredQuery<T4>> query4,
        Func<IUnitOfWork, IDeferredQuery<T5>> query5,
        Func<IUnitOfWork, IDeferredQuery<T6>> query6,
        Func<IUnitOfWork, IDeferredQuery<T7>> query7,
        Func<IUnitOfWork, IDeferredQuery<T8>> query8,
        Func<IUnitOfWork, IDeferredQuery<T9>> query9);

    Task<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10)> RunAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(
        Func<IUnitOfWork, IDeferredQuery<T1>> query1,
        Func<IUnitOfWork, IDeferredQuery<T2>> query2,
        Func<IUnitOfWork, IDeferredQuery<T3>> query3,
        Func<IUnitOfWork, IDeferredQuery<T4>> query4,
        Func<IUnitOfWork, IDeferredQuery<T5>> query5,
        Func<IUnitOfWork, IDeferredQuery<T6>> query6,
        Func<IUnitOfWork, IDeferredQuery<T7>> query7,
        Func<IUnitOfWork, IDeferredQuery<T8>> query8,
        Func<IUnitOfWork, IDeferredQuery<T9>> query9,
        Func<IUnitOfWork, IDeferredQuery<T10>> query10);

    Task<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11)> RunAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(
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
        Func<IUnitOfWork, IDeferredQuery<T11>> query11);

    Task<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12)> RunAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(
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
        Func<IUnitOfWork, IDeferredQuery<T12>> query12);

    Task<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13)> RunAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(
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
        Func<IUnitOfWork, IDeferredQuery<T13>> query13);

    Task<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14)> RunAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(
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
        Func<IUnitOfWork, IDeferredQuery<T14>> query14);

    Task<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15)> RunAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(
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
        Func<IUnitOfWork, IDeferredQuery<T15>> query15);

    Task<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16)> RunAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(
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
        Func<IUnitOfWork, IDeferredQuery<T16>> query16);

    Task<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17)> RunAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17>(
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
        Func<IUnitOfWork, IDeferredQuery<T17>> query17);

    Task<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18)> RunAsync<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18>(
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
        Func<IUnitOfWork, IDeferredQuery<T18>> query18);

    Task SaveChangesAsync();
}