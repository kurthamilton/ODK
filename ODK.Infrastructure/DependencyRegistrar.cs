using Microsoft.Extensions.DependencyInjection;
using ODK.Core.Countries;
using ODK.Core.Exceptions;
using ODK.Core.Platforms;
using ODK.Core.Subscriptions;
using ODK.Core.Web;
using ODK.Data.Core;
using ODK.Data.EntityFramework;
using ODK.Infrastructure.Settings;
using ODK.Services;
using ODK.Services.Authentication;
using ODK.Services.Authentication.OAuth;
using ODK.Services.Authorization;
using ODK.Services.Chapters;
using ODK.Services.Chapters.Workflows;
using ODK.Services.Contact;
using ODK.Services.Countries;
using ODK.Services.Csv;
using ODK.Services.Emails;
using ODK.Services.Emails.Parameters;
using ODK.Services.Html;
using ODK.Services.Emails.Validation;
using ODK.Services.Integrations.Emails;
using ODK.Services.Integrations.Html;
using ODK.Services.Events;
using ODK.Services.Features;
using ODK.Services.Geolocation;
using ODK.Services.Imaging;
using ODK.Services.Integrations.Authentication;
using ODK.Services.Integrations.Csv;
using ODK.Services.Integrations.Emails.Brevo;
using ODK.Services.Integrations.Geolocation;
using ODK.Services.Integrations.Imaging;
using ODK.Services.Integrations.Instagram;
using ODK.Services.Integrations.OAuth;
using ODK.Services.Integrations.Payments;
using ODK.Services.Integrations.Payments.PayPal;
using ODK.Services.Integrations.Payments.Stripe;
using ODK.Services.Integrations.Recaptcha;
using ODK.Services.Localization;
using ODK.Services.Logging;
using ODK.Services.Members;
using ODK.Services.Members.Workflows.Account;
using ODK.Services.Members.Workflows.ChapterMembership;
using ODK.Services.Workflows;
using ODK.Core.Workflows;
using ODK.Services.Members.Tasks;
using ODK.Services.Members.Tasks.Providers;
using ODK.Services.Notifications;
using ODK.Services.Payments;
using ODK.Services.Platforms;
using ODK.Services.Referrals;
using ODK.Services.Recaptcha;
using ODK.Services.Settings;
using ODK.Services.SocialMedia;
using ODK.Services.Subscriptions;
using ODK.Services.Topics;
using ODK.Services.Users;
using ODK.Services.Venues;
using ODK.Services.Web;
using ODK.Web.Common.Account;
using ODK.Web.Common.Routes;
using ODK.Web.Common.Services;
using ODK.Web.Common.Settings;
using ODK.Services.Integrations.Emails.Reoon;
using ODK.Services.Questions;

namespace ODK.Infrastructure;

public static class DependencyRegistrar
{
    public static void ConfigureDependencies(this IServiceCollection services, AppSettings appSettings)
    {
        ConfigureAuthentication(services, appSettings);
        ConfigureCore(services);
        ConfigurePayments(services, appSettings);
        ConfigureServiceSettings(services, appSettings);
        ConfigureServices(services, appSettings);
        ConfigureData(services, appSettings);
        ConfigureWebSettings(services, appSettings);

        /* AppSettings is deliberately not registered. Every consumer takes a mapped settings type declaring the
           values it uses, so config stays a contract this project translates rather than one anything can reach
           into - and a consumer that wants a new value has to say so here. */
    }

    private static void ConfigureAuthentication(this IServiceCollection services, AppSettings appSettings)
    {
        services.AddScoped<ILoginHandler, LoginHandler>();
        services.AddSingleton(new LoginHandlerSettings(appSettings.Auth.CookieLifetimeDays));
    }

    private static void ConfigureCore(IServiceCollection services)
    {
        services
            .AddScoped<IDistanceUnitFactory, DistanceUnitFactory>()
            .AddSingleton<IHtmlTextExtractor>(new HtmlTextExtractor())
            .AddSingleton<IHtmlValidator>(new HtmlValidator())
            .AddSingleton<ICsvReader, CsvReader>()
            .AddSingleton<ICsvWriter, CsvWriter>()
            .AddScoped<IUrlProviderFactory, UrlProviderFactory>();
    }

    private static void ConfigureData(IServiceCollection services, AppSettings appSettings)
    {
        var connectionString = appSettings.ConnectionStrings.Default;
        services.AddScoped<OdkContext>();
        services.AddSingleton(new OdkContextSettings(connectionString));
        services.AddSingleton<IEntityIdGenerator>(new SequentialIdGenerator());
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IUnitOfWorkFactory, UnitOfWorkFactory>();
    }

    private static void ConfigurePayments(this IServiceCollection services, AppSettings appSettings)
    {
        var payments = appSettings.Payments;

        services.AddScoped<IPaymentProviderFactory, PaymentProviderFactory>();
        services.AddSingleton(new PaymentProviderFactorySettings
        {
            DefaultProvider = appSettings.Payments.Active
        });
        /* What the deployment transacts as. Several services ask it, so it is registered once rather
           than declared per service. */
        services.AddSingleton(new PaymentSettings
        {
            Platforms = payments.Stripe.Platforms.ToDictionary(
                x => x.Key,
                x => new PaymentPlatformSettings
                {
                    AccountId = x.Value.AccountId,
                    Enabled = x.Value.Enabled,
                    PublicApiKey = x.Value.PublicApiKey
                }),
            Provider = payments.Active
        });
        services.AddSingleton(new PayPalPaymentProviderSettings
        {
            ApiBaseUrl = payments.PayPal.ApiBaseUrl
        });
        services.AddSingleton(new StripePaymentProviderSettings
        {
            Platforms = payments.Stripe.Platforms.ToDictionary(
                x => x.Key,
                x => new StripePaymentProviderPlatformSettings
                {
                    ConnectedAccountBaseUrl = x.Value.ConnectedAccountBaseUrl,
                    PublicApiKey = x.Value.PublicApiKey,
                    SecretApiKey = x.Value.SecretApiKey
                }),
            ConnectedAccountBusinessName = payments.Stripe.ConnectedAccountBusinessName,
            ConnectedAccountCommissionPercentage = payments.Stripe.ConnectedAccountCommissionPercentage,
            ConnectedAccountMcc = payments.Stripe.ConnectedAccountMcc,
            ConnectedAccountProductDescription = payments.Stripe.ConnectedAccountProductDescription,
            SettlementReadDelay = TimeSpan.FromSeconds(payments.Stripe.SettlementReadDelaySeconds)
        });
        services.AddScoped<IStripeWebhookAdminService, StripeWebhookAdminService>();
        services.AddSingleton(new StripeWebhookAdminServiceSettings
        {
            Events = payments.Stripe.Webhooks.Events,
            Hosts = (payments.Stripe.Webhooks.Hosts ?? []).ToDictionary(
                x => x.Key,
                x => (IReadOnlyDictionary<PlatformType, string>)(x.Value ?? [])),
            LiveDashboardUrlFormat = payments.Stripe.Webhooks.LiveDashboardUrlFormat,
            Path = payments.Stripe.Webhooks.Path,
            TestDashboardUrlFormat = payments.Stripe.Webhooks.TestDashboardUrlFormat
        });
        services.AddScoped<IStripeWebhookParser, StripeWebhookParser>();
        services.AddSingleton(new StripeWebhookParserSettings
        {
            WebhookSecretsV1 = appSettings.Payments.Stripe.Platforms.ToDictionary(x => x.Key, x => x.Value.WebhookSecretV1),
            WebhookSecretsV2 = appSettings.Payments.Stripe.Platforms.ToDictionary(x => x.Key, x => x.Value.WebhookSecretV2)
        });
    }

    private static void ConfigureServices(this IServiceCollection services, AppSettings appSettings)
    {
        services
            .AddScoped<IAccountViewModelService, AccountViewModelService>()
            .AddScoped<IAuthenticationService, AuthenticationService>()
            .AddScoped<IAuthorizationService, AuthorizationService>()
            .AddScoped<IChapterAdminService, ChapterAdminService>()
            .AddSingleton(new ChapterAdminServiceSettings
            {
                ContactMessageRecaptchaScoreThreshold = appSettings.Recaptcha.ScoreThreshold,
                DefaultCountryCode = appSettings.Groups.DefaultCountryCode,
                ReservedSlugs = appSettings.Groups.ReservedSlugs
            })
            .AddScoped<IChapterService, ChapterService>()
            .AddScoped<IChapterSiteAdminService, ChapterSiteAdminService>()
            .AddScoped<IChapterViewModelService, ChapterViewModelService>()
            .AddScoped<IContactAdminService, ContactAdminService>()
            .AddSingleton(new ContactAdminServiceSettings
            {
                ContactMessageRecaptchaScoreThreshold = appSettings.Recaptcha.ScoreThreshold
            })
            .AddScoped<IContactService, ContactService>()
            .AddScoped<IEmailAdminService, EmailAdminService>()
            .AddScoped<BrevoApiEmailClient>()
            .AddScoped<ConsoleEmailClient>()
            .AddScoped<IEmailClient>(serviceProvider => appSettings.Emails.UseConsoleClient
                ? serviceProvider.GetRequiredService<ConsoleEmailClient>()
                : serviceProvider.GetRequiredService<BrevoApiEmailClient>())
            .AddSingleton(new BrevoApiEmailClientSettings
            {
                ApiKey = appSettings.Brevo.ApiKey,
                DebugEmailAddress = appSettings.Emails.DebugEmailAddress,
                Environment = appSettings.Environment,
                EnvironmentTagPrefix = appSettings.Brevo.EnvironmentTagPrefix
            })
            .AddScoped<IBrevoWebhookParser, BrevoWebhookParser>()
            .AddSingleton(new BrevoWebhookParserSettings
            {
                Environment = appSettings.Environment,
                EnvironmentTagPrefix = appSettings.Brevo.EnvironmentTagPrefix
            })
            .AddScoped<IEventAdminService, EventAdminService>()
            .AddSingleton(new EventAdminServiceSettings
            {
                ShortcodeLength = appSettings.Events.ShortcodeLength
            })
            .AddScoped<IEventService, EventService>()
            .AddScoped<IEventViewModelService, EventViewModelService>()
            .AddScoped<IFeatureService, FeatureService>()
            .AddScoped<IEmailValidationService, EmailValidationService>()
            .AddScoped<IEmailVerifier, ReoonEmailVerifier>()
            .AddScoped<IReferralAdminService, ReferralAdminService>()
            .AddScoped<IReferralService, ReferralService>()
            .AddScoped<ISiteQuestionAdminService, SiteQuestionAdminService>()
            .AddScoped<ISiteQuestionViewModelService, SiteQuestionViewModelService>()
            .AddScoped<IImageService, ImageService>()
            .AddSingleton(new ImageServiceSettings
            {
                MaxPixels = appSettings.Imaging.MaxPixels
            })
            .AddScoped<ILoggingService, LoggingService>()
            .AddSingleton(new LoggingServiceSettings
            {
                IgnoreExceptions = appSettings.Logging.IgnoreExceptions
                    /* Coalesced because `required` binds nothing: the configuration binder constructs settings
                       reflectively, so a key absent from appsettings.json arrives null however the property is
                       declared. An omitted criterion means the rule has none of that kind, which is what an
                       empty array says - and lets a rule state only the criteria it uses. */
                    .Select(x => new IgnoreExceptionRule
                    {
                        Exceptions = x.Exceptions ?? [],
                        Headers = x.Headers ?? [],
                        Paths = x.Paths ?? [],
                        PathPatterns = x.PathPatterns ?? [],
                        UserAgents = x.UserAgents ?? []
                    })
                    .Append(new IgnoreExceptionRule
                    {
                        Exceptions = [nameof(OdkNotFoundException)],
                        PathPatterns = appSettings.RateLimiting.BlockPatterns.ToArray()
                    })
                    .Append(new IgnoreExceptionRule
                    {
                        Exceptions = [nameof(OdkNotFoundException)],
                        Paths = appSettings.RateLimiting.BlockPaths.ToArray()
                    })
                    .ToArray()
            })
            .AddScoped<IEmailService, EmailService>()
            .AddScoped<ITestEmailParametersFactory, TestEmailParametersFactory>()
            .AddSingleton(new EmailServiceSettings
            {
                DefaultBodyBackground = appSettings.Emails.Theme.Body.Background,
                DefaultBodyColor = appSettings.Emails.Theme.Body.Color,
                DefaultHeaderBackground = appSettings.Emails.Theme.Header.Background,
                DefaultHeaderColor = appSettings.Emails.Theme.Header.Color
            })
            .AddScoped<IGeolocationService, GeolocationService>()
            .AddSingleton(new GeolocationServiceSettings
            {
                GoogleApiKey = appSettings.Google.Geolocation.ApiKey,
                GoogleDisabled = appSettings.Google.Geolocation.Disabled
            })
            .AddScoped<IInstagramClient, InstagramClient>()
            .AddSingleton(new InstagramClientSettings
            {
                ChannelUrl = appSettings.Instagram.BaseUrl + appSettings.Instagram.Paths.Channel,
                // Coalesced because config cannot state an empty dictionary - see InstagramClientAppSettings.
                Cookies = appSettings.Instagram.Client.Cookies ?? [],
                GraphQLUrl = appSettings.Instagram.BaseUrl + appSettings.Instagram.Paths.GraphQL,
                Headers = appSettings.Instagram.Client.Headers ?? [],
                PostsGraphQlDocId = appSettings.Instagram.Client.GraphQL.PostsDocId
            })
            .AddScoped<ILatLongCalculator, LatLongCalculator>()
            .AddScoped<IMemberAdminService, MemberAdminService>()
            .AddSingleton(new MemberAdminServiceSettings
            {
                MemberAvatarSize = appSettings.Members.AvatarSize
            })
            .AddScoped<IMemberChapterSubscriptionWriter, MemberChapterSubscriptionWriter>()
            .AddScoped<IMemberSiteSubscriptionWriter, MemberSiteSubscriptionWriter>()
            .AddScoped<IMemberEmailService, MemberEmailService>()
            .AddScoped<IMemberImageService, MemberImageService>()
            .AddScoped<IMemberLocaleService, MemberLocaleService>()
            .AddSingleton(new MemberImageServiceSettings
            {
                MemberAvatarSize = appSettings.Members.AvatarSize
            })
            .AddScoped<IMemberService, MemberService>()
            .AddAccountWorkflows()
            .AddChapterWorkflows()
            .AddScoped<IMemberTaskService, MemberTaskService>()
            .AddScoped<IMemberTaskProvider, CompleteChapterProfileTaskProvider>()
            .AddScoped<IMemberTaskProvider, PublishChapterTaskProvider>()
            .AddScoped<IMemberTaskProvider, UploadChapterImageTaskProvider>()
            .AddScoped<IMemberTaskProvider, UploadImageTaskProvider>()
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
            .AddScoped<IBreachedPasswordChecker, HibpBreachedPasswordChecker>()
            .AddSingleton(new HibpBreachedPasswordCheckerSettings
            {
                Enabled = appSettings.Hibp.Enabled,
                RangeApiUrl = appSettings.Hibp.RangeApiUrl
            })
            .AddScoped<IMemberPasswordService, MemberPasswordService>()
            .AddScoped<IPasswordPolicy, PasswordPolicy>()
            .AddSingleton(new PasswordPolicySettings
            {
                MinLength = appSettings.Auth.Passwords.MinLength
            })
            .AddScoped<IPaymentAdminService, PaymentAdminService>()
            .AddScoped<IPaymentService, PaymentService>()
            .AddSingleton(new PaymentServiceSettings
            {
                Environment = appSettings.Environment,
                PaymentProvider = appSettings.Payments.Active
            })
            .AddScoped<IPlatformProvider, PlatformProvider>()
            .AddSingleton(new PlatformProviderSettings
            {
                Names = appSettings.Platforms.ToDictionary(x => x.Key, x => x.Value.Name),
                Urls = appSettings.Platforms.ToDictionary(x => x.Key, x => (IReadOnlyCollection<string>)x.Value.Urls)
            })
            .AddScoped<ICountryAdminService, CountryAdminService>()
            .AddScoped<ILocaleService, LocaleService>()
            .AddScoped<IRecaptchaService, RecaptchaService>()
            .AddScoped<IRequestStore, RequestStore>()
            .AddSingleton(new RequestStoreSettings
            {
                Environment = appSettings.Environment
            })
            .AddScoped<IRequestStoreFactory, RequestStoreFactory>()
            .AddScoped<IServiceRequestFactory, ServiceRequestFactory>()
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

        // App-level fallback locale (Localisation:DefaultLocale), used when a member has neither a
        // preference nor a resolvable country.
        services.AddSingleton(new LocaleServiceSettings
        {
            DefaultLocale = appSettings.Localisation.DefaultLocale
        });

        services.AddSingleton(new ReoonEmailVerifierSettings
        {
            ApiKey = appSettings.Reoon.ApiKey,
            Mode = appSettings.Reoon.Mode,
            VerifyUrl = appSettings.Reoon.VerifyUrl
        });

        services.AddSingleton(new RecaptchaServiceSettings
        {
            Enabled = recaptcha.Enabled,
            ScoreThreshold = recaptcha.ScoreThreshold,
            SecretKey = recaptcha.SecretKey,
            SiteKey = recaptcha.SiteKey,
            VerifyUrl = recaptcha.VerifyUrl
        });

        // How long an expired site subscription keeps its access (Subscriptions:DefaultCooldownMonths).
        services.AddSingleton(new SiteSubscriptionCooldown(appSettings.Subscriptions.DefaultCooldownMonths));
    }

    /* The web layer's own mapped settings, declared in ODK.Web.Common so this project can see them - the
       consumers are middleware and controllers in ODK.Web.Razor, which it cannot. Same rule as the service
       settings above: a consumer takes what it needs, not AppSettings. */
    private static void ConfigureWebSettings(IServiceCollection services, AppSettings appSettings)
    {
        GoogleMapsSettings maps = appSettings.Google.Maps;
        RateLimitingSettings rateLimiting = appSettings.RateLimiting;

        services.AddSingleton(new GoogleLocationViewSettings
        {
            ApiKey = maps.ApiKey
        });

        services.AddSingleton(new GoogleMapViewSettings
        {
            ApiKey = maps.ApiKey
        });

        services.AddSingleton(new RateLimitingMiddlewareSettings
        {
            BlockForSeconds = rateLimiting.BlockForSeconds,
            BlockIpAddresses = rateLimiting.BlockIpAddresses,
            BlockPaths = rateLimiting.BlockPaths,
            BlockPatterns = rateLimiting.BlockPatterns
        });

        services.AddSingleton(new ScheduledTasksControllerSettings
        {
            ApiKey = appSettings.ScheduledTasks.ApiKey
        });

        services.AddSingleton(new WebhooksControllerSettings
        {
            BrevoWebhookPassword = appSettings.Brevo.WebhookPassword,
            BrevoWebhookPasswordHeader = appSettings.Brevo.WebhookPasswordHeader
        });
    }

    /// <summary>
    /// The two account machines. A definition is immutable and holds no state, so one instance serves every
    /// request, and the steps come from the definition rather than from a list repeated here - a step added to a
    /// transition is registered by being on it.
    /// </summary>
    /// <summary>
    /// How a group becomes findable. Registered alongside the account machines because the shape is the same;
    /// it shares nothing with them but the framework.
    /// </summary>
    private static IServiceCollection AddChapterWorkflows(this IServiceCollection services)
    {
        var publication = ChapterPublicationStateMachine.Create();

        services
            .AddSingleton(publication)
            .AddSingleton<IStateMachineDiagram>(publication)
            .AddScoped<
                IStateResolver<ChapterPublicationState, ChapterPublicationContext>,
                ChapterPublicationStateResolver>()
            .AddScoped<
                IStepFactory<ChapterPublicationContext>,
                ServiceProviderStepFactory<ChapterPublicationContext>>()
            .AddScoped<StateMachineRunner<
                ChapterPublicationState, ChapterPublicationTrigger, ChapterPublicationContext>>();

        foreach (var stepType in publication.StepTypes)
        {
            services.AddScoped(stepType);
        }

        return services;
    }

    private static IServiceCollection AddAccountWorkflows(this IServiceCollection services)
    {
        var account = AccountStateMachine.Create();
        var membership = ChapterMembershipStateMachine.Create();

        services
            .AddSingleton(account)
            /* Also registered under the non-generic view of a definition, which exists so the site-admin page
               can hold machines whose state and context types differ. */
            .AddSingleton<IStateMachineDiagram>(account)
            .AddScoped<IAccountContextFactory, AccountContextFactory>()
            .AddScoped<IStateResolver<AccountState, AccountContext>, AccountStateResolver>()
            .AddScoped<IStepFactory<AccountContext>, ServiceProviderStepFactory<AccountContext>>()
            .AddScoped<StateMachineRunner<AccountState, AccountTrigger, AccountContext>>()
            .AddSingleton(membership)
            .AddSingleton<IStateMachineDiagram>(membership)
            .AddScoped<IChapterMembershipContextFactory, ChapterMembershipContextFactory>()
            .AddScoped<
                IStateResolver<ChapterMembershipState, ChapterMembershipContext>,
                ChapterMembershipStateResolver>()
            .AddScoped<
                IStepFactory<ChapterMembershipContext>,
                ServiceProviderStepFactory<ChapterMembershipContext>>()
            .AddScoped<StateMachineRunner<
                ChapterMembershipState, ChapterMembershipTrigger, ChapterMembershipContext>>();

        foreach (var stepType in account.StepTypes.Concat(membership.StepTypes))
        {
            services.AddScoped(stepType);
        }

        return services;
    }
}
