using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using ODK.Core.Chapters;
using ODK.Core.Countries;
using ODK.Core.Events;
using ODK.Core.Logging;
using ODK.Core.Emails;
using ODK.Core.Members;
using ODK.Core.Payments;
using ODK.Core.Settings;
using ODK.Core.Venues;
using ODK.Data;
using ODK.Data.Repositories;
using ODK.Data.Sql;
using ODK.Services.Authentication;
using ODK.Services.Authorization;
using ODK.Services.Caching;
using ODK.Services.Chapters;
using ODK.Services.Countries;
using ODK.Services.Emails;
using ODK.Services.Events;
using ODK.Services.Imaging;
using ODK.Services.Logging;
using ODK.Services.Members;
using ODK.Services.Payments;
using ODK.Services.Payments.Stripe;
using ODK.Services.Settings;
using ODK.Services.SocialMedia;
using ODK.Services.Venues;
using ODK.Web.Api.Config.Settings;

namespace ODK.Web.Api.Config
{
    public static class DependencyConfig
    {
        public static void ConfigureDependencies(this IServiceCollection services, IConfiguration configuration,
            AppSettings appSettings)
        {
            ConfigureApi(services);
            ConfigureServiceSettings(services, appSettings);
            ConfigureServices(services);
            ConfigureData(services, configuration);
        }

        private static void ConfigureApi(IServiceCollection services)
        {
            services.AddSingleton<IFileProvider>(new PhysicalFileProvider(Directory.GetCurrentDirectory()));
            services.AddSingleton(LoggingConfig.Logger);
        }

        private static void ConfigureData(IServiceCollection services, IConfiguration configuration)
        {
            string connectionString = configuration.GetConnectionString("Default");
            services.AddSingleton<SqlContext>(new OdkContext(connectionString));

            services.AddScoped<IChapterRepository, ChapterRepository>();
            services.AddScoped<ICountryRepository, CountryRepository>();
            services.AddScoped<IEventRepository, EventRepository>();
            services.AddScoped<IEmailRepository, EmailRepository>();
            services.AddScoped<ILoggingRepository, LoggingRepository>();
            services.AddScoped<IMemberRepository, MemberRepository>();
            services.AddScoped<IMemberGroupRepository, MemberGroupRepository>();
            services.AddScoped<IPaymentRepository, PaymentRepository>();
            services.AddScoped<ISettingsRepository, SettingsRepository>();
            services.AddScoped<IVenueRepository, VenueRepository>();
        }

        private static void ConfigureServices(this IServiceCollection services)
        {
            services.AddScoped<IAuthenticationService, AuthenticationService>();
            services.AddScoped<IAuthenticationTokenFactory, AuthenticationTokenFactory>();
            services.AddScoped<IAuthorizationService, AuthorizationService>();
            services.AddScoped<ICacheService, CacheService>();
            services.AddScoped<IChapterAdminService, ChapterAdminService>();
            services.AddScoped<IChapterService, ChapterService>();
            services.AddScoped<ICountryService, CountryService>();
            services.AddScoped<IEmailAdminService, EmailAdminService>();
            services.AddScoped<IEventAdminService, EventAdminService>();
            services.AddScoped<IEventService, EventService>();
            services.AddScoped<IImageService, ImageService>();
            services.AddScoped<ILoggingService, LoggingService>();
            services.AddScoped<IMailProviderFactory, MailProviderFactory>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IMemberAdminService, MemberAdminService>();
            services.AddScoped<IMemberService, MemberService>();
            services.AddScoped<IPaymentService, PaymentService>();
            // TODO: resolve via factory
            services.AddScoped<IPaymentProvider, StripePaymentProvider>();
            services.AddScoped<ISettingsService, SettingsService>();
            services.AddScoped<ISocialMediaService, SocialMediaService>();
            services.AddScoped<IVenueAdminService, VenueAdminService>();
            services.AddScoped<IVenueService, VenueService>();
        }

        private static void ConfigureServiceSettings(IServiceCollection services, AppSettings appSettings)
        {
            AuthSettings auth = appSettings.Auth;
            MembersSettings members = appSettings.Members;
            UrlSettings urls = appSettings.Urls;

            services.AddSingleton(new AuthenticationServiceSettings
            {
                AccessTokenLifetimeMinutes = auth.AccessTokenLifetimeMinutes,
                EventsUrl = $"{urls.Base}{urls.Events}",
                PasswordResetTokenLifetimeMinutes = auth.PasswordResetTokenLifetimeMinutes,
                PasswordResetUrl = $"{urls.Base}{urls.PasswordReset}",
                RefreshTokenLifetimeDays = auth.RefreshTokenLifetimeDays
            });

            services.AddSingleton(new AuthenticationTokenFactorySettings
            {
                Key = auth.Key
            });

            services.AddSingleton(new EventAdminServiceSettings
            {
                BaseUrl = urls.Base,
                EventRsvpUrlFormat = urls.EventRsvp,
                EventUrlFormat = urls.Event
            });

            services.AddSingleton(new MemberServiceSettings
            {
                ActivateAccountUrl = $"{urls.Base}{urls.ActivateAccount}",
                ConfirmEmailAddressUpdateUrl = $"{urls.Base}{urls.ConfirmEmailAddressUpdate}",
                MaxImageSize = members.MaxImageSize
            });
        }
    }
}
