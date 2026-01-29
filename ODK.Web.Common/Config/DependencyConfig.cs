using System.IO;
using System.Linq;
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
using ODK.Services.Emails;
using ODK.Services.Events;
using ODK.Services.Features;
using ODK.Services.Files;
using ODK.Services.Geolocation;
using ODK.Services.Imaging;
using ODK.Services.Integrations.Emails.Brevo;
using ODK.Services.Integrations.Geolocation;
using ODK.Services.Integrations.Imaging;
using ODK.Services.Integrations.Instagram;
using ODK.Services.Integrations.OAuth;
using ODK.Services.Integrations.Payments;
using ODK.Services.Integrations.Payments.PayPal;
using ODK.Services.Integrations.Payments.Stripe;
using ODK.Services.Integrations.Recaptcha;
using ODK.Services.Issues;
using ODK.Services.Logging;
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
using ODK.Web.Common.Routes;
using ODK.Web.Common.Services;

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
        services.AddSingleton(new StripePaymentProviderSettings
        {
            ConnectedAccountBaseUrl = payments.Stripe.ConnectedAccountBaseUrl,
            ConnectedAccountCommissionPercentage = payments.Stripe.ConnectedAccountCommissionPercentage,
            ConnectedAccountMcc = payments.Stripe.ConnectedAccountMcc,
            ConnectedAccountProductDescription = payments.Stripe.ConnectedAccountProductDescription
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
        services
            .AddScoped<IAccountViewModelService, AccountViewModelService>() 
            .AddScoped<IAuthenticationService, AuthenticationService>() 
            .AddScoped<IAuthorizationService, AuthorizationService>()
            .AddScoped<ICacheService, CacheService>()
            .AddScoped<IChapterAdminService, ChapterAdminService>()
            .AddSingleton(new ChapterAdminServiceSettings
            {
                ContactMessageRecaptchaScoreThreshold = appSettings.Recaptcha.ScoreThreshold
            })
            .AddScoped<IChapterService, ChapterService>()   
            .AddScoped<IChapterViewModelService, ChapterViewModelService>()
            .AddScoped<IContactAdminService, ContactAdminService>()
            .AddScoped<IContactService, ContactService>()
            .AddScoped<ICsvService, CsvService>()
            .AddScoped<IEmailAdminService, EmailAdminService>()
            .AddScoped<IEmailClient, BrevoApiEmailClient>()
            .AddSingleton(new BrevoApiEmailClientSettings
            {
                ApiKey = appSettings.Brevo.ApiKey,
                DebugEmailAddress = appSettings.Emails.DebugEmailAddress
            })
            .AddScoped<IEventAdminService, EventAdminService>()
            .AddSingleton(new EventAdminServiceSettings
            {
                ShortcodeLength = appSettings.Events.ShortcodeLength
            })
            .AddScoped<IEventService, EventService>()
            .AddScoped<IEventViewModelService, EventViewModelService>()
            .AddScoped<IFeatureService, FeatureService>()
            .AddScoped<IImageService, ImageService>()
            .AddScoped<IIssueAdminService, IssueAdminService>()
            .AddScoped<IIssueService, IssueService>()
            .AddScoped<ILoggingService, LoggingService>()
            .AddSingleton(new LoggingServiceSettings
            {
                IgnoreUnknownPathPatterns = appSettings.Logging.IgnorePatterns
                    .Concat(appSettings.RateLimiting.BlockPatterns)
                    .ToArray(),
                IgnoreUnknownPaths = appSettings.Logging.IgnorePaths,
                IgnoreUnknownPathUserAgents = appSettings.Logging.IgnoreUserAgents
            })
            .AddScoped<IEmailService, EmailService>()
            .AddSingleton(new EmailServiceSettings
            {
                DefaultBodyBackground = appSettings.Emails.Theme.Body.Background,
                DefaultBodyColor = appSettings.Emails.Theme.Body.Color,
                DefaultHeaderBackground = appSettings.Emails.Theme.Header.Background,
                DefaultHeaderColor = appSettings.Emails.Theme.Header.Color
            })
            .AddScoped<IGeolocationService, GoogleGeolocationService>()
            .AddSingleton(new GoogleGeolocationServiceSettings
            {
                ApiKey = appSettings.Google.Geolocation.ApiKey
            })
            .AddScoped<IInstagramClient, InstagramClient>()
            .AddSingleton(new InstagramClientSettings
            {
                ChannelUrl = appSettings.Instagram.BaseUrl + appSettings.Instagram.Paths.Channel,
                Cookies = appSettings.Instagram.Client.Cookies,
                GraphQLUrl = appSettings.Instagram.BaseUrl + appSettings.Instagram.Paths.GraphQL,
                Headers = appSettings.Instagram.Client.Headers,
                PostsGraphQlDocId = appSettings.Instagram.Client.GraphQL.PostsDocId
            })
            .AddScoped<IMemberAdminService, MemberAdminService>()
            .AddSingleton(new MemberAdminServiceSettings
            {
                MemberAvatarSize = appSettings.Members.AvatarSize
            })
            .AddScoped<IMemberEmailService, MemberEmailService>()
            .AddScoped<IMemberImageService, MemberImageService>()
            .AddSingleton(new MemberImageServiceSettings
            {
                MaxImageSize = appSettings.Members.MaxImageSize,
                MemberAvatarSize = appSettings.Members.AvatarSize
            })
            .AddScoped<IMemberService, MemberService>()
            .AddScoped<IMemberViewModelService, MemberViewModelService>()
            .AddScoped<INotificationService, NotificationService>()
            .AddScoped<IOAuthProviderFactory, OAuthProviderFactory>()
            .AddScoped<IOdkRoutes, OdkRoutes>()
            .AddScoped<IOdkRoutesFactory, OdkRoutesFactory>()
            .AddScoped<IPasswordHasher, PasswordHasher>()
            .AddSingleton(new PasswordHasherSettings
            {
                Algorithm = appSettings.Auth.Passwords.Algorithm,
                Iterations = appSettings.Auth.Passwords.Iterations
            })
            .AddScoped<IPaymentAdminService, PaymentAdminService>()
            .AddScoped<IPaymentService, PaymentService>()
            .AddScoped<IPlatformProvider, PlatformProvider>()
            .AddSingleton(new PlatformProviderSettings
            {
                DefaultBaseUrls = appSettings.Platforms
                    .Where(x => x.Type == PlatformType.Default.ToString())
                    .Select(x => x.BaseUrl)
                    .ToArray(),
                DrunkenKnitwitsBaseUrls = appSettings.Platforms
                    .Where(x => x.Type == PlatformType.DrunkenKnitwits.ToString())
                    .Select(x => x.BaseUrl)
                    .ToArray()
            })
            .AddScoped<IRecaptchaService, RecaptchaService>()
            .AddScoped<IRequestStore, RequestStore>()
            .AddScoped<IRequestStoreFactory, RequestStoreFactory>()
            .AddScoped<ISettingsService, SettingsService>()
            .AddScoped<ISiteSubscriptionAdminService, SiteSubscriptionAdminService>()
            .AddScoped<ISiteSubscriptionService, SiteSubscriptionService>()
            .AddScoped<ISocialMediaService, SocialMediaService>()
            .AddSingleton(new SocialMediaServiceSettings
            {
                InstagramChannelUrlFormat = appSettings.Instagram.BaseUrl + appSettings.Instagram.Paths.Channel,
                InstagramFetchWaitSeconds = appSettings.Instagram.FetchWaitSeconds,
                InstagramPostUrlFormat = appSettings.Instagram.BaseUrl + appSettings.Instagram.Paths.Post,
                InstagramTagUrlFormat = appSettings.Instagram.BaseUrl + appSettings.Instagram.Paths.Tag,
                WhatsAppUrlFormat = appSettings.WhatsApp.UrlFormat
            })
            .AddScoped<ITopicAdminService, TopicAdminService>()
            .AddScoped<ITopicService, TopicService>()
            .AddScoped<IVenueAdminService, VenueAdminService>();
    }

    private static void ConfigureServiceSettings(IServiceCollection services, AppSettings appSettings)
    {
        AuthSettings auth = appSettings.Auth;
        MembersSettings members = appSettings.Members;
        OAuthSettings oauth = appSettings.OAuth;
        RecaptchaSettings recaptcha = appSettings.Recaptcha;

        services.AddSingleton(new AccountViewModelServiceSettings
        {
            GoogleClientId = oauth.Google.ClientId
        });

        services.AddSingleton(new AuthenticationServiceSettings
        {
            PasswordResetTokenLifetimeMinutes = auth.PasswordResetTokenLifetimeMinutes,
        });

        services.AddSingleton(new RecaptchaServiceSettings
        {
            ScoreThreshold = recaptcha.ScoreThreshold,
            SecretKey = recaptcha.SecretKey,
            SiteKey = recaptcha.SiteKey,
            VerifyUrl = recaptcha.VerifyUrl
        });
    }
}