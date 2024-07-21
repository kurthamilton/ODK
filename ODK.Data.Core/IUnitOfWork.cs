using ODK.Core.Settings;
using ODK.Core.SocialMedia;
using ODK.Data.Core.Deferred;
using ODK.Data.Core.Repositories;

namespace ODK.Data.Core;
public interface IUnitOfWork
{
    IChapterAdminMemberRepository ChapterAdminMemberRepository { get; }
    IChapterEmailProviderRepository ChapterEmailProviderRepository { get; }
    IChapterEmailRepository ChapterEmailRepository { get; }
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
    IEmailRepository EmailRepository { get; }
    IErrorPropertyRepository ErrorPropertyRepository { get; }
    IErrorRepository ErrorRepository { get; }
    IEventEmailRepository EventEmailRepository { get; }
    IEventInviteRepository EventInviteRepository { get; }
    IEventRepository EventRepository { get; }
    IEventResponseRepository EventResponseRepository { get; }
    IInstagramImageRepository InstagramImageRepository { get; }
    IInstagramPostRepository InstagramPostRepository { get; }
    IMemberActivationTokenRepository MemberActivationTokenRepository { get; }
    IMemberEmailAddressUpdateTokenRepository MemberEmailAddressUpdateTokenRepository { get; }
    IMemberImageRepository MemberImageRepository { get; }
    IMemberPasswordRepository MemberPasswordRepository { get; }
    IMemberPasswordResetRequestRepository MemberPasswordResetRequestRepository { get; }
    IMemberPropertyRepository MemberPropertyRepository { get; }
    IMemberRepository MemberRepository { get; }
    IMemberSubscriptionRepository MemberSubscriptionRepository { get; }
    IPaymentRepository PaymentRepository { get; }
    ISiteSettingsRepository SiteSettingsRepository { get; }
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
    Task SaveChangesAsync();
}
