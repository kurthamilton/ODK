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
    public IChapterEmailRepository ChapterEmailRepository => Mock.Object.ChapterEmailRepository;
    public IChapterEmailSettingsRepository ChapterEmailSettingsRepository => Mock.Object.ChapterEmailSettingsRepository;
    public IChapterEventSettingsRepository ChapterEventSettingsRepository => Mock.Object.ChapterEventSettingsRepository;
    public IChapterLinksRepository ChapterLinksRepository => Mock.Object.ChapterLinksRepository;
    public IChapterMembershipSettingsRepository ChapterMembershipSettingsRepository => Mock.Object.ChapterMembershipSettingsRepository;
    public IChapterPaymentSettingsRepository ChapterPaymentSettingsRepository => Mock.Object.ChapterPaymentSettingsRepository;
    public IChapterPropertyOptionRepository ChapterPropertyOptionRepository => Mock.Object.ChapterPropertyOptionRepository;
    public IChapterPropertyRepository ChapterPropertyRepository => Mock.Object.ChapterPropertyRepository;
    public IChapterQuestionRepository ChapterQuestionRepository => Mock.Object.ChapterQuestionRepository;
    public IChapterRepository ChapterRepository => Mock.Object.ChapterRepository;
    public IChapterSubscriptionRepository ChapterSubscriptionRepository => Mock.Object.ChapterSubscriptionRepository;
    public ICountryRepository CountryRepository => Mock.Object.CountryRepository;
    public IChapterTextsRepository ChapterTextsRepository => Mock.Object.ChapterTextsRepository;
    public IContactRequestRepository ContactRequestRepository => Mock.Object.ContactRequestRepository;
    public ICurrencyRepository CurrencyRepository => Mock.Object.CurrencyRepository;
    public IEmailProviderRepository EmailProviderRepository => Mock.Object.EmailProviderRepository;
    public IEmailRepository EmailRepository => Mock.Object.EmailRepository;
    public IErrorPropertyRepository ErrorPropertyRepository => Mock.Object.ErrorPropertyRepository;
    public IErrorRepository ErrorRepository => Mock.Object.ErrorRepository;
    public IEventCommentRepository EventCommentRepository => Mock.Object.EventCommentRepository;
    public IEventEmailRepository EventEmailRepository => Mock.Object.EventEmailRepository;
    public IEventHostRepository EventHostRepository => Mock.Object.EventHostRepository;
    public IEventInviteRepository EventInviteRepository => Mock.Object.EventInviteRepository;
    public IEventRepository EventRepository => Mock.Object.EventRepository;
    public IEventResponseRepository EventResponseRepository => Mock.Object.EventResponseRepository;
    public IFeatureRepository FeatureRepository => Mock.Object.FeatureRepository;
    public IInstagramImageRepository InstagramImageRepository => Mock.Object.InstagramImageRepository;
    public IInstagramPostRepository InstagramPostRepository => Mock.Object.InstagramPostRepository;
    public IMemberActivationTokenRepository MemberActivationTokenRepository => Mock.Object.MemberActivationTokenRepository;
    public IMemberAvatarRepository MemberAvatarRepository => Mock.Object.MemberAvatarRepository;
    public IMemberEmailAddressUpdateTokenRepository MemberEmailAddressUpdateTokenRepository => Mock.Object.MemberEmailAddressUpdateTokenRepository;
    public IMemberImageRepository MemberImageRepository => Mock.Object.MemberImageRepository;
    public IMemberPasswordRepository MemberPasswordRepository => Mock.Object.MemberPasswordRepository;
    public IMemberPasswordResetRequestRepository MemberPasswordResetRequestRepository => Mock.Object.MemberPasswordResetRequestRepository;
    public IMemberPrivacySettingsRepository MemberChapterPrivacySettingsRepository => Mock.Object.MemberChapterPrivacySettingsRepository;
    public IMemberPropertyRepository MemberPropertyRepository => Mock.Object.MemberPropertyRepository;
    public IMemberRepository MemberRepository => Mock.Object.MemberRepository;
    public IMemberSubscriptionRepository MemberSubscriptionRepository => Mock.Object.MemberSubscriptionRepository;
    public IPaymentRepository PaymentRepository => Mock.Object.PaymentRepository;    
    public ISiteEmailSettingsRepository SiteEmailSettingsRepository => Mock.Object.SiteEmailSettingsRepository;
    public ISiteSettingsRepository SiteSettingsRepository => Mock.Object.SiteSettingsRepository;
    public ISiteSubscriptionPriceRepository SiteSubscriptionPriceRepository => Mock.Object.SiteSubscriptionPriceRepository;
    public ISiteSubscriptionRepository SiteSubscriptionRepository => Mock.Object.SiteSubscriptionRepository;
    public IVenueRepository VenueRepository => Mock.Object.VenueRepository;

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

    public Task SaveChangesAsync() => Mock.Object.SaveChangesAsync();
}
