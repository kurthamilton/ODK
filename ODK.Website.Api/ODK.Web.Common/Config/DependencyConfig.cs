﻿using System.IO;
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
using ODK.Web.Common.Config.Settings;
using ODK.Services.Media;
using ODK.Services.Files;

namespace ODK.Web.Common.Config
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
            services.AddScoped<ICsvService, CsvService>();
            services.AddScoped<IEmailAdminService, EmailAdminService>();
            services.AddScoped<IEventAdminService, EventAdminService>();
            services.AddScoped<IEventService, EventService>();
            services.AddScoped<IImageService, ImageService>();
            services.AddScoped<ILoggingService, LoggingService>();
            services.AddScoped<IMailProvider, MailProvider>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IInstagramService, InstagramService>();
            services.AddScoped<IMediaAdminService, MediaAdminService>();
            services.AddScoped<IMediaFileProvider, MediaFileProvider>();
            services.AddScoped<IMediaService, MediaService>();
            services.AddScoped<IMemberAdminService, MemberAdminService>();
            services.AddScoped<IMemberService, MemberService>();
            services.AddScoped<IPaymentService, PaymentService>();
            // TODO: resolve via factory
            services.AddScoped<IPaymentProvider, StripePaymentProvider>();
            services.AddScoped<ISettingsService, SettingsService>();
            services.AddScoped<ISocialMediaService, SocialMediaService>();
            services.AddScoped<ISocialMediaAdminService, SocialMediaAdminService>();
            services.AddScoped<IVenueAdminService, VenueAdminService>();
            services.AddScoped<IVenueService, VenueService>();
        }

        private static void ConfigureServiceSettings(IServiceCollection services, AppSettings appSettings)
        {
            AuthSettings auth = appSettings.Auth;
            MembersSettings members = appSettings.Members;
            PathSettings paths = appSettings.Paths;
            UrlSettings urls = appSettings.Urls;

            services.AddSingleton(new AuthenticationServiceSettings
            {
                AccessTokenLifetimeMinutes = auth.AccessTokenLifetimeMinutes,
                EventsUrl = $"{urls.AppBase}{urls.Events}",
                PasswordResetTokenLifetimeMinutes = auth.PasswordResetTokenLifetimeMinutes,
                PasswordResetUrl = $"{urls.AppBase}{urls.PasswordReset}",
                RefreshTokenLifetimeDays = auth.RefreshTokenLifetimeDays
            });

            services.AddSingleton(new AuthenticationTokenFactorySettings
            {
                Key = auth.Key
            });

            services.AddSingleton(new EventAdminServiceSettings
            {
                BaseUrl = urls.AppBase,
                EventRsvpUrlFormat = urls.EventRsvp,
                EventUrlFormat = urls.Event,
                UnsubscribeUrlFormat = urls.Unsubscribe
            });

            services.AddSingleton(new MemberServiceSettings
            {
                ActivateAccountUrl = $"{urls.AppBase}{urls.ActivateAccount}",
                ConfirmEmailAddressUpdateUrl = $"{urls.AppBase}{urls.ConfirmEmailAddressUpdate}",
                MaxImageSize = members.MaxImageSize
            });

            services.AddSingleton(new MediaFileProviderSettings
            {
                RootMediaPath = paths.MediaRoot,
                RootMediaUrl = $"{urls.ApiBase}{urls.Media}"
            });
        }
    }
}
