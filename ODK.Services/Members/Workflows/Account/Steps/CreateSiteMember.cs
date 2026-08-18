using ODK.Core.Chapters;
using ODK.Core.Countries;
using ODK.Core.Members;
using ODK.Core.Workflows;
using ODK.Data.Core;
using ODK.Services.Geolocation;
using ODK.Services.Logging;
using ODK.Services.Recaptcha;

namespace ODK.Services.Members.Workflows.Account.Steps;

/// <summary>
/// Adds the account a sign-up to the site asks for, together with everything the submitted location decides:
/// where the member is, the currency they are billed in, and the units they see distances in.
/// </summary>
/// <remarks>
/// One step rather than four because the timezone and the country come from two geolocation lookups that all
/// four writes depend on, and those are outbound calls - resolving them here means only a transition that
/// creates an account pays for them, and means no step has to hand another the answer.
/// </remarks>
public sealed class CreateSiteMember : IStep<AccountContext>
{
    private readonly IDistanceUnitFactory _distanceUnitFactory;
    private readonly IGeolocationService _geolocationService;
    private readonly ILoggingService _loggingService;
    private readonly IRecaptchaService _recaptchaService;
    private readonly IUnitOfWork _unitOfWork;

    public CreateSiteMember(
        IUnitOfWork unitOfWork,
        IGeolocationService geolocationService,
        IDistanceUnitFactory distanceUnitFactory,
        IRecaptchaService recaptchaService,
        ILoggingService loggingService)
    {
        _distanceUnitFactory = distanceUnitFactory;
        _geolocationService = geolocationService;
        _loggingService = loggingService;
        _recaptchaService = recaptchaService;
        _unitOfWork = unitOfWork;
    }

    public static string Description => "creates the account, and places it from the submitted location";

    public static StepKind Kind => StepKind.Write;

    public async Task<StepOutcome> Execute(AccountContext context, CancellationToken cancellationToken)
    {
        var profile = context.RequiredSiteProfile;
        var location = profile.Location;

        var timeZone = location != null
            ? await _geolocationService.GetTimeZoneFromLocation(location.Value)
            : null;

        if (timeZone == null)
        {
            await _loggingService.Error(
                $"Error getting member time zone for location {location?.Lat}, {location?.Long}. " +
                $"Falling back to default");

            timeZone = Chapter.DefaultTimeZone;
        }

        // Never blocking: a low score flags the account for site admin review, decided against the threshold
        // in force now and stored as a snapshot of it.
        var recaptcha = await _recaptchaService.Verify(profile.RecaptchaToken);

        var member = _unitOfWork.MemberRepository.Add(new Member
        {
            CreatedUtc = DateTime.UtcNow,
            EmailAddress = profile.EmailAddress,
            FirstName = profile.FirstName,
            LastName = profile.LastName,
            RecaptchaFlagged = !_recaptchaService.Success(recaptcha),
            RecaptchaScore = recaptcha.Score,
            /* The resolved referral, not the posted id: the id comes from the browser, so one that matches
               nothing is discarded rather than failing the sign-up on a foreign key violation. An already
               completed referral is discarded too, so one referral can only ever bring in one member - without
               it, a stored id left over from an earlier sign-up would attribute a later one as well. */
            ReferralId = context.Referral?.CompletedUtc == null ? context.Referral?.Id : null,
            TimeZone = timeZone
        });

        context.NewMember = member;

        Country? country = null;
        if (location != null)
        {
            country = await _geolocationService.GetCountryFromLocation(location.Value);

            _unitOfWork.MemberLocationRepository.Add(new MemberLocation
            {
                CountryId = country?.Id,
                Latitude = location.Value.Lat,
                Longitude = location.Value.Long,
                MemberId = member.Id,
                Name = profile.LocationName
            });

            if (country?.CurrencyId != null)
            {
                _unitOfWork.MemberPaymentSettingsRepository.Add(new MemberPaymentSettings
                {
                    CurrencyId = country.CurrencyId,
                    MemberId = member.Id
                });
            }
        }

        _unitOfWork.MemberPreferencesRepository.Add(new MemberPreferences
        {
            DistanceUnit = country != null ? country.DistanceUnit : _distanceUnitFactory.GetDefault().Type,
            Locale = context.Request.HttpRequestContext.Locale,
            MemberId = member.Id
        });

        return StepOutcome.Continue();
    }
}
