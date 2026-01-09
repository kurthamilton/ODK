using System;
using System.Threading.Tasks;
using Moq;
using ODK.Core.Settings;
using ODK.Core.SocialMedia;
using ODK.Data.Core;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Repositories;

namespace ODK.Services.Tests.Helpers;

internal class MockUnitOfWork : IUnitOfWork
{
    public MockUnitOfWork(Mock<IUnitOfWork> mock)
    {
        Mock = mock;
    }

    public Mock<IUnitOfWork> Mock { get; }

    public IChapterAdminMemberRepository ChapterAdminMemberRepository => Mock.Object.ChapterAdminMemberRepository;
    public IChapterContactMessageReplyRepository ChapterContactMessageReplyRepository => Mock.Object.ChapterContactMessageReplyRepository;
    public IChapterContactMessageRepository ChapterContactMessageRepository => Mock.Object.ChapterContactMessageRepository;
    public IChapterConversationMessageRepository ChapterConversationMessageRepository => Mock.Object.ChapterConversationMessageRepository;
    public IChapterConversationRepository ChapterConversationRepository => Mock.Object.ChapterConversationRepository;
    public IChapterEmailRepository ChapterEmailRepository => Mock.Object.ChapterEmailRepository;
    public IChapterEventSettingsRepository ChapterEventSettingsRepository => Mock.Object.ChapterEventSettingsRepository;
    public IChapterImageRepository ChapterImageRepository => Mock.Object.ChapterImageRepository;
    public IChapterLinksRepository ChapterLinksRepository => Mock.Object.ChapterLinksRepository;
    public IChapterLocationRepository ChapterLocationRepository => Mock.Object.ChapterLocationRepository;
    public IChapterMembershipSettingsRepository ChapterMembershipSettingsRepository => Mock.Object.ChapterMembershipSettingsRepository;
    public IChapterPageRepository ChapterPageRepository => Mock.Object.ChapterPageRepository;
    public IChapterPaymentAccountRepository ChapterPaymentAccountRepository => Mock.Object.ChapterPaymentAccountRepository;
    public IChapterPaymentSettingsRepository ChapterPaymentSettingsRepository => Mock.Object.ChapterPaymentSettingsRepository;
    public IChapterPrivacySettingsRepository ChapterPrivacySettingsRepository => Mock.Object.ChapterPrivacySettingsRepository;
    public IChapterPropertyOptionRepository ChapterPropertyOptionRepository => Mock.Object.ChapterPropertyOptionRepository;
    public IChapterPropertyRepository ChapterPropertyRepository => Mock.Object.ChapterPropertyRepository;
    public IChapterQuestionRepository ChapterQuestionRepository => Mock.Object.ChapterQuestionRepository;
    public IChapterRepository ChapterRepository => Mock.Object.ChapterRepository;
    public IChapterSubscriptionRepository ChapterSubscriptionRepository => Mock.Object.ChapterSubscriptionRepository;
    public IChapterTopicRepository ChapterTopicRepository => Mock.Object.ChapterTopicRepository;
    public ICountryRepository CountryRepository => Mock.Object.CountryRepository;
    public IChapterTextsRepository ChapterTextsRepository => Mock.Object.ChapterTextsRepository;
    public ICurrencyRepository CurrencyRepository => Mock.Object.CurrencyRepository;
    public IDistanceUnitRepository DistanceUnitRepository => Mock.Object.DistanceUnitRepository;
    public IEmailRepository EmailRepository => Mock.Object.EmailRepository;
    public IErrorPropertyRepository ErrorPropertyRepository => Mock.Object.ErrorPropertyRepository;
    public IErrorRepository ErrorRepository => Mock.Object.ErrorRepository;
    public IEventCommentRepository EventCommentRepository => Mock.Object.EventCommentRepository;
    public IEventEmailRepository EventEmailRepository => Mock.Object.EventEmailRepository;
    public IEventHostRepository EventHostRepository => Mock.Object.EventHostRepository;
    public IEventInviteRepository EventInviteRepository => Mock.Object.EventInviteRepository;
    public IEventRepository EventRepository => Mock.Object.EventRepository;
    public IEventResponseRepository EventResponseRepository => Mock.Object.EventResponseRepository;
    public IEventTicketPurchaseRepository EventTicketPurchaseRepository => Mock.Object.EventTicketPurchaseRepository;
    public IEventTicketSettingsRepository EventTicketSettingsRepository => Mock.Object.EventTicketSettingsRepository;
    public IEventTopicRepository EventTopicRepository => Mock.Object.EventTopicRepository;
    public IFeatureRepository FeatureRepository => Mock.Object.FeatureRepository;
    public IInstagramImageRepository InstagramImageRepository => Mock.Object.InstagramImageRepository;
    public IInstagramPostRepository InstagramPostRepository => Mock.Object.InstagramPostRepository;
    public IIssueMessageRepository IssueMessageRepository => Mock.Object.IssueMessageRepository;
    public IIssueRepository IssueRepository => Mock.Object.IssueRepository;
    public IMemberActivationTokenRepository MemberActivationTokenRepository => Mock.Object.MemberActivationTokenRepository;
    public IMemberAvatarRepository MemberAvatarRepository => Mock.Object.MemberAvatarRepository;
    public IMemberChapterRepository MemberChapterRepository => Mock.Object.MemberChapterRepository;
    public IMemberEmailAddressUpdateTokenRepository MemberEmailAddressUpdateTokenRepository => Mock.Object.MemberEmailAddressUpdateTokenRepository;
    public IMemberEmailPreferenceRepository MemberEmailPreferenceRepository => Mock.Object.MemberEmailPreferenceRepository;
    public IMemberImageRepository MemberImageRepository => Mock.Object.MemberImageRepository;
    public IMemberLocationRepository MemberLocationRepository => Mock.Object.MemberLocationRepository;
    public IMemberNotificationSettingsRepository MemberNotificationSettingsRepository => Mock.Object.MemberNotificationSettingsRepository;
    public IMemberPasswordRepository MemberPasswordRepository => Mock.Object.MemberPasswordRepository;
    public IMemberPasswordResetRequestRepository MemberPasswordResetRequestRepository => Mock.Object.MemberPasswordResetRequestRepository;
    public IMemberPaymentSettingsRepository MemberPaymentSettingsRepository => Mock.Object.MemberPaymentSettingsRepository;
    public IMemberPreferencesRepository MemberPreferencesRepository => Mock.Object.MemberPreferencesRepository;
    public IMemberPrivacySettingsRepository MemberPrivacySettingsRepository => Mock.Object.MemberPrivacySettingsRepository;
    public IMemberPropertyRepository MemberPropertyRepository => Mock.Object.MemberPropertyRepository;
    public IMemberRepository MemberRepository => Mock.Object.MemberRepository;
    public IMemberSiteSubscriptionRepository MemberSiteSubscriptionRepository => Mock.Object.MemberSiteSubscriptionRepository;
    public IMemberSubscriptionRecordRepository MemberSubscriptionRecordRepository => Mock.Object.MemberSubscriptionRecordRepository;
    public IMemberSubscriptionRepository MemberSubscriptionRepository => Mock.Object.MemberSubscriptionRepository;
    public IMemberTopicRepository MemberTopicRepository => Mock.Object.MemberTopicRepository;
    public INewChapterTopicRepository NewChapterTopicRepository => Mock.Object.NewChapterTopicRepository;
    public INewMemberTopicRepository NewMemberTopicRepository => Mock.Object.NewMemberTopicRepository;
    public INotificationRepository NotificationRepository => Mock.Object.NotificationRepository;
    public IPaymentCheckoutSessionRepository PaymentCheckoutSessionRepository => Mock.Object.PaymentCheckoutSessionRepository;
    public IPaymentReconciliationRepository PaymentReconciliationRepository => Mock.Object.PaymentReconciliationRepository;
    public IPaymentRepository PaymentRepository => Mock.Object.PaymentRepository;
    public IQueuedEmailRecipientRepository QueuedEmailRecipientRepository => Mock.Object.QueuedEmailRecipientRepository;
    public IQueuedEmailRepository QueuedEmailRepository => Mock.Object.QueuedEmailRepository;
    public ISentEmailRepository SentEmailRepository => Mock.Object.SentEmailRepository;
    public ISiteContactMessageReplyRepository SiteContactMessageReplyRepository => Mock.Object.SiteContactMessageReplyRepository;
    public ISiteContactMessageRepository SiteContactMessageRepository => Mock.Object.SiteContactMessageRepository;
    public ISiteEmailSettingsRepository SiteEmailSettingsRepository => Mock.Object.SiteEmailSettingsRepository;
    public ISitePaymentSettingsRepository SitePaymentSettingsRepository => Mock.Object.SitePaymentSettingsRepository;
    public ISiteSettingsRepository SiteSettingsRepository => Mock.Object.SiteSettingsRepository;
    public ISiteSubscriptionPriceRepository SiteSubscriptionPriceRepository => Mock.Object.SiteSubscriptionPriceRepository;
    public ISiteSubscriptionRepository SiteSubscriptionRepository => Mock.Object.SiteSubscriptionRepository;
    public IPaymentProviderWebhookEventRepository PaymentProviderWebhookEventRepository => Mock.Object.PaymentProviderWebhookEventRepository;
    public ITopicGroupRepository TopicGroupRepository => Mock.Object.TopicGroupRepository;
    public ITopicRepository TopicRepository => Mock.Object.TopicRepository;
    public IVenueLocationRepository VenueLocationRepository => Mock.Object.VenueLocationRepository;
    public IVenueRepository VenueRepository => Mock.Object.VenueRepository;

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

    public Task SaveChangesAsync() => Mock.Object.SaveChangesAsync();
}