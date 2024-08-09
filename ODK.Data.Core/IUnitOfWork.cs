using ODK.Core.Settings;
using ODK.Core.SocialMedia;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Repositories;

namespace ODK.Data.Core;
public interface IUnitOfWork
{
    IChapterAdminMemberRepository ChapterAdminMemberRepository { get; }    
    IChapterEmailRepository ChapterEmailRepository { get; }
    IChapterEmailSettingsRepository ChapterEmailSettingsRepository { get; }
    IChapterEventSettingsRepository ChapterEventSettingsRepository { get; }
    IChapterLinksRepository ChapterLinksRepository { get; }
    IChapterMembershipSettingsRepository ChapterMembershipSettingsRepository { get; }
    IChapterPaymentSettingsRepository ChapterPaymentSettingsRepository { get; }
    IChapterPropertyOptionRepository ChapterPropertyOptionRepository { get; }
    IChapterPropertyRepository ChapterPropertyRepository { get; }
    IChapterQuestionRepository ChapterQuestionRepository { get; }
    IChapterRepository ChapterRepository { get; }
    IChapterSubscriptionRepository ChapterSubscriptionRepository { get; }
    ICountryRepository CountryRepository { get; }
    IChapterTextsRepository ChapterTextsRepository { get; }
    IContactRequestRepository ContactRequestRepository { get; }
    ICurrencyRepository CurrencyRepository { get; }
    IEmailProviderRepository EmailProviderRepository { get; }
    IEmailRepository EmailRepository { get; }
    IErrorPropertyRepository ErrorPropertyRepository { get; }
    IErrorRepository ErrorRepository { get; }
    IEventCommentRepository EventCommentRepository { get; }
    IEventEmailRepository EventEmailRepository { get; }
    IEventHostRepository EventHostRepository { get; }
    IEventInviteRepository EventInviteRepository { get; }
    IEventRepository EventRepository { get; }
    IEventResponseRepository EventResponseRepository { get; }
    IFeatureRepository FeatureRepository { get; }
    IInstagramImageRepository InstagramImageRepository { get; }
    IInstagramPostRepository InstagramPostRepository { get; }
    IMemberActivationTokenRepository MemberActivationTokenRepository { get; }
    IMemberAvatarRepository MemberAvatarRepository { get; }
    IMemberChapterRepository MemberChapterRepository { get; }
    IMemberEmailAddressUpdateTokenRepository MemberEmailAddressUpdateTokenRepository { get; }
    IMemberImageRepository MemberImageRepository { get; }
    IMemberPasswordRepository MemberPasswordRepository { get; }
    IMemberPasswordResetRequestRepository MemberPasswordResetRequestRepository { get; }
    IMemberPrivacySettingsRepository MemberChapterPrivacySettingsRepository { get; }
    IMemberPropertyRepository MemberPropertyRepository { get; }
    IMemberRepository MemberRepository { get; }
    IMemberSiteSubscriptionRepository MemberSiteSubscriptionRepository { get; }
    IMemberSubscriptionRepository MemberSubscriptionRepository { get; }
    IPaymentRepository PaymentRepository { get; }    
    ISiteEmailSettingsRepository SiteEmailSettingsRepository { get; }
    ISiteSettingsRepository SiteSettingsRepository { get; }
    ISiteSubscriptionPriceRepository SiteSubscriptionPriceRepository { get; }
    ISiteSubscriptionRepository SiteSubscriptionRepository { get; }
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

    Task SaveChangesAsync();
}
