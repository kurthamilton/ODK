using ODK.Core.Countries;
using ODK.Core.Members;
using ODK.Core.Payments;
using ODK.Core.Platforms;
using ODK.Data.Core;
using ODK.Services.Logging;
using ODK.Services.Payments.Models;
using ODK.Services.Payments.ViewModels;

namespace ODK.Services.Payments;

/// <summary>
/// Reads the browsing platform's Stripe account and says which of its objects our records account for.
/// </summary>
/// <remarks>
/// Two database round trips, and the second genuinely depends on the first: which rows exist can only be
/// asked once the ids are known, and those come from the Stripe metadata <em>and</em> from the records the
/// first read returned - a subscription with no metadata at all is named by its record or by nothing.
/// </remarks>
public class StripeTransactionAdminService : OdkAdminServiceBase, IStripeTransactionAdminService
{
    private const string AccountPlaceholder = "{account}";

    private const string IdPlaceholder = "{id}";

    private readonly ILoggingService _loggingService;
    private readonly IPaymentProviderFactory _paymentProviderFactory;
    private readonly StripeTransactionAdminServiceSettings _settings;
    private readonly IUnitOfWork _unitOfWork;

    public StripeTransactionAdminService(
        IUnitOfWork unitOfWork,
        IPaymentProviderFactory paymentProviderFactory,
        ILoggingService loggingService,
        StripeTransactionAdminServiceSettings settings)
        : base(unitOfWork)
    {
        _loggingService = loggingService;
        _paymentProviderFactory = paymentProviderFactory;
        _settings = settings;
        _unitOfWork = unitOfWork;
    }

    public async Task<SiteAdminStripeTransactionsViewModel> GetStripeTransactionsViewModel(
        IMemberServiceRequest request)
    {
        var (environment, platform, timeZone) =
            (request.Environment, request.Platform, request.CurrentMember.TimeZone);

        var account = new StripePaymentAccount
        {
            AccountId = _settings.AccountIds.TryGetValue(platform, out var accountId)
                ? accountId
                : string.Empty,
            Environment = environment,
            Platform = platform
        };

        /* Every record that could answer for something in the account, before the account is asked - the
           read is site-admin-only, and this is what asserts that. Neither subscription read can be narrowed
           by what Stripe holds: a record naming a subscription the account does not have is a finding, so
           narrowing to the ids Stripe returned would hide exactly what is being looked for. */
        var (payments, memberSubscriptionRecords, memberSiteSubscriptionRecords) =
            await GetSiteAdminRestrictedContent(
                request,
                x => x.PaymentRepository
                    .Query()
                    .ForEnvironment(environment)
                    .ForPlatform(platform)
                    .GetAll(),
                x => x.MemberSubscriptionRecordRepository.Query().HasExternalId().GetAll(),
                x => x.MemberSiteSubscriptionRecordRepository.Query().HasExternalId().GetAll());

        var provider = _paymentProviderFactory.GetStripeTransactionProvider(platform);
        if (provider == null)
        {
            return Unreadable(account, timeZone, "Provider does not support transactions");
        }

        IReadOnlyCollection<StripeTransaction> transactions;
        IReadOnlyCollection<StripeSubscription> subscriptions;

        try
        {
            transactions = await provider.ListTransactions();
            subscriptions = await provider.ListSubscriptions();
        }
        catch (Exception ex)
        {
            await _loggingService.Error($"Error listing Stripe transactions for '{platform}'", ex);
            return Unreadable(account, timeZone, ex.Message);
        }

        var metadata = transactions
            .Select(x => x.Metadata)
            .Concat(subscriptions.Select(x => x.Metadata))
            .Select(PaymentMetadataModel.FromDictionary)
            .ToArray();

        /* The ids to ask about: what the metadata names, so a dangling reference can be reported, and what
           the records name, so a row whose metadata says nothing is still named after the member and group
           it belongs to. A record's own ids always exist, so adding them cannot make a missing row look
           present - it only widens what is asked. */
        var chapterIds = Ids(metadata, x => x.ChapterId)
            .Concat(memberSubscriptionRecords.Select(x => x.ChapterId))
            .Concat(payments.Where(x => x.ChapterId != null).Select(x => x.ChapterId!.Value))
            .ToHashSet();

        var memberIds = Ids(metadata, x => x.MemberId)
            .Concat(memberSubscriptionRecords.Select(x => x.MemberId))
            .Concat(memberSiteSubscriptionRecords.Select(x => x.MemberId))
            .Concat(payments.Select(x => x.MemberId))
            .ToHashSet();

        var (members, chapters, chapterSubscriptions, siteSubscriptionPrices, checkoutSessions, currencies) =
            await _unitOfWork.Run(
                x => x.MemberRepository.GetByIds(memberIds),
                /* Default, which ChapterRepository reads as no platform filter, so this is a lookup by id
                   alone. The question is whether a chapter exists, and one the metadata names exists
                   whichever platform it belongs to. */
                x => x.ChapterRepository.GetByIds(PlatformType.Default, chapterIds),
                x => x.ChapterSubscriptionRepository.GetByIds(
                    Ids(metadata, y => y.ChapterSubscriptionId).ToHashSet()),
                x => x.SiteSubscriptionPriceRepository.GetByIds(
                    Ids(metadata, y => y.SiteSubscriptionPriceId).ToHashSet()),
                x => x.PaymentCheckoutSessionRepository.GetByIds(
                    Ids(metadata, y => y.PaymentCheckoutSessionId).ToHashSet()),
                x => x.CurrencyRepository.GetAll());

        var audit = StripeAccountAudit.Audit(
            account,
            transactions,
            subscriptions,
            new StripeTransactionRecords
            {
                ChapterIds = chapters.Select(x => x.Id).ToHashSet(),
                ChapterSubscriptionIds = chapterSubscriptions.Select(x => x.Id).ToHashSet(),
                MemberIds = members.Select(x => x.Id).ToHashSet(),
                MemberSiteSubscriptionRecords = memberSiteSubscriptionRecords,
                MemberSubscriptionRecords = memberSubscriptionRecords,
                PaymentCheckoutSessionIds = checkoutSessions.Select(x => x.Id).ToHashSet(),
                Payments = payments,
                SiteSubscriptionPriceIds = siteSubscriptionPrices.Select(x => x.Id).ToHashSet()
            });

        var chapterNames = chapters.ToDictionary(x => x.Id, x => x.Name);
        var memberNames = members.ToDictionary(x => x.Id, x => x.FullName);
        var currenciesByCode = currencies.ToDictionary(x => x.Code, StringComparer.OrdinalIgnoreCase);
        var currenciesById = currencies.ToDictionary(x => x.Id);

        return new SiteAdminStripeTransactionsViewModel
        {
            Account = account,
            Error = null,
            Subscriptions =
            [
                .. audit.Subscriptions.Select(x => ToSubscriptionViewModel(account, x, memberNames, chapterNames))
            ],
            TimeZone = timeZone,
            Transactions =
            [
                .. audit.Transactions.Select(x => ToTransactionViewModel(account, x, memberNames, currenciesByCode))
            ],
            UnaccountedPayments =
            [
                .. audit.UnaccountedPayments.Select(x => ToUnaccountedPayment(x, memberNames, currenciesById))
            ],
            UnaccountedSubscriptions =
            [
                .. audit.UnaccountedMemberSubscriptionRecords
                    .Select(x => ToUnaccountedSubscription(x, memberNames, chapterNames))
                    .Concat(audit.UnaccountedMemberSiteSubscriptionRecords
                        .Select(x => ToUnaccountedSubscription(x, memberNames)))
            ]
        };
    }

    private static string AmountDisplay(decimal amount, Currency? currency, string? currencyCode)
        => currency != null
            ? currency.ToAmountString(amount)
            : $"{Currency.ToValueString(amount)} {currencyCode?.ToUpperInvariant()}".TrimEnd();

    private static IEnumerable<Guid> Ids(
        IEnumerable<PaymentMetadataModel> metadata, Func<PaymentMetadataModel, Guid?> select)
        => metadata
            .Select(select)
            .Where(x => x != null)
            .Select(x => x!.Value);

    /// <summary>
    /// The name of the first of <paramref name="ids"/> that resolves to one, so a row can prefer what the
    /// metadata says and fall back to what the record says without each call site spelling that out.
    /// </summary>
    private static string? ResolveName(IReadOnlyDictionary<Guid, string> names, params Guid?[] ids)
        => ids
            .Where(x => x != null)
            .Select(x => names.TryGetValue(x!.Value, out var name) ? name : null)
            .FirstOrDefault(x => x != null);

    private static SiteAdminStripeUnaccountedPaymentViewModel ToUnaccountedPayment(
        Payment payment,
        IReadOnlyDictionary<Guid, string> memberNames,
        IReadOnlyDictionary<Guid, Currency> currenciesById)
        => new()
        {
            /* Both amounts are in the payment's own currency, so the currency it names is the one to show -
               not the settlement code, which names the balance the money landed in. */
            AmountDisplay = AmountDisplay(
                payment.ActualAmount ?? payment.Amount,
                currenciesById.TryGetValue(payment.CurrencyId, out var currency) ? currency : null,
                currencyCode: null),
            ExternalId = payment.ExternalChargeId ?? payment.ExternalId,
            Id = payment.Id,
            MemberName = ResolveName(memberNames, payment.MemberId),
            PaidUtc = payment.PaidUtc,
            Reference = payment.Reference
        };

    private static SiteAdminStripeUnaccountedSubscriptionViewModel ToUnaccountedSubscription(
        MemberSiteSubscriptionRecord record, IReadOnlyDictionary<Guid, string> memberNames)
        => new()
        {
            ChapterName = null,
            ExpiresUtc = record.ExpiresUtc,
            ExternalId = record.ExternalId ?? string.Empty,
            Id = record.Id,
            MemberName = ResolveName(memberNames, record.MemberId),
            Type = StripeSubscriptionRecordType.Site
        };

    private static SiteAdminStripeUnaccountedSubscriptionViewModel ToUnaccountedSubscription(
        MemberSubscriptionRecord record,
        IReadOnlyDictionary<Guid, string> memberNames,
        IReadOnlyDictionary<Guid, string> chapterNames)
        => new()
        {
            ChapterName = ResolveName(chapterNames, record.ChapterId),
            ExpiresUtc = record.ExpiresUtc,
            ExternalId = record.ExternalId ?? string.Empty,
            Id = record.Id,
            MemberName = ResolveName(memberNames, record.MemberId),
            Type = StripeSubscriptionRecordType.Group
        };

    private static SiteAdminStripeTransactionsViewModel Unreadable(
        StripePaymentAccount account, TimeZoneInfo timeZone, string error)
        => new()
        {
            Account = account,
            Error = error,
            Subscriptions = [],
            TimeZone = timeZone,
            Transactions = [],
            /* Nothing is reported as unaccounted for, because nothing was read - an account that could not
               be listed is not an account holding none of our payments. */
            UnaccountedPayments = [],
            UnaccountedSubscriptions = []
        };

    private string? DashboardUrl(
        StripePaymentAccount account, string liveFormat, string testFormat, string? id)
    {
        /* An account whose environment is unset says nothing about which of Stripe's two dashboards holds
           it, and a link into the wrong one is worse than no link at all. */
        var format = account.Environment switch
        {
            EnvironmentType.None => null,
            EnvironmentType.Prod => liveFormat,
            _ => testFormat
        };

        return !string.IsNullOrWhiteSpace(account.AccountId)
            && !string.IsNullOrWhiteSpace(format)
            && !string.IsNullOrWhiteSpace(id)
                ? format.Replace(AccountPlaceholder, account.AccountId).Replace(IdPlaceholder, id)
                : null;
    }

    private SiteAdminStripeSubscriptionViewModel ToSubscriptionViewModel(
        StripePaymentAccount account,
        StripeSubscriptionAudit audit,
        IReadOnlyDictionary<Guid, string> memberNames,
        IReadOnlyDictionary<Guid, string> chapterNames)
        => new()
        {
            ChapterName = ResolveName(
                chapterNames, audit.Metadata.ChapterId, audit.MemberSubscriptionRecord?.ChapterId),
            CreatedUtc = audit.Subscription.CreatedUtc,
            DashboardUrl = DashboardUrl(
                account,
                _settings.LiveSubscriptionUrlFormat,
                _settings.TestSubscriptionUrlFormat,
                audit.Subscription.Id),
            ExpectedMetadata = audit.ExpectedMetadata,
            Findings = audit.Findings,
            Id = audit.Subscription.Id,
            MemberName = ResolveName(
                memberNames,
                audit.Metadata.MemberId,
                audit.MemberSubscriptionRecord?.MemberId,
                audit.MemberSiteSubscriptionRecord?.MemberId),
            Metadata = audit.Subscription.Metadata,
            Status = audit.Subscription.Status
        };

    private SiteAdminStripeTransactionViewModel ToTransactionViewModel(
        StripePaymentAccount account,
        StripeTransactionAudit audit,
        IReadOnlyDictionary<Guid, string> memberNames,
        IReadOnlyDictionary<string, Currency> currenciesByCode)
    {
        var transaction = audit.Transaction;

        // The payment intent where there is one, because that is the page Stripe's dashboard opens a charge
        // on; the invoice otherwise, which is all an invoice settled by anything else names.
        var (liveFormat, testFormat, id) = transaction.PaymentIntentId != null
            ? (_settings.LivePaymentUrlFormat, _settings.TestPaymentUrlFormat, transaction.PaymentIntentId)
            : (_settings.LiveInvoiceUrlFormat, _settings.TestInvoiceUrlFormat, transaction.InvoiceId);

        return new SiteAdminStripeTransactionViewModel
        {
            AmountDisplay = AmountDisplay(
                transaction.Amount,
                currenciesByCode.TryGetValue(transaction.CurrencyCode, out var currency) ? currency : null,
                transaction.CurrencyCode),
            ChargeId = transaction.ChargeId,
            CreatedUtc = transaction.CreatedUtc,
            DashboardUrl = DashboardUrl(account, liveFormat, testFormat, id),
            Findings = audit.Findings,
            InvoiceId = transaction.InvoiceId,
            Kind = transaction.Kind,
            MemberName = ResolveName(memberNames, audit.Metadata.MemberId, audit.Payment?.MemberId),
            Metadata = transaction.Metadata,
            PaymentId = audit.Payment?.Id,
            PaymentIntentId = transaction.PaymentIntentId,
            Reference = audit.Payment?.Reference,
            Status = transaction.Status,
            SubscriptionId = transaction.SubscriptionId
        };
    }
}
