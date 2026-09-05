using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using ODK.Core.Members;
using ODK.Core.Payments;
using ODK.Core.Platforms;
using ODK.Services.Payments.Models;

namespace ODK.Services.Tests.Payments;

[Parallelizable]
public static class StripeRenewalBackfillTests
{
    private const string SubscriptionId = "sub_1";

    private static readonly Guid ChapterId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    private static readonly Guid ChapterSubscriptionId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    private static readonly Guid MemberId = Guid.Parse("33333333-3333-3333-3333-333333333333");

    private static readonly Guid RecordChapterId = Guid.Parse("44444444-4444-4444-4444-444444444444");

    private static readonly Guid RecordMemberId = Guid.Parse("55555555-5555-5555-5555-555555555555");

    private static readonly Guid SiteSubscriptionPriceId = Guid.Parse("66666666-6666-6666-6666-666666666666");

    private static readonly DateTime Now = new(2026, 3, 1, 12, 0, 0, DateTimeKind.Utc);

    [Test]
    public static void Resolve_WhenGroupRecordNamesTheSubscription_ResolvesFromTheRecord()
    {
        // Arrange
        var audit = CreateAudit();
        var records = CreateRecords(memberSubscriptionRecords: [CreateMemberSubscriptionRecord()]);

        // Act
        var result = StripeRenewalBackfill.Resolve(audit, records);

        // Assert
        result.CanBackfill.Should().BeTrue();
        result.ChapterId.Should().Be(RecordChapterId);
        result.ChapterSubscriptionId.Should().Be(ChapterSubscriptionId);
        result.MemberId.Should().Be(RecordMemberId);
        result.SiteSubscriptionPriceId.Should().BeNull();
    }

    [Test]
    public static void Resolve_WhenSiteRecordNamesTheSubscription_ResolvesFromTheRecord()
    {
        // Arrange
        var audit = CreateAudit();
        var records = CreateRecords(
            memberSiteSubscriptionRecords: [CreateMemberSiteSubscriptionRecord()]);

        // Act
        var result = StripeRenewalBackfill.Resolve(audit, records);

        // Assert
        result.CanBackfill.Should().BeTrue();
        result.ChapterId.Should().BeNull();
        result.ChapterSubscriptionId.Should().BeNull();
        result.MemberId.Should().Be(RecordMemberId);
        result.SiteSubscriptionPriceId.Should().Be(SiteSubscriptionPriceId);
    }

    [Test]
    public static void Resolve_WhenRecordAndMetadataDisagree_PrefersTheRecord()
    {
        // Arrange
        var audit = CreateAudit(metadata: ChapterSubscriptionMetadata());
        var records = CreateRecords(memberSubscriptionRecords: [CreateMemberSubscriptionRecord()]);

        // Act
        var result = StripeRenewalBackfill.Resolve(audit, records);

        // Assert
        result.MemberId.Should().Be(RecordMemberId);
        result.ChapterId.Should().Be(RecordChapterId);
    }

    [Test]
    public static void Resolve_WhenNoRecordNamesTheSubscription_ResolvesFromTheMetadata()
    {
        // Arrange
        var audit = CreateAudit(metadata: ChapterSubscriptionMetadata());
        var records = CreateRecords();

        // Act
        var result = StripeRenewalBackfill.Resolve(audit, records);

        // Assert
        result.CanBackfill.Should().BeTrue();
        result.ChapterId.Should().Be(ChapterId);
        result.ChapterSubscriptionId.Should().Be(ChapterSubscriptionId);
        result.MemberId.Should().Be(MemberId);
    }

    [Test]
    public static void Resolve_WhenSiteMetadataNamesThePrice_ResolvesFromTheMetadata()
    {
        // Arrange
        var audit = CreateAudit(metadata: SiteSubscriptionMetadata());
        var records = CreateRecords();

        // Act
        var result = StripeRenewalBackfill.Resolve(audit, records);

        // Assert
        result.CanBackfill.Should().BeTrue();
        result.ChapterId.Should().BeNull();
        result.MemberId.Should().Be(MemberId);
        result.SiteSubscriptionPriceId.Should().Be(SiteSubscriptionPriceId);
    }

    [Test]
    public static void Resolve_WhenMetadataNamesAGroupSubscriptionThatIsGone_CannotBackfill()
    {
        // Arrange
        var audit = CreateAudit(metadata: ChapterSubscriptionMetadata());
        var records = CreateRecords(chapterSubscriptionIds: new HashSet<Guid>());

        // Act
        var result = StripeRenewalBackfill.Resolve(audit, records);

        // Assert
        result.CanBackfill.Should().BeFalse();
    }

    [Test]
    public static void Resolve_WhenMetadataNamesAMemberThatIsGone_CannotBackfill()
    {
        // Arrange
        var audit = CreateAudit(metadata: ChapterSubscriptionMetadata());
        var records = CreateRecords(memberIds: new HashSet<Guid>());

        // Act
        var result = StripeRenewalBackfill.Resolve(audit, records);

        // Assert
        result.CanBackfill.Should().BeFalse();
    }

    [Test]
    public static void Resolve_WhenNothingNamesTheRenewal_CannotBackfill()
    {
        // Arrange
        var audit = CreateAudit();

        // Act
        var result = StripeRenewalBackfill.Resolve(audit, CreateRecords());

        // Assert
        result.CanBackfill.Should().BeFalse();
        result.MemberId.Should().BeNull();
    }

    [Test]
    public static void Resolve_WhenThePaymentIsAlreadyRecorded_CannotBackfill()
    {
        // Arrange
        var audit = CreateAudit(payment: new Payment { Id = Guid.NewGuid() });
        var records = CreateRecords(memberSubscriptionRecords: [CreateMemberSubscriptionRecord()]);

        // Act
        var result = StripeRenewalBackfill.Resolve(audit, records);

        // Assert
        result.CanBackfill.Should().BeFalse();
    }

    [Test]
    public static void Resolve_WhenTheRenewalTookNoMoney_CannotBackfill()
    {
        // Arrange
        var audit = CreateAudit(status: StripeTransactionStatus.Pending);
        var records = CreateRecords(memberSubscriptionRecords: [CreateMemberSubscriptionRecord()]);

        // Act
        var result = StripeRenewalBackfill.Resolve(audit, records);

        // Assert
        result.CanBackfill.Should().BeFalse();
    }

    [TestCase(StripeTransactionKind.OneOff)]
    [TestCase(StripeTransactionKind.SubscriptionInitial)]
    public static void Resolve_WhenTheTransactionIsNotARenewal_CannotBackfill(StripeTransactionKind kind)
    {
        // Arrange
        var audit = CreateAudit(kind: kind);
        var records = CreateRecords(memberSubscriptionRecords: [CreateMemberSubscriptionRecord()]);

        // Act
        var result = StripeRenewalBackfill.Resolve(audit, records);

        // Assert
        result.CanBackfill.Should().BeFalse();
    }

    [Test]
    public static void Resolve_WhenTheRenewalNamesNoSubscription_CannotBackfill()
    {
        // Arrange
        var audit = CreateAudit(subscriptionId: null, metadata: ChapterSubscriptionMetadata());
        var records = CreateRecords();

        // Act
        var result = StripeRenewalBackfill.Resolve(audit, records);

        // Assert
        result.CanBackfill.Should().BeFalse();
    }

    [Test]
    public static void Resolve_WhenARecordNamesAnotherSubscription_IgnoresIt()
    {
        // Arrange
        var audit = CreateAudit();
        var records = CreateRecords(
            memberSubscriptionRecords: [CreateMemberSubscriptionRecord(externalId: "sub_2")]);

        // Act
        var result = StripeRenewalBackfill.Resolve(audit, records);

        // Assert
        result.CanBackfill.Should().BeFalse();
    }

    private static Dictionary<string, string> ChapterSubscriptionMetadata()
        => new()
        {
            { PaymentMetadataModel.Keys.ChapterId, ChapterId.ToString() },
            { PaymentMetadataModel.Keys.ChapterSubscriptionId, ChapterSubscriptionId.ToString() },
            { PaymentMetadataModel.Keys.MemberId, MemberId.ToString() },
            { PaymentMetadataModel.Keys.Platform, PlatformType.Default.ToString() }
        };

    private static StripeTransactionAudit CreateAudit(
        StripeTransactionKind kind = StripeTransactionKind.SubscriptionRenewal,
        StripeTransactionStatus status = StripeTransactionStatus.Succeeded,
        string? subscriptionId = SubscriptionId,
        IReadOnlyDictionary<string, string>? metadata = null,
        Payment? payment = null)
    {
        metadata ??= new Dictionary<string, string>();

        return new StripeTransactionAudit
        {
            Findings = [],
            Metadata = PaymentMetadataModel.FromDictionary(metadata),
            Payment = payment,
            Transaction = new StripeTransaction
            {
                Amount = 10m,
                ChargeId = "ch_1",
                CreatedUtc = Now,
                CurrencyCode = "gbp",
                InvoiceId = "in_1",
                Kind = kind,
                Metadata = metadata,
                PaidUtc = Now,
                PaymentIntentId = "pi_1",
                Status = status,
                SubscriptionId = subscriptionId
            }
        };
    }

    private static MemberSiteSubscriptionRecord CreateMemberSiteSubscriptionRecord(
        string? externalId = SubscriptionId)
        => new()
        {
            CreatedUtc = Now,
            ExternalId = externalId,
            Id = Guid.NewGuid(),
            IsCurrent = true,
            MemberId = RecordMemberId,
            SiteSubscriptionId = Guid.NewGuid(),
            SiteSubscriptionPriceId = SiteSubscriptionPriceId
        };

    private static MemberSubscriptionRecord CreateMemberSubscriptionRecord(
        string? externalId = SubscriptionId)
        => new()
        {
            ChapterId = RecordChapterId,
            ChapterSubscriptionId = ChapterSubscriptionId,
            ExternalId = externalId,
            Id = Guid.NewGuid(),
            IsCurrent = true,
            MemberId = RecordMemberId,
            PurchasedUtc = Now
        };

    private static StripeTransactionRecords CreateRecords(
        IReadOnlyCollection<MemberSubscriptionRecord>? memberSubscriptionRecords = null,
        IReadOnlyCollection<MemberSiteSubscriptionRecord>? memberSiteSubscriptionRecords = null,
        IReadOnlySet<Guid>? chapterSubscriptionIds = null,
        IReadOnlySet<Guid>? memberIds = null)
        => new()
        {
            ChapterIds = new HashSet<Guid> { ChapterId },
            ChapterSubscriptionIds = chapterSubscriptionIds ?? new HashSet<Guid> { ChapterSubscriptionId },
            MemberIds = memberIds ?? new HashSet<Guid> { MemberId },
            MemberSiteSubscriptionRecords = memberSiteSubscriptionRecords ?? [],
            MemberSubscriptionRecords = memberSubscriptionRecords ?? [],
            PaymentCheckoutSessionIds = new HashSet<Guid>(),
            Payments = [],
            SiteSubscriptionPriceIds = new HashSet<Guid> { SiteSubscriptionPriceId }
        };

    private static Dictionary<string, string> SiteSubscriptionMetadata()
        => new()
        {
            { PaymentMetadataModel.Keys.MemberId, MemberId.ToString() },
            { PaymentMetadataModel.Keys.Platform, PlatformType.Default.ToString() },
            { PaymentMetadataModel.Keys.SiteSubscriptionPriceId, SiteSubscriptionPriceId.ToString() }
        };
}
