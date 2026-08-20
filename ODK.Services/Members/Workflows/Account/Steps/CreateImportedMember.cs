using ODK.Core.Countries;
using ODK.Core.Members;
using ODK.Core.Workflows;
using ODK.Data.Core;
using ODK.Services.Subscriptions;

namespace ODK.Services.Members.Workflows.Account.Steps;

/// <summary>
/// Raises an account for an address an admin imported. Nobody has signed up for it, so it has no picture and
/// no reCAPTCHA score, and it starts where the group is: its location, its currency, its country's units.
/// </summary>
/// <remarks>
/// The activation token is deliberately not scoped to the group, because an import is not a sign-up to it. On
/// Drunken Knitwits that leaves the token unusable by the chapter-scoped activation page, which is why nothing
/// there emails it - a new member is sent the invitation instead.
/// </remarks>
public sealed class CreateImportedMember : IStep<AccountContext>
{
    private readonly IDistanceUnitFactory _distanceUnitFactory;
    private readonly IMemberSiteSubscriptionWriter _memberSiteSubscriptionWriter;
    private readonly IUnitOfWork _unitOfWork;

    public CreateImportedMember(
        IUnitOfWork unitOfWork,
        IDistanceUnitFactory distanceUnitFactory,
        IMemberSiteSubscriptionWriter memberSiteSubscriptionWriter)
    {
        _distanceUnitFactory = distanceUnitFactory;
        _memberSiteSubscriptionWriter = memberSiteSubscriptionWriter;
        _unitOfWork = unitOfWork;
    }

    public static string Description => "raises an account for the imported address";

    public static StepKind Kind => StepKind.Write;

    public Task<StepOutcome> Execute(AccountContext context, CancellationToken cancellationToken)
    {
        var import = context.RequiredImport;
        var chapter = context.RequiredChapter;

        var member = _unitOfWork.MemberRepository.Add(new Member
        {
            CreatedUtc = DateTime.UtcNow,
            EmailAddress = import.EmailAddress,
            FirstName = import.FirstName,
            LastName = import.LastName,
            Platform = context.Request.Platform,
            TimeZone = chapter.TimeZone
        });

        context.NewMember = member;

        if (context.ChapterLocation != null)
        {
            _unitOfWork.MemberLocationRepository.Add(new MemberLocation
            {
                CountryId = chapter.CountryId,
                Latitude = context.ChapterLocation.Latitude,
                Longitude = context.ChapterLocation.Longitude,
                MemberId = member.Id,
                Name = context.ChapterLocation.Name
            });
        }

        if (context.Currency != null)
        {
            _unitOfWork.MemberPaymentSettingsRepository.Add(new MemberPaymentSettings
            {
                CurrencyId = context.Currency.Id,
                MemberId = member.Id
            });
        }

        _unitOfWork.MemberPreferencesRepository.Add(new MemberPreferences
        {
            DistanceUnit = context.Country != null
                ? context.Country.DistanceUnit
                : _distanceUnitFactory.GetDefault().Type,
            MemberId = member.Id
        });

        /* The overload that takes the existing record, passing none: the account was created a moment ago, so
           there cannot be one. Looking it up would cost a query per row of the file. */
        _memberSiteSubscriptionWriter.MakeRecordCurrent(
            newRecord: new MemberSiteSubscriptionRecord
            {
                CreatedUtc = DateTime.UtcNow,
                MemberId = member.Id,
                SiteSubscriptionId = context.RequiredSiteSubscription.Id
            },
            existingCurrent: null);

        _unitOfWork.MemberActivationTokenRepository.Add(new MemberActivationToken
        {
            ActivationToken = context.RequiredActivationToken,
            MemberId = member.Id
        });

        return Task.FromResult(StepOutcome.Continue());
    }
}
