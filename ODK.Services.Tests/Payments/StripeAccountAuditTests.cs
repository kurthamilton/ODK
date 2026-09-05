using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using ODK.Core.Members;
using ODK.Core.Payments;
using ODK.Core.Platforms;
using ODK.Services.Payments;
using ODK.Services.Payments.Models;

namespace ODK.Services.Tests.Payments;

[Parallelizable]
public static class StripeAccountAuditTests
{
    private const string ChargeId = "ch_1";

    private const string PaymentIntentId = "pi_1";

    private const string SubscriptionId = "sub_1";

    private static readonly Guid ChapterId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    private static readonly Guid ChapterSubscriptionId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    private static readonly Guid MemberId = Guid.Parse("33333333-3333-3333-3333-333333333333");

    private static readonly Guid PaymentCheckoutSessionId = Guid.Parse("44444444-4444-4444-4444-444444444444");

    private static readonly Guid PaymentId = Guid.Parse("55555555-5555-5555-5555-555555555555");

    private static readonly Guid SiteSubscriptionPriceId = Guid.Parse("66666666-6666-6666-6666-666666666666");

    private static readonly DateTime Now = new(2026, 3, 1, 12, 0, 0, DateTimeKind.Utc);

    [Test]
    public static void Audit_WhenSubscriptionMetadataIsComplete_ReportsNothing()
    {
        // Arrange
        var subscriptions = new[] { CreateSubscription(metadata: ChapterSubscriptionMetadata()) };
        var records = CreateRecords(memberSubscriptionRecords: [CreateMemberSubscriptionRecord()]);

        // Act
        var result = StripeAccountAudit.Audit(CreateAccount(), [], subscriptions, records);

        // Assert
        result.Subscriptions.Single().Findings.Should().BeEmpty();
    }

    [Test]
    public static void Audit_WhenSubscriptionHasNoMetadata_ReportsMetadataAbsent()
    {
        // Arrange
        var subscriptions = new[] { CreateSubscription(metadata: new Dictionary<string, string>()) };
        var records = CreateRecords(memberSubscriptionRecords: [CreateMemberSubscriptionRecord()]);

        // Act
        var result = StripeAccountAudit.Audit(CreateAccount(), [], subscriptions, records);

        // Assert
        result.Subscriptions
            .Single()
            .Findings
            .Select(x => x.Type)
            .Should()
            .Equal(StripeTransactionFindingType.MetadataAbsent);
    }

    [Test]
    public static void Audit_WhenSubscriptionNamesNeitherSubscriptionKind_ReportsRoutingKeyMissing()
    {
        // Arrange
        var metadata = ChapterSubscriptionMetadata();
        metadata.Remove(PaymentMetadataModel.Keys.ChapterSubscriptionId);

        var subscriptions = new[] { CreateSubscription(metadata: metadata) };
        var records = CreateRecords(memberSubscriptionRecords: [CreateMemberSubscriptionRecord()]);

        // Act
        var result = StripeAccountAudit.Audit(CreateAccount(), [], subscriptions, records);

        // Assert
        result.Subscriptions
            .Single()
            .Findings
            .Should()
            .ContainSingle(x => x.Type == StripeTransactionFindingType.RoutingKeyMissing);
    }

    [Test]
    public static void Audit_WhenSubscriptionOmitsMemberId_ReportsRequiredKeyMissing()
    {
        // Arrange
        var metadata = ChapterSubscriptionMetadata();
        metadata.Remove(PaymentMetadataModel.Keys.MemberId);

        var subscriptions = new[] { CreateSubscription(metadata: metadata) };
        var records = CreateRecords(memberSubscriptionRecords: [CreateMemberSubscriptionRecord()]);

        // Act
        var result = StripeAccountAudit.Audit(CreateAccount(), [], subscriptions, records);

        // Assert
        result.Subscriptions
            .Single()
            .Findings
            .Should()
            .ContainSingle(x => x.Type == StripeTransactionFindingType.RequiredKeyMissing
                && x.Key == PaymentMetadataModel.Keys.MemberId);
    }

    [Test]
    public static void Audit_WhenGroupSubscriptionOmitsChapterId_ReportsRequiredKeyMissing()
    {
        // Arrange
        var metadata = ChapterSubscriptionMetadata();
        metadata.Remove(PaymentMetadataModel.Keys.ChapterId);

        var subscriptions = new[] { CreateSubscription(metadata: metadata) };
        var records = CreateRecords(memberSubscriptionRecords: [CreateMemberSubscriptionRecord()]);

        // Act
        var result = StripeAccountAudit.Audit(CreateAccount(), [], subscriptions, records);

        // Assert
        result.Subscriptions
            .Single()
            .Findings
            .Should()
            .ContainSingle(x => x.Type == StripeTransactionFindingType.RequiredKeyMissing
                && x.Key == PaymentMetadataModel.Keys.ChapterId);
    }

    [Test]
    public static void Audit_WhenSiteSubscriptionOmitsChapterId_ReportsNothingAboutIt()
    {
        // Arrange
        var subscriptions = new[] { CreateSubscription(metadata: SiteSubscriptionMetadata()) };
        var records = CreateRecords(
            memberSiteSubscriptionRecords: [CreateMemberSiteSubscriptionRecord()]);

        // Act
        var result = StripeAccountAudit.Audit(CreateAccount(), [], subscriptions, records);

        // Assert
        result.Subscriptions.Single().Findings.Should().BeEmpty();
    }

    [Test]
    public static void Audit_WhenSubscriptionNamesAMemberThatDoesNotExist_ReportsUnknownReference()
    {
        // Arrange
        var subscriptions = new[] { CreateSubscription(metadata: ChapterSubscriptionMetadata()) };
        var records = CreateRecords(
            memberIds: new HashSet<Guid>(),
            memberSubscriptionRecords: [CreateMemberSubscriptionRecord()]);

        // Act
        var result = StripeAccountAudit.Audit(CreateAccount(), [], subscriptions, records);

        // Assert
        result.Subscriptions
            .Single()
            .Findings
            .Should()
            .ContainSingle(x => x.Type == StripeTransactionFindingType.UnknownReference
                && x.Key == PaymentMetadataModel.Keys.MemberId
                && x.Actual == MemberId.ToString());
    }

    [Test]
    public static void Audit_WhenSubscriptionNamesADifferentMemberThanItsRecord_ReportsDisagreesWithRecord()
    {
        // Arrange
        var otherMemberId = Guid.Parse("99999999-9999-9999-9999-999999999999");

        var subscriptions = new[] { CreateSubscription(metadata: ChapterSubscriptionMetadata()) };
        var records = CreateRecords(
            memberSubscriptionRecords: [CreateMemberSubscriptionRecord(memberId: otherMemberId)]);

        // Act
        var result = StripeAccountAudit.Audit(CreateAccount(), [], subscriptions, records);

        // Assert
        result.Subscriptions
            .Single()
            .Findings
            .Should()
            .ContainSingle(x => x.Type == StripeTransactionFindingType.DisagreesWithRecord
                && x.Key == PaymentMetadataModel.Keys.MemberId
                && x.Actual == MemberId.ToString()
                && x.Expected == otherMemberId.ToString());
    }

    [Test]
    public static void Audit_WhenSubscriptionCarriesTheCheckoutIds_ReportsNothing()
    {
        // Arrange - checkout writes one metadata dictionary to the session and to the subscription, so a
        // subscription names the payment and session of the purchase that created it. The first invoice is
        // what claims them, and every later invoice creates a payment of its own, so they are not a fault.
        var metadata = ChapterSubscriptionMetadata();
        metadata.Add(PaymentMetadataModel.Keys.PaymentId, PaymentId.ToString());
        metadata.Add(
            PaymentMetadataModel.Keys.PaymentCheckoutSessionId, PaymentCheckoutSessionId.ToString());

        var subscriptions = new[] { CreateSubscription(metadata: metadata) };
        var records = CreateRecords(memberSubscriptionRecords: [CreateMemberSubscriptionRecord()]);

        // Act
        var result = StripeAccountAudit.Audit(CreateAccount(), [], subscriptions, records);

        // Assert
        result.Subscriptions.Single().Findings.Should().BeEmpty();
    }

    [Test]
    public static void Audit_WhenSubscriptionMatchesNoRecord_ReportsNoDatabaseRecord()
    {
        // Arrange
        var subscriptions = new[] { CreateSubscription(metadata: ChapterSubscriptionMetadata()) };

        // Act
        var result = StripeAccountAudit.Audit(CreateAccount(), [], subscriptions, CreateRecords());

        // Assert
        result.Subscriptions
            .Single()
            .Findings
            .Should()
            .ContainSingle(x => x.Type == StripeTransactionFindingType.NoDatabaseRecord);
    }

    [Test]
    public static void Audit_WhenSubscriptionWillBillAgain_ReportsItsFindingsAsErrors()
    {
        // Arrange
        var subscriptions = new[]
        {
            CreateSubscription(
                metadata: new Dictionary<string, string>(), status: StripeSubscriptionStatus.Active)
        };

        // Act
        var result = StripeAccountAudit.Audit(CreateAccount(), [], subscriptions, CreateRecords());

        // Assert
        result.Subscriptions
            .Single()
            .Findings
            .Should()
            .OnlyContain(x => x.Severity == StripeFindingSeverity.Error);
    }

    [Test]
    public static void Audit_WhenSubscriptionIsCancelled_ReportsItsFindingsAsWarnings()
    {
        // Arrange
        var subscriptions = new[]
        {
            CreateSubscription(
                metadata: new Dictionary<string, string>(), status: StripeSubscriptionStatus.Cancelled)
        };

        // Act
        var result = StripeAccountAudit.Audit(CreateAccount(), [], subscriptions, CreateRecords());

        // Assert
        result.Subscriptions
            .Single()
            .Findings
            .Should()
            .OnlyContain(x => x.Severity == StripeFindingSeverity.Warning);
    }

    [Test]
    public static void Audit_WhenSubscriptionStatusIsUnrecognised_ReportsItsFindingsAsErrors()
    {
        // Arrange
        var subscriptions = new[]
        {
            CreateSubscription(
                metadata: new Dictionary<string, string>(), status: StripeSubscriptionStatus.None)
        };

        // Act
        var result = StripeAccountAudit.Audit(CreateAccount(), [], subscriptions, CreateRecords());

        // Assert
        result.Subscriptions
            .Single()
            .Findings
            .Should()
            .OnlyContain(x => x.Severity == StripeFindingSeverity.Error);
    }

    [Test]
    public static void Audit_WhenSubscriptionMatchesAGroupRecord_StatesTheMetadataItShouldCarry()
    {
        // Arrange
        var subscriptions = new[] { CreateSubscription(metadata: new Dictionary<string, string>()) };
        var records = CreateRecords(memberSubscriptionRecords: [CreateMemberSubscriptionRecord()]);

        // Act
        var result = StripeAccountAudit.Audit(CreateAccount(), [], subscriptions, records);

        // Assert
        result.Subscriptions
            .Single()
            .ExpectedMetadata
            .Should()
            .BeEquivalentTo(ChapterSubscriptionMetadata());
    }

    [Test]
    public static void Audit_WhenSubscriptionMatchesASiteRecord_StatesTheMetadataItShouldCarry()
    {
        // Arrange
        var subscriptions = new[] { CreateSubscription(metadata: new Dictionary<string, string>()) };
        var records = CreateRecords(
            memberSiteSubscriptionRecords: [CreateMemberSiteSubscriptionRecord()]);

        // Act
        var result = StripeAccountAudit.Audit(CreateAccount(), [], subscriptions, records);

        // Assert
        result.Subscriptions
            .Single()
            .ExpectedMetadata
            .Should()
            .BeEquivalentTo(SiteSubscriptionMetadata());
    }

    [Test]
    public static void Audit_WhenSubscriptionMatchesNoRecord_StatesNoExpectedMetadata()
    {
        // Arrange
        var subscriptions = new[] { CreateSubscription(metadata: new Dictionary<string, string>()) };

        // Act
        var result = StripeAccountAudit.Audit(CreateAccount(), [], subscriptions, CreateRecords());

        // Assert
        result.Subscriptions.Single().ExpectedMetadata.Should().BeNull();
    }

    [Test]
    public static void Audit_WhenSomeSubscriptionsHaveFindings_OrdersThoseFirst()
    {
        // Arrange
        var subscriptions = new[]
        {
            CreateSubscription(id: "sub_clean", metadata: ChapterSubscriptionMetadata()),
            CreateSubscription(id: "sub_broken", metadata: new Dictionary<string, string>())
        };

        var records = CreateRecords(memberSubscriptionRecords:
        [
            CreateMemberSubscriptionRecord(externalId: "sub_clean"),
            CreateMemberSubscriptionRecord(externalId: "sub_broken")
        ]);

        // Act
        var result = StripeAccountAudit.Audit(CreateAccount(), [], subscriptions, records);

        // Assert
        result.Subscriptions.First().Subscription.Id.Should().Be("sub_broken");
    }

    [Test]
    public static void Audit_WhenTransactionDidNotSucceed_ReportsNothingAboutIt()
    {
        // Arrange
        var transactions = new[]
        {
            CreateTransaction(
                metadata: new Dictionary<string, string>(), status: StripeTransactionStatus.Pending)
        };

        // Act
        var result = StripeAccountAudit.Audit(CreateAccount(), transactions, [], CreateRecords());

        // Assert
        result.Transactions.Single().Findings.Should().BeEmpty();
    }

    [Test]
    public static void Audit_WhenTransactionMatchesAPaymentByChargeId_ReportsNothing()
    {
        // Arrange
        var transactions = new[] { CreateTransaction(metadata: OneOffMetadata()) };
        var records = CreateRecords(payments: [CreatePayment()]);

        // Act
        var result = StripeAccountAudit.Audit(CreateAccount(), transactions, [], records);

        // Assert
        var audited = result.Transactions.Single();
        audited.Payment.Should().NotBeNull();
        audited.Findings.Should().BeEmpty();
    }

    [Test]
    public static void Audit_WhenTransactionMatchesNoPayment_ReportsNoDatabaseRecord()
    {
        // Arrange
        var transactions = new[] { CreateTransaction(metadata: OneOffMetadata()) };

        // Act
        var result = StripeAccountAudit.Audit(CreateAccount(), transactions, [], CreateRecords());

        // Assert
        var audited = result.Transactions.Single();
        audited.Payment.Should().BeNull();
        audited.Findings
            .Should()
            .Contain(x => x.Type == StripeTransactionFindingType.NoDatabaseRecord);
    }

    [Test]
    public static void Audit_WhenRenewalNamesTheFirstPurchasesPayment_DoesNotMatchThatPayment()
    {
        // Arrange
        var metadata = ChapterSubscriptionMetadata();
        metadata.Add(PaymentMetadataModel.Keys.PaymentId, PaymentId.ToString());

        var transactions = new[]
        {
            CreateTransaction(
                chargeId: null,
                paymentIntentId: null,
                subscriptionId: SubscriptionId,
                kind: StripeTransactionKind.SubscriptionRenewal,
                metadata: metadata)
        };

        // The purchase that created the subscription, a year before this renewal.
        var records = CreateRecords(payments:
        [
            CreatePayment(
                externalId: SubscriptionId,
                externalChargeId: null,
                paidUtc: Now.AddYears(-1))
        ]);

        // Act
        var result = StripeAccountAudit.Audit(CreateAccount(), transactions, [], records);

        // Assert
        result.Transactions.Single().Payment.Should().BeNull();
    }

    [Test]
    public static void Audit_WhenRenewalIsPaidWithinTheWindowOfOnePayment_MatchesIt()
    {
        // Arrange
        var transactions = new[]
        {
            CreateTransaction(
                chargeId: null,
                paymentIntentId: null,
                subscriptionId: SubscriptionId,
                kind: StripeTransactionKind.SubscriptionRenewal,
                metadata: ChapterSubscriptionMetadata())
        };

        var records = CreateRecords(payments:
        [
            CreatePayment(
                externalId: SubscriptionId, externalChargeId: null, paidUtc: Now.AddHours(1))
        ]);

        // Act
        var result = StripeAccountAudit.Audit(CreateAccount(), transactions, [], records);

        // Assert
        result.Transactions.Single().Payment.Should().NotBeNull();
    }

    [Test]
    public static void Audit_WhenTwoPaymentsShareASubscriptionIdInTheWindow_MatchesNeither()
    {
        // Arrange
        var transactions = new[]
        {
            CreateTransaction(
                chargeId: null,
                paymentIntentId: null,
                subscriptionId: SubscriptionId,
                kind: StripeTransactionKind.SubscriptionRenewal,
                metadata: ChapterSubscriptionMetadata())
        };

        var records = CreateRecords(payments:
        [
            CreatePayment(
                id: Guid.NewGuid(),
                externalId: SubscriptionId,
                externalChargeId: null,
                paidUtc: Now.AddHours(1)),
            CreatePayment(
                id: Guid.NewGuid(),
                externalId: SubscriptionId,
                externalChargeId: null,
                paidUtc: Now.AddHours(2))
        ]);

        // Act
        var result = StripeAccountAudit.Audit(CreateAccount(), transactions, [], records);

        // Assert
        result.Transactions.Single().Payment.Should().BeNull();
    }

    [Test]
    public static void Audit_WhenTransactionAmountDiffersFromItsPayment_ReportsAmountDisagrees()
    {
        // Arrange
        var transactions = new[] { CreateTransaction(amount: 10m, metadata: OneOffMetadata()) };
        var records = CreateRecords(payments: [CreatePayment(amount: 12m)]);

        // Act
        var result = StripeAccountAudit.Audit(CreateAccount(), transactions, [], records);

        // Assert
        result.Transactions
            .Single()
            .Findings
            .Should()
            .ContainSingle(x => x.Type == StripeTransactionFindingType.AmountDisagrees
                && x.Actual == "10"
                && x.Expected == "12");
    }

    [Test]
    public static void Audit_WhenPaymentHasBeenSettled_MeasuresTheAmountAgainstWhatItActuallyTook()
    {
        // Arrange
        var transactions = new[] { CreateTransaction(amount: 10m, metadata: OneOffMetadata()) };
        var records = CreateRecords(payments: [CreatePayment(amount: 12m, actualAmount: 10m)]);

        // Act
        var result = StripeAccountAudit.Audit(CreateAccount(), transactions, [], records);

        // Assert
        result.Transactions
            .Single()
            .Findings
            .Should()
            .NotContain(x => x.Type == StripeTransactionFindingType.AmountDisagrees);
    }

    [Test]
    public static void Audit_WhenOneOffOmitsPaymentCheckoutSessionId_ReportsRequiredKeyMissing()
    {
        // Arrange
        var metadata = OneOffMetadata();
        metadata.Remove(PaymentMetadataModel.Keys.PaymentCheckoutSessionId);

        var transactions = new[] { CreateTransaction(metadata: metadata) };
        var records = CreateRecords(payments: [CreatePayment()]);

        // Act
        var result = StripeAccountAudit.Audit(CreateAccount(), transactions, [], records);

        // Assert
        result.Transactions
            .Single()
            .Findings
            .Should()
            .ContainSingle(x => x.Type == StripeTransactionFindingType.RequiredKeyMissing
                && x.Key == PaymentMetadataModel.Keys.PaymentCheckoutSessionId);
    }

    [Test]
    public static void Audit_WhenOneOffIsAnEventTicket_DoesNotRequireAGroupSubscription()
    {
        // Arrange
        var metadata = OneOffMetadata();
        metadata.Remove(PaymentMetadataModel.Keys.ChapterId);
        metadata.Remove(PaymentMetadataModel.Keys.ChapterSubscriptionId);
        metadata.Add(PaymentMetadataModel.Keys.EventTicketPaymentId, Guid.NewGuid().ToString());

        var transactions = new[] { CreateTransaction(metadata: metadata) };
        var records = CreateRecords(payments: [CreatePayment()]);

        // Act
        var result = StripeAccountAudit.Audit(CreateAccount(), transactions, [], records);

        // Assert
        result.Transactions.Single().Findings.Should().BeEmpty();
    }

    [Test]
    public static void Audit_WhenAPaidPaymentMatchesNoTransaction_ReportsItUnaccountedFor()
    {
        // Arrange
        var records = CreateRecords(payments: [CreatePayment()]);

        // Act
        var result = StripeAccountAudit.Audit(CreateAccount(), [], [], records);

        // Assert
        result.UnaccountedPayments.Should().ContainSingle(x => x.Id == PaymentId);
    }

    [Test]
    public static void Audit_WhenAPaymentIsUnpaid_DoesNotReportItUnaccountedFor()
    {
        // Arrange
        var payment = CreatePayment();
        payment.PaidUtc = null;

        var records = CreateRecords(payments: [payment]);

        // Act
        var result = StripeAccountAudit.Audit(CreateAccount(), [], [], records);

        // Assert
        result.UnaccountedPayments.Should().BeEmpty();
    }

    [Test]
    public static void Audit_WhenACurrentRecordNamesAMissingSubscription_ReportsItUnaccountedFor()
    {
        // Arrange
        var records = CreateRecords(memberSubscriptionRecords: [CreateMemberSubscriptionRecord()]);

        // Act
        var result = StripeAccountAudit.Audit(CreateAccount(), [], [], records);

        // Assert
        result.UnaccountedMemberSubscriptionRecords.Should().ContainSingle();
    }

    [Test]
    public static void Audit_WhenARecordIsCancelled_DoesNotReportItUnaccountedFor()
    {
        // Arrange
        var records = CreateRecords(memberSubscriptionRecords:
        [
            CreateMemberSubscriptionRecord(cancelledUtc: Now)
        ]);

        // Act
        var result = StripeAccountAudit.Audit(CreateAccount(), [], [], records);

        // Assert
        result.UnaccountedMemberSubscriptionRecords.Should().BeEmpty();
    }

    [Test]
    public static void Audit_WhenTheSubscriptionARecordNamesIsInTheAccount_DoesNotReportItUnaccountedFor()
    {
        // Arrange
        var subscriptions = new[] { CreateSubscription(metadata: ChapterSubscriptionMetadata()) };
        var records = CreateRecords(memberSubscriptionRecords: [CreateMemberSubscriptionRecord()]);

        // Act
        var result = StripeAccountAudit.Audit(CreateAccount(), [], subscriptions, records);

        // Assert
        result.UnaccountedMemberSubscriptionRecords.Should().BeEmpty();
    }

    private static Dictionary<string, string> ChapterSubscriptionMetadata()
        => new()
        {
            { PaymentMetadataModel.Keys.ChapterId, ChapterId.ToString() },
            { PaymentMetadataModel.Keys.ChapterSubscriptionId, ChapterSubscriptionId.ToString() },
            { PaymentMetadataModel.Keys.MemberId, MemberId.ToString() },
            { PaymentMetadataModel.Keys.Platform, PlatformType.Default.ToString() },
            { PaymentMetadataModel.Keys.Reason, PaymentReasonType.ChapterSubscription.ToString() }
        };

    private static StripePaymentAccount CreateAccount(PlatformType platform = PlatformType.Default)
        => new()
        {
            AccountId = "acct_1",
            Environment = EnvironmentType.Prod,
            Platform = platform
        };

    private static MemberSiteSubscriptionRecord CreateMemberSiteSubscriptionRecord(
        Guid? memberId = null,
        Guid? siteSubscriptionPriceId = null,
        string? externalId = SubscriptionId,
        DateTime? cancelledUtc = null,
        bool isCurrent = true)
        => new()
        {
            CancelledUtc = cancelledUtc,
            CreatedUtc = Now,
            ExternalId = externalId,
            Id = Guid.NewGuid(),
            IsCurrent = isCurrent,
            MemberId = memberId ?? MemberId,
            SiteSubscriptionId = Guid.NewGuid(),
            SiteSubscriptionPriceId = siteSubscriptionPriceId ?? SiteSubscriptionPriceId
        };

    private static MemberSubscriptionRecord CreateMemberSubscriptionRecord(
        Guid? memberId = null,
        Guid? chapterId = null,
        Guid? chapterSubscriptionId = null,
        string? externalId = SubscriptionId,
        DateTime? cancelledUtc = null,
        bool isCurrent = true)
        => new()
        {
            CancelledUtc = cancelledUtc,
            ChapterId = chapterId ?? ChapterId,
            ChapterSubscriptionId = chapterSubscriptionId ?? ChapterSubscriptionId,
            ExternalId = externalId,
            Id = Guid.NewGuid(),
            IsCurrent = isCurrent,
            MemberId = memberId ?? MemberId,
            PurchasedUtc = Now
        };

    private static Payment CreatePayment(
        Guid? id = null,
        decimal amount = 10m,
        decimal? actualAmount = null,
        string? externalId = PaymentIntentId,
        string? externalChargeId = ChargeId,
        DateTime? paidUtc = null)
        => new()
        {
            ActualAmount = actualAmount,
            Amount = amount,
            ExternalChargeId = externalChargeId,
            ExternalId = externalId,
            Id = id ?? PaymentId,
            MemberId = MemberId,
            PaidUtc = paidUtc ?? Now,
            PaymentProvider = PaymentProviderType.Stripe
        };

    private static StripeTransactionRecords CreateRecords(
        IReadOnlyCollection<Payment>? payments = null,
        IReadOnlyCollection<MemberSubscriptionRecord>? memberSubscriptionRecords = null,
        IReadOnlyCollection<MemberSiteSubscriptionRecord>? memberSiteSubscriptionRecords = null,
        IReadOnlySet<Guid>? memberIds = null)
        => new()
        {
            ChapterIds = new HashSet<Guid> { ChapterId },
            ChapterSubscriptionIds = new HashSet<Guid> { ChapterSubscriptionId },
            MemberIds = memberIds ?? new HashSet<Guid> { MemberId },
            MemberSiteSubscriptionRecords = memberSiteSubscriptionRecords ?? [],
            MemberSubscriptionRecords = memberSubscriptionRecords ?? [],
            PaymentCheckoutSessionIds = new HashSet<Guid> { PaymentCheckoutSessionId },
            Payments = payments ?? [],
            SiteSubscriptionPriceIds = new HashSet<Guid> { SiteSubscriptionPriceId }
        };

    private static StripeSubscription CreateSubscription(
        string id = SubscriptionId,
        IReadOnlyDictionary<string, string>? metadata = null,
        StripeSubscriptionStatus status = StripeSubscriptionStatus.Active)
        => new()
        {
            CreatedUtc = Now,
            CustomerId = "cus_1",
            Id = id,
            Metadata = metadata ?? new Dictionary<string, string>(),
            Status = status
        };

    private static StripeTransaction CreateTransaction(
        decimal amount = 10m,
        string? chargeId = ChargeId,
        string? paymentIntentId = PaymentIntentId,
        string? subscriptionId = null,
        StripeTransactionKind kind = StripeTransactionKind.OneOff,
        IReadOnlyDictionary<string, string>? metadata = null,
        StripeTransactionStatus status = StripeTransactionStatus.Succeeded)
        => new()
        {
            Amount = amount,
            ChargeId = chargeId,
            CreatedUtc = Now,
            CurrencyCode = "gbp",
            InvoiceId = subscriptionId != null ? "in_1" : null,
            Kind = kind,
            Metadata = metadata ?? new Dictionary<string, string>(),
            PaidUtc = Now,
            PaymentIntentId = paymentIntentId,
            Status = status,
            SubscriptionId = subscriptionId
        };

    private static Dictionary<string, string> OneOffMetadata()
    {
        var metadata = ChapterSubscriptionMetadata();
        metadata.Add(PaymentMetadataModel.Keys.PaymentId, PaymentId.ToString());
        metadata.Add(
            PaymentMetadataModel.Keys.PaymentCheckoutSessionId, PaymentCheckoutSessionId.ToString());
        return metadata;
    }

    private static Dictionary<string, string> SiteSubscriptionMetadata()
        => new()
        {
            { PaymentMetadataModel.Keys.MemberId, MemberId.ToString() },
            { PaymentMetadataModel.Keys.Platform, PlatformType.Default.ToString() },
            { PaymentMetadataModel.Keys.Reason, PaymentReasonType.SiteSubscription.ToString() },
            { PaymentMetadataModel.Keys.SiteSubscriptionPriceId, SiteSubscriptionPriceId.ToString() }
        };
}
