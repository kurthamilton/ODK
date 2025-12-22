using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using ODK.Core.Platforms;
using ODK.Core.Web;
using ODK.Data.Core;
using ODK.Data.EntityFramework;
using ODK.Services.Authentication;
using ODK.Services.Authentication.OAuth;
using ODK.Services.Authorization;
using ODK.Services.Caching;
using ODK.Services.Chapters;
using ODK.Services.Contact;
using ODK.Services.Countries;
using ODK.Services.Emails;
using ODK.Services.Events;
using ODK.Services.Features;
using ODK.Services.Files;
using ODK.Services.Imaging;
using ODK.Services.Integrations.Emails;
using ODK.Services.Integrations.Emails.Smtp;
using ODK.Services.Integrations.Imaging;
using ODK.Services.Integrations.OAuth;
using ODK.Services.Integrations.Payments;
using ODK.Services.Integrations.Payments.PayPal;
using ODK.Services.Integrations.Payments.Stripe;
using ODK.Services.Issues;
using ODK.Services.Logging;
using ODK.Services.Media;
using ODK.Services.Members;
using ODK.Services.Notifications;
using ODK.Services.Payments;
using ODK.Services.Recaptcha;
using ODK.Services.Settings;
using ODK.Services.SocialMedia;
using ODK.Services.Subscriptions;
using ODK.Services.Topics;
using ODK.Services.Users;
using ODK.Services.Venues;
using ODK.Services.Web;
using ODK.Web.Common.Account;
using ODK.Web.Common.Config.Settings;
using ODK.Web.Common.Platforms;
using ODK.Web.Common.Services;
using System.IO;
using System.Linq;

namespace ODK.Web.Common.Config;

public static class DependencyConfig
{
    public static void ConfigureDependencies(this IServiceCollection services, AppSettings appSettings)
    {
        ConfigureApi(services);
        ConfigureAuthentication(services, appSettings);
        ConfigureCore(services);
        ConfigurePayments(services, appSettings);
        ConfigureServiceSettings(services, appSettings);
        ConfigureServices(services, appSettings);
        ConfigureData(services, appSettings);

        services.AddSingleton(appSettings);
    }

    private static void ConfigureAuthentication(this IServiceCollection services, AppSettings appSettings)
    {
        services.AddScoped<ILoginHandler, LoginHandler>();
        services.AddSingleton(new LoginHandlerSettings(appSettings.Auth.CookieLifetimeDays));
    }

    private static void ConfigureApi(IServiceCollection services)
    {
        services.AddSingleton<IFileProvider>(new PhysicalFileProvider(Directory.GetCurrentDirectory()));

        if (LoggingConfig.Logger != null)
        {
            services.AddSingleton(LoggingConfig.Logger);
        }
    }

    private static void ConfigureCore(IServiceCollection services)
    {        
        services.AddSingleton<IHtmlSanitizer>(new HtmlSanitizer());
        services.AddScoped<IUrlProviderFactory, UrlProviderFactory>();
    }

    private static void ConfigureData(IServiceCollection services, AppSettings appSettings)
    {
        var connectionString = appSettings.ConnectionStrings.Default;
        services.AddScoped<OdkContext>();
        services.AddSingleton(new OdkContextSettings(connectionString));
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IUnitOfWorkFactory, UnitOfWorkFactory>();
    }

    private static void ConfigurePayments(this IServiceCollection services, AppSettings appSettings)
    {
        var payments = appSettings.Payments;

        services.AddScoped<IPaymentProviderFactory, PaymentProviderFactory>();
        services.AddSingleton(new PayPalPaymentProviderSettings
        {
            ApiBaseUrl = payments.PayPal.ApiBaseUrl
        });
        services.AddScoped<IStripeWebhookParser, StripeWebhookParser>();
        services.AddSingleton(new StripeWebhookParserSettings
        {
            WebhookSecretV1 = appSettings.Payments.Stripe.WebhookSecretV1,
            WebhookSecretV2 = appSettings.Payments.Stripe.WebhookSecretV2
        });
    }

    private static void ConfigureServices(this IServiceCollection services, AppSettings appSettings)
    {
        services.AddScoped<IAccountViewModelService, AccountViewModelService>();
        services.AddScoped<IAuthenticationService, AuthenticationService>();
        services.AddScoped<IAuthorizationService, AuthorizationService>();
        services.AddScoped<ICacheService, CacheService>();
        services.AddScoped<IChapterAdminService, ChapterAdminService>();
        services.AddSingleton(new ChapterAdminServiceSettings
        {
            ContactMessageRecaptchaScoreThreshold = appSettings.Recaptcha.ScoreThreshold
        });
        services.AddScoped<IChapterService, ChapterService>();
        services.AddScoped<IChapterViewModelService, ChapterViewModelService>();
        services.AddScoped<IContactAdminService, ContactAdminService>();
        services.AddScoped<IContactService, ContactService>();
        services.AddScoped<ICountryService, CountryService>();
        services.AddScoped<ICsvService, CsvService>();
        services.AddScoped<ICurrencyService, CurrencyService>();
        services.AddScoped<IEmailAdminService, EmailAdminService>();
        services.AddScoped<IEmailClientFactory, EmailClientFactory>();
        services.AddScoped<IEventAdminService, EventAdminService>();
        services.AddScoped<IEventService, EventService>();
        services.AddScoped<IEventViewModelService, EventViewModelService>();
        services.AddScoped<IFeatureService, FeatureService>();
        services.AddScoped<IImageService, ImageService>();
        services.AddScoped<IIssueAdminService, IssueAdminService>();
        services.AddScoped<IIssueService, IssueService>();
        services.AddScoped<ILoggingService, LoggingService>();
        services.AddScoped<SmtpEmailClient>();
        services.AddSingleton(new SmtpEmailClientSettings
        {
            DebugEmailAddress = appSettings.Emails.DebugEmailAddress
        });
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IInstagramService, InstagramService>();
        services.AddScoped<IMediaAdminService, MediaAdminService>();
        services.AddScoped<IMediaFileProvider, MediaFileProvider>();
        services.AddScoped<IMediaService, MediaService>();
        services.AddScoped<IMemberAdminService, MemberAdminService>();
        services.AddSingleton(new MemberAdminServiceSettings
        {
            MemberAvatarSize = appSettings.Members.AvatarSize
        });
        services.AddScoped<IMemberEmailService, MemberEmailService>();
        services.AddScoped<IMemberImageService, MemberImageService>();
        services.AddSingleton(new MemberImageServiceSettings
        {
            MaxImageSize = appSettings.Members.MaxImageSize,
            MemberAvatarSize = appSettings.Members.AvatarSize
        });
        services.AddScoped<IMemberService, MemberService>();
        services.AddScoped<IMemberViewModelService, MemberViewModelService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IOAuthProviderFactory, OAuthProviderFactory>();
        services.AddScoped<IPaymentAdminService, PaymentAdminService>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IPlatformProvider, PlatformProvider>();
        services.AddSingleton(new PlatformProviderSettings
        {
            DefaultBaseUrls = appSettings.Platforms
                .Where(x => x.Type == PlatformType.Default.ToString())
                .Select(x => x.BaseUrl)
                .ToArray(),
            DrunkenKnitwitsBaseUrls = appSettings.Platforms
                .Where(x => x.Type == PlatformType.DrunkenKnitwits.ToString())
                .Select(x => x.BaseUrl)
                .ToArray()
        });
        services.AddScoped<IRecaptchaService, RecaptchaService>();
        services.AddScoped<IRequestCache, RequestCache>();
        services.AddScoped<ISettingsService, SettingsService>();
        services.AddScoped<ISiteSubscriptionAdminService, SiteSubscriptionAdminService>();
        services.AddScoped<ISiteSubscriptionService, SiteSubscriptionService>();
        services.AddScoped<ITopicAdminService, TopicAdminService>();
        services.AddScoped<ITopicService, TopicService>();
        services.AddScoped<IVenueAdminService, VenueAdminService>();
        services.AddScoped<IVenueService, VenueService>();
    }

    private static void ConfigureServiceSettings(IServiceCollection services, AppSettings appSettings)
    {
        AuthSettings auth = appSettings.Auth;
        MembersSettings members = appSettings.Members;
        OAuthSettings oauth = appSettings.OAuth;
        PathSettings paths = appSettings.Paths;
        RecaptchaSettings recaptcha = appSettings.Recaptcha;

        services.AddSingleton(new AccountViewModelServiceSettings
        {
            GoogleClientId = oauth.Google.ClientId
        });

        services.AddSingleton(new AuthenticationServiceSettings
        {
            PasswordResetTokenLifetimeMinutes = auth.PasswordResetTokenLifetimeMinutes,
        });
        
        services.AddSingleton(new MediaFileProviderSettings
        {
            RootMediaPath = paths.MediaRoot,
            RootMediaUrlPath = "NOT IMPLEMENTED"
        });

        services.AddSingleton(new RecaptchaServiceSettings
        {
            ScoreThreshold = recaptcha.ScoreThreshold,
            VerifyUrl = recaptcha.VerifyUrl
        });
    }
}
