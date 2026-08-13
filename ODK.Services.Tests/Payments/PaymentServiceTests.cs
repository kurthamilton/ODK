using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using ODK.Core.Chapters;
using ODK.Core.Countries;
using ODK.Core.Emails;
using ODK.Core.Events;
using ODK.Core.Members;
using ODK.Core.Payments;
using ODK.Core.Platforms;
using ODK.Data.Core;
using ODK.Services.Events;
using ODK.Services.Logging;
using ODK.Services.Members;
using ODK.Services.Payments;
using ODK.Services.Payments.Models;
using ODK.Services.Subscriptions;
using ODK.Services.Tests.Helpers;

namespace ODK.Services.Tests.Payments;

[Parallelizable]
public static class PaymentServiceTests
{
    [Test]
    public static async Task ProcessWebhook_WhenEventAlreadyProcessed_ReturnsWithoutProcessing()
    {
        // Arrange
        using var context = CreateMockOdkContext();

        var webhook = CreatePaymentProviderWebhook();

        context.Create(new PaymentProviderWebhookEvent
        {
            ExternalId = webhook.Id,
            PaymentProviderType = webhook.PaymentProviderType,
            ReceivedUtc = DateTime.UtcNow
        });

        var service = CreatePaymentService(context);
        var request = CreateServiceRequest();

        // Act
        await service.ProcessWebhook(request, webhook);

        // Assert
        context.Set<PaymentProviderWebhookEvent>()
            .Select(x => x.ExternalId)
            .ToArray()
            .Should()
            .BeEquivalentTo([webhook.Id]);
    }

    [Test]
    public static async Task ProcessWebhook_WhenEventIsNew_AddsWebhookEvent()
    {
        // Arrange
        using var context = CreateMockOdkContext();

        var webhook = CreatePaymentProviderWebhook(type: PaymentProviderWebhookType.CheckoutSessionExpired);

        var service = CreatePaymentService(context);
        var request = CreateServiceRequest();

        // Act
        await service.ProcessWebhook(request, webhook);

        // Assert
        context.Set<PaymentProviderWebhookEvent>()
            .Select(x => x.ExternalId)
            .ToArray()
            .Should()
            .BeEquivalentTo([webhook.Id]);
    }

    [TestCase(PaymentProviderWebhookType.CheckoutSessionCompleted)]
    public static async Task ProcessWebhook_PaymentSucceeded_UpdatesPayment(PaymentProviderWebhookType webhookType)
    {
        // Arrange
        using var context = CreateMockOdkContext();

        var member = context.CreateMember();
        var chapter = context.CreateChapter(
            members: [member]);

        var chapterSubscription = context.CreateChapterSubscription(chapter);

        var payment = context.CreatePayment(member: member);
        var paymentCheckoutSession = context.CreatePaymentCheckoutSession(payment: payment);

        var webhook = CreatePaymentProviderWebhook(
            type: webhookType,
            paymentId: paymentCheckoutSession.PaymentId.ToString(),
            metadata: new PaymentMetadataModel(
                PlatformType.Default,
                PaymentReasonType.None,
                member,
                chapterSubscription,
                paymentCheckoutSession.Id,
                payment.Id));

        var service = CreatePaymentService(context);
        var request = CreateServiceRequest();

        // Act
        await service.ProcessWebhook(request, webhook);

        // Assert
        payment = context.Set<Payment>()
            .Single(x => x.Id == paymentCheckoutSession.PaymentId);

        payment.PaidUtc.Should().NotBeNull();
        payment.ExternalId.Should().Be(webhook.PaymentId);
    }

    [TestCase(PaymentProviderWebhookType.CheckoutSessionCompleted)]
    public static async Task ProcessWebhook_EventTicketPaymentSucceeded_UpdatesEventTicketStatus(
        PaymentProviderWebhookType webhookType)
    {
        // Arrange
        using var context = CreateMockOdkContext();

        var member = context.CreateMember();
        var payment = context.CreatePayment(
            member: member);
        var paymentCheckoutSession = context.CreatePaymentCheckoutSession(
            payment: payment);
        var @event = context.CreateEvent();
        var eventTicketPayment = context.Create(CreateEventTicketPayment(@event, payment));

        var webhook = CreatePaymentProviderWebhook(
            type: webhookType,
            paymentId: payment.Id.ToString(),
            metadata: new PaymentMetadataModel(
                PlatformType.Default,
                PaymentReasonType.EventTicket,
                member,
                eventTicketPayment,
                paymentCheckoutSession.Id));

        var eventService = CreateMockEventService();

        var service = CreatePaymentService(
            context,
            eventService: eventService);
        var request = CreateServiceRequest();

        // Act
        await service.ProcessWebhook(request, webhook);

        // Assert
        payment = context.Set<Payment>()
            .Single(x => x.Id == payment.Id);

        Mock.Get(eventService)
            .Verify(x => x.CompleteEventTicketPurchase(@event.Id, payment.MemberId), Times.Once);

        payment.PaidUtc.Should().NotBeNull();
        payment.ExternalId.Should().Be(webhook.PaymentId);
    }

    [TestCase(PaymentProviderWebhookType.CheckoutSessionCompleted)]
    public static async Task ProcessWebhook_EventTicketPaymentSucceeded_UpdatesChapterSubscription(
        PaymentProviderWebhookType webhookType)
    {
        // Arrange
        using var context = CreateMockOdkContext();

        var member = context.CreateMember();

        var chapter = context.CreateChapter(
            members: [member]);

        var payment = context.CreatePayment(
            member: member,
            chapter: chapter);

        var paymentCheckoutSession = context.CreatePaymentCheckoutSession(
            payment: payment);

        var chapterSubscription = context.CreateChapterSubscription(chapter: chapter);

        var webhook = CreatePaymentProviderWebhook(
            type: webhookType,
            paymentId: payment.Id.ToString(),
            metadata: new PaymentMetadataModel(
                PlatformType.Default,
                PaymentReasonType.ChapterSubscription,
                member,
                chapterSubscription,
                paymentCheckoutSession.Id,
                payment.Id));

        var service = CreatePaymentService(context);
        var request = CreateServiceRequest();

        // Act
        await service.ProcessWebhook(request, webhook);

        // Assert
        context.Set<MemberSubscriptionRecord>()
            .Count(x => x.MemberId == member.Id && x.ChapterId == chapter.Id && x.IsCurrent)
            .Should()
            .Be(1);

        context.Set<MemberSubscriptionRecord>()
            .Count(x => x.MemberId == member.Id && x.ChapterSubscriptionId == chapterSubscription.Id)
            .Should()
            .Be(1);
    }

    [Test]
    public static async Task ProcessWebhook_WhenCheckoutSessionExpired_UpdatesSessionExpiry()
    {
        // Arrange
        using var context = CreateMockOdkContext();

        var member = context.CreateMember();
        var currency = context.CreateCurrency();
        var payment = context.CreatePayment(member: member, currency: currency);
        var paymentCheckoutSession = context.CreatePaymentCheckoutSession(payment: payment);
        var siteSubscriptionPrice = context.CreateSiteSubscriptionPrice(
            currency: currency);

        var webhook = CreatePaymentProviderWebhook(
            type: PaymentProviderWebhookType.CheckoutSessionExpired,
            metadata: new PaymentMetadataModel(
                PlatformType.Default,
                PaymentReasonType.SiteSubscription,
                member,
                siteSubscriptionPrice,
                paymentCheckoutSession.Id,
                payment.Id));

        var service = CreatePaymentService(context);
        var request = CreateServiceRequest();

        // Act
        await service.ProcessWebhook(request, webhook);

        // Assert
        paymentCheckoutSession = context.Set<PaymentCheckoutSession>()
            .Single(x => x.Id == paymentCheckoutSession.Id);

        paymentCheckoutSession.ExpiredUtc.Should().NotBeNull();
    }

    [TestCase(PaymentProviderWebhookType.InvoicePaymentSucceeded)]
    public static async Task ProcessWebhook_SubscriptionSucceeded_UpdatesChapterSubscription(PaymentProviderWebhookType webhookType)
    {
        // Arrange
        using var context = CreateMockOdkContext();

        var member = context.CreateMember();

        var chapter = context.CreateChapter(
            members: [member]);

        var chapterSubscription = context.CreateChapterSubscription(chapter: chapter);

        var paymentCheckoutSession = context.CreatePaymentCheckoutSession();

        var webhook = CreatePaymentProviderWebhook(
            type: webhookType,
            subscriptionId: "sub_123",
            metadata: new PaymentMetadataModel(
                PlatformType.Default,
                PaymentReasonType.ChapterSubscription,
                member,
                chapterSubscription,
                paymentCheckoutSession.Id,
                paymentCheckoutSession.PaymentId));

        var service = CreatePaymentService(context);
        var request = CreateServiceRequest();

        // Act
        await service.ProcessWebhook(request, webhook);

        // Assert
        context.Set<MemberSubscriptionRecord>()
            .Count(x => x.MemberId == member.Id && x.ChapterId == chapter.Id && x.IsCurrent)
            .Should()
            .Be(1);

        context.Set<MemberSubscriptionRecord>()
            .Count(x => x.MemberId == member.Id && x.ChapterSubscriptionId == chapterSubscription.Id)
            .Should()
            .Be(1);
    }

    [Test]
    public static async Task ProcessWebhook_ChapterSubscription_PersistsDecimalAmountWithCents()
    {
        // Arrange
        // Amount is a decimal end-to-end (previously a double, which risked binary-float artefacts when
        // cast to the decimal Payment/record amount). A cents value must round-trip exactly.
        using var context = CreateMockOdkContext();

        var member = context.CreateMember();
        var chapter = context.CreateChapter(members: [member]);
        var chapterSubscription = context.CreateChapterSubscription(chapter: chapter);
        chapterSubscription.Amount = 12.34m;

        var paymentCheckoutSession = context.CreatePaymentCheckoutSession();

        var webhook = CreatePaymentProviderWebhook(
            type: PaymentProviderWebhookType.InvoicePaymentSucceeded,
            subscriptionId: "sub_123",
            metadata: new PaymentMetadataModel(
                PlatformType.Default,
                PaymentReasonType.ChapterSubscription,
                member,
                chapterSubscription,
                paymentCheckoutSession.Id,
                paymentCheckoutSession.PaymentId));

        var service = CreatePaymentService(context);
        var request = CreateServiceRequest();

        // Act
        await service.ProcessWebhook(request, webhook);

        // Assert
        var record = context.Set<MemberSubscriptionRecord>()
            .Single(x => x.MemberId == member.Id && x.ChapterSubscriptionId == chapterSubscription.Id);

        record.Amount.Should().Be(12.34m);
    }

    [Test]
    public static async Task ProcessWebhook_CheckoutSessionCompleted_ForSubscriptionWithoutPaymentIntent_DoesNotUpdateChapterSubscription()
    {
        // Arrange
        // A subscription-mode Checkout Session carries no payment_intent, so checkout.session.completed
        // arrives with an empty PaymentId and must be a no-op for subscriptions - the subscription is
        // extended solely by invoice.payment_succeeded. This guards against reintroducing a double extension.
        using var context = CreateMockOdkContext();

        var member = context.CreateMember();
        var chapter = context.CreateChapter(members: [member]);
        var chapterSubscription = context.CreateChapterSubscription(chapter: chapter);

        var webhook = CreatePaymentProviderWebhook(
            type: PaymentProviderWebhookType.CheckoutSessionCompleted,
            paymentId: "",
            metadata: new PaymentMetadataModel(
                PlatformType.Default,
                PaymentReasonType.ChapterSubscription,
                member,
                chapterSubscription,
                Guid.NewGuid(),
                Guid.NewGuid()));

        var service = CreatePaymentService(context);
        var request = CreateServiceRequest();

        // Act
        await service.ProcessWebhook(request, webhook);

        // Assert
        context.Set<MemberSubscriptionRecord>().Should().BeEmpty();
    }

    [Test]
    public static async Task ProcessWebhook_NewChapterSubscription_ProcessedByCheckoutAndInvoice_ExtendsExpiryOnce()
    {
        // Arrange
        // Stripe fires both checkout.session.completed (no payment_intent) and invoice.payment_succeeded
        // for a new subscription. Only the invoice event should extend the expiry - exactly once.
        using var context = CreateMockOdkContext();

        var member = context.CreateMember();
        var chapter = context.CreateChapter(members: [member]);
        var chapterSubscription = context.CreateChapterSubscription(chapter: chapter);
        chapterSubscription.Months = 1;

        var payment = context.CreatePayment(member: member, chapter: chapter);
        var paymentCheckoutSession = context.CreatePaymentCheckoutSession(payment: payment);

        var metadata = new PaymentMetadataModel(
            PlatformType.Default,
            PaymentReasonType.ChapterSubscription,
            member,
            chapterSubscription,
            paymentCheckoutSession.Id,
            payment.Id);

        var checkoutWebhook = CreatePaymentProviderWebhook(
            id: "wh_checkout",
            type: PaymentProviderWebhookType.CheckoutSessionCompleted,
            paymentId: "",
            metadata: metadata);

        var invoiceWebhook = CreatePaymentProviderWebhook(
            id: "wh_invoice",
            type: PaymentProviderWebhookType.InvoicePaymentSucceeded,
            subscriptionId: "sub_123",
            metadata: metadata);

        var service = CreatePaymentService(context);
        var request = CreateServiceRequest();

        // Act
        await service.ProcessWebhook(request, checkoutWebhook);
        await service.ProcessWebhook(request, invoiceWebhook);

        // Assert
        context.Set<MemberSubscriptionRecord>()
            .Count(x => x.MemberId == member.Id && x.ChapterSubscriptionId == chapterSubscription.Id)
            .Should()
            .Be(1);

        var memberSubscription = context.Set<MemberSubscriptionRecord>()
            .Single(x => x.MemberId == member.Id && x.ChapterId == chapter.Id && x.IsCurrent);

        // A single extension of 1 month - not two.
        memberSubscription.ExpiresUtc
            .Should()
            .BeCloseTo(DateTime.UtcNow.AddMonths(1), TimeSpan.FromMinutes(5));
    }

    [Test]
    public static async Task ProcessWebhook_ChapterSubscriptionRenewal_ExtendsExpiryByPlanMonths()
    {
        // Arrange
        // A renewal is a subsequent invoice.payment_succeeded (a distinct event) for an existing
        // subscription. It should extend the existing expiry by the plan's months, exactly once.
        using var context = CreateMockOdkContext();

        var member = context.CreateMember();
        var chapter = context.CreateChapter(members: [member]);
        var chapterSubscription = context.CreateChapterSubscription(chapter: chapter);
        chapterSubscription.Months = 1;

        var payment = context.CreatePayment(
            member: member,
            chapter: chapter,
            paidUtc: DateTime.UtcNow.AddMonths(-1));
        var paymentCheckoutSession = context.CreatePaymentCheckoutSession(
            payment: payment,
            completedUtc: DateTime.UtcNow.AddMonths(-1));

        var originalExpiry = DateTime.UtcNow.AddDays(10);

        // The record created by the initial subscription, keyed on the Stripe subscription id - currently the
        // member's current record.
        var initialRecord = new MemberSubscriptionRecord
        {
            Amount = chapterSubscription.Amount,
            ChapterId = chapter.Id,
            ChapterSubscriptionId = chapterSubscription.Id,
            ExpiresUtc = originalExpiry,
            ExternalId = "sub_123",
            Id = Guid.NewGuid(),
            IsCurrent = true,
            MemberId = member.Id,
            Months = chapterSubscription.Months,
            PaymentId = payment.Id,
            PurchasedUtc = DateTime.UtcNow.AddMonths(-1),
            Type = chapterSubscription.Type
        };
        context.Create(initialRecord);

        var webhook = CreatePaymentProviderWebhook(
            id: "wh_renewal",
            type: PaymentProviderWebhookType.InvoicePaymentSucceeded,
            subscriptionId: "sub_123",
            metadata: new PaymentMetadataModel(
                PlatformType.Default,
                PaymentReasonType.ChapterSubscription,
                member,
                chapterSubscription,
                paymentCheckoutSession.Id,
                payment.Id));

        var service = CreatePaymentService(context);
        var request = CreateServiceRequest();

        // Act
        await service.ProcessWebhook(request, webhook);

        // Assert
        var memberSubscription = context.Set<MemberSubscriptionRecord>()
            .Single(x => x.MemberId == member.Id && x.ChapterId == chapter.Id && x.IsCurrent);

        memberSubscription.ExpiresUtc
            .Should()
            .BeCloseTo(originalExpiry.AddMonths(1), TimeSpan.FromMinutes(5));

        // The renewal appends a new record (keeping the subscription's payment history) rather than mutating
        // the existing one, so there are now two records for the subscription.
        var records = context.Set<MemberSubscriptionRecord>()
            .Where(x => x.MemberId == member.Id && x.ChapterSubscriptionId == chapterSubscription.Id)
            .ToArray();
        records.Length.Should().Be(2);

        // Exactly one is current: the new record, carrying the rolled-forward expiry. The original is not.
        var currentRecord = records.Should().ContainSingle(x => x.IsCurrent).Subject;
        currentRecord.Id.Should().NotBe(initialRecord.Id);
        currentRecord.ExpiresUtc
            .Should()
            .BeCloseTo(originalExpiry.AddMonths(1), TimeSpan.FromMinutes(5));
    }

    [Test]
    // A live period continues, so a membership keeps its anniversary rather than drifting by however late
    // the member renewed.
    [TestCase(10, 30, true)]
    // Lapsed but inside the cooldown - still effectively a member, so the period continues.
    [TestCase(-5, 30, true)]
    // Lapsed beyond the cooldown: a returning member starts a new period.
    [TestCase(-40, 30, false)]
    // No cooldown configured, so only a live period continues.
    [TestCase(-5, 0, false)]
    // The cooldown outlasts the subscription itself, so continuing would expire in the past. A payment has
    // to leave the member current, so the period starts now.
    [TestCase(-40, 60, false)]
    public static async Task ProcessWebhook_OneOffChapterSubscription_ContinuesPeriodOnlyWithinCooldown(
        int expiryDaysFromNow,
        int cooldownDays,
        bool continuesExistingPeriod)
    {
        // Arrange
        using var context = CreateMockOdkContext();

        var member = context.CreateMember();
        var chapter = context.CreateChapter(members: [member]);
        var chapterSubscription = context.CreateChapterSubscription(chapter: chapter);
        chapterSubscription.Months = 1;
        chapterSubscription.Recurring = false;

        context.Create(new ChapterMembershipSettings
        {
            ChapterId = chapter.Id,
            Enabled = true,
            MembershipDisabledAfterDaysExpired = cooldownDays
        });

        var payment = context.CreatePayment(
            member: member,
            chapter: chapter,
            paidUtc: DateTime.UtcNow.AddMonths(-1));
        var paymentCheckoutSession = context.CreatePaymentCheckoutSession(
            payment: payment,
            completedUtc: DateTime.UtcNow.AddMonths(-1));

        var originalExpiry = DateTime.UtcNow.AddDays(expiryDaysFromNow);

        context.Create(new MemberSubscriptionRecord
        {
            Amount = chapterSubscription.Amount,
            ChapterId = chapter.Id,
            ChapterSubscriptionId = chapterSubscription.Id,
            ExpiresUtc = originalExpiry,
            Id = Guid.NewGuid(),
            IsCurrent = true,
            MemberId = member.Id,
            Months = chapterSubscription.Months,
            PaymentId = payment.Id,
            PurchasedUtc = DateTime.UtcNow.AddMonths(-1),
            Type = chapterSubscription.Type
        });

        var webhook = CreatePaymentProviderWebhook(
            id: "wh_oneoff",
            type: PaymentProviderWebhookType.InvoicePaymentSucceeded,
            subscriptionId: "sub_123",
            metadata: new PaymentMetadataModel(
                PlatformType.Default,
                PaymentReasonType.ChapterSubscription,
                member,
                chapterSubscription,
                paymentCheckoutSession.Id,
                payment.Id));

        var service = CreatePaymentService(context);
        var request = CreateServiceRequest();

        // Act
        await service.ProcessWebhook(request, webhook);

        // Assert
        var current = context.Set<MemberSubscriptionRecord>()
            .Single(x => x.MemberId == member.Id && x.ChapterId == chapter.Id && x.IsCurrent);

        var expected = continuesExistingPeriod
            ? originalExpiry.AddMonths(1)
            : DateTime.UtcNow.AddMonths(1);

        current.ExpiresUtc.Should().BeCloseTo(expected, TimeSpan.FromMinutes(5));
    }

    [Test]
    public static async Task ProcessWebhook_ChapterSubscriptionFirstInvoiceThenRenewal_ExtendsExpiryOncePerPeriod()
    {
        // Arrange - mirror the E2E test-clock flow driven end to end: a recurring subscription's first
        // invoice then a renewal invoice (both invoice.payment_succeeded on the same subscription id, with
        // distinct event ids and neither carrying a checkout payment). Each should extend by the plan's
        // months, leaving expiry ~2 months out and exactly one current record.
        using var context = CreateMockOdkContext();

        var member = context.CreateMember();
        var chapter = context.CreateChapter(members: [member]);
        var chapterSubscription = context.CreateChapterSubscription(chapter: chapter);
        chapterSubscription.Months = 1;

        // Joining the chapter already created a current 'Free' record (AddMemberToChapter).
        context.Create(new MemberSubscriptionRecord
        {
            ChapterId = chapter.Id,
            Id = Guid.NewGuid(),
            IsCurrent = true,
            MemberId = member.Id,
            PurchasedUtc = DateTime.UtcNow.AddMinutes(-5),
            Type = SubscriptionType.Free
        });

        var metadata = new PaymentMetadataModel(
            PlatformType.Default,
            PaymentReasonType.ChapterSubscription,
            member,
            chapterSubscription,
            Guid.NewGuid(),
            Guid.NewGuid());

        var firstInvoice = CreatePaymentProviderWebhook(
            id: "wh_inv1",
            type: PaymentProviderWebhookType.InvoicePaymentSucceeded,
            subscriptionId: "sub_123",
            metadata: metadata);
        var renewalInvoice = CreatePaymentProviderWebhook(
            id: "wh_inv2",
            type: PaymentProviderWebhookType.InvoicePaymentSucceeded,
            subscriptionId: "sub_123",
            metadata: metadata);

        var service = CreatePaymentService(context);
        var request = CreateServiceRequest();

        // Act
        await service.ProcessWebhook(request, firstInvoice);
        await service.ProcessWebhook(request, renewalInvoice);

        // Assert
        var memberSubscription = context.Set<MemberSubscriptionRecord>()
            .Single(x => x.MemberId == member.Id && x.ChapterId == chapter.Id && x.IsCurrent);
        memberSubscription.ExpiresUtc
            .Should()
            .BeCloseTo(DateTime.UtcNow.AddMonths(2), TimeSpan.FromDays(2));

        context.Set<MemberSubscriptionRecord>()
            .Count(x => x.MemberId == member.Id && x.IsCurrent)
            .Should()
            .Be(1);
    }

    [Test]
    public static async Task ProcessWebhook_ChapterSubscription_PersistsExternalIdOnlyWhenRecurring(
        [Values(true, false)] bool recurring)
    {
        // Arrange - a chapter-subscription invoice webhook carrying an external id. Only a recurring
        // subscription should keep it (the Stripe subscription id); a one-off must leave it null so no
        // "no such subscription" lookup is later attempted against a payment-intent id.
        using var context = CreateMockOdkContext();

        var member = context.CreateMember();
        var chapter = context.CreateChapter(members: [member]);
        var chapterSubscription = context.CreateChapterSubscription(chapter: chapter);
        chapterSubscription.Months = 1;
        chapterSubscription.Recurring = recurring;

        var metadata = new PaymentMetadataModel(
            PlatformType.Default,
            PaymentReasonType.ChapterSubscription,
            member,
            chapterSubscription,
            Guid.NewGuid(),
            Guid.NewGuid());

        var webhook = CreatePaymentProviderWebhook(
            id: "wh_inv",
            type: PaymentProviderWebhookType.InvoicePaymentSucceeded,
            subscriptionId: "sub_123",
            metadata: metadata);

        var service = CreatePaymentService(context);
        var request = CreateServiceRequest();

        // Act
        await service.ProcessWebhook(request, webhook);

        // Assert
        var current = context.Set<MemberSubscriptionRecord>()
            .Single(x => x.MemberId == member.Id && x.ChapterId == chapter.Id && x.IsCurrent);
        current.ExternalId.Should().Be(recurring ? "sub_123" : null);
    }

    [Test]
    public static async Task ProcessWebhook_ChapterSubscription_UsesNextPaymentDateOnlyWhenRecurring(
        [Values(true, false)] bool recurring)
    {
        // Arrange - a recurring subscription expires when the provider next takes payment, so the two cannot
        // drift apart. A one-off has no payment schedule to read and calculates the expiry from the plan's
        // months; the external id it was given is a payment intent, not a subscription, so it must not be
        // looked up.
        using var context = CreateMockOdkContext();

        var member = context.CreateMember();
        var chapter = context.CreateChapter(members: [member]);
        var chapterSubscription = context.CreateChapterSubscription(chapter: chapter);
        chapterSubscription.Months = 1;
        chapterSubscription.Recurring = recurring;

        // Deliberately not a month out, so only reading the provider's date can satisfy the assertion.
        var nextPaymentDate = DateTime.UtcNow.AddDays(20);

        var webhook = CreatePaymentProviderWebhook(
            id: "wh_inv",
            type: PaymentProviderWebhookType.InvoicePaymentSucceeded,
            subscriptionId: "sub_123",
            metadata: new PaymentMetadataModel(
                PlatformType.Default,
                PaymentReasonType.ChapterSubscription,
                member,
                chapterSubscription,
                Guid.NewGuid(),
                Guid.NewGuid()));

        var service = CreatePaymentService(
            context,
            paymentProviderFactory: CreateMockPaymentProviderFactory(nextPaymentDate));
        var request = CreateServiceRequest();

        // Act
        await service.ProcessWebhook(request, webhook);

        // Assert
        var current = context.Set<MemberSubscriptionRecord>()
            .Single(x => x.MemberId == member.Id && x.ChapterId == chapter.Id && x.IsCurrent);

        var expected = recurring ? nextPaymentDate : DateTime.UtcNow.AddMonths(1);
        current.ExpiresUtc.Should().BeCloseTo(expected, TimeSpan.FromMinutes(5));
    }

    [Test]
    public static async Task ProcessWebhookAction_WhenSameEventProcessedTwice_ExtendsChapterSubscriptionOnce()
    {
        // Arrange
        // Simulates the webhook-processing action being retried (e.g. after a transient failure that occurs
        // once the extension has already committed). The second run must not extend the subscription again -
        // it is guarded by the initiating event id (InitiatorId = webhook.Id).
        using var context = CreateMockOdkContext();

        var member = context.CreateMember();
        var chapter = context.CreateChapter(members: [member]);
        var chapterSubscription = context.CreateChapterSubscription(chapter: chapter);
        chapterSubscription.Months = 1;

        var paymentCheckoutSession = context.CreatePaymentCheckoutSession();

        var webhook = CreatePaymentProviderWebhook(
            id: "wh_invoice",
            type: PaymentProviderWebhookType.InvoicePaymentSucceeded,
            subscriptionId: "sub_123",
            metadata: new PaymentMetadataModel(
                PlatformType.Default,
                PaymentReasonType.ChapterSubscription,
                member,
                chapterSubscription,
                paymentCheckoutSession.Id,
                paymentCheckoutSession.PaymentId));

        var service = CreatePaymentService(context);
        var request = CreateServiceRequest();

        // Act - run the processing action twice, as a Hangfire retry would
        await service.ProcessWebhookAction(request, webhook);
        await service.ProcessWebhookAction(request, webhook);

        // Assert
        context.Set<MemberSubscriptionRecord>()
            .Count(x => x.MemberId == member.Id && x.ChapterSubscriptionId == chapterSubscription.Id)
            .Should()
            .Be(1);

        var memberSubscription = context.Set<MemberSubscriptionRecord>()
            .Single(x => x.MemberId == member.Id && x.ChapterId == chapter.Id && x.IsCurrent);

        // Extended once (one month), not twice.
        memberSubscription.ExpiresUtc
            .Should()
            .BeCloseTo(DateTime.UtcNow.AddMonths(1), TimeSpan.FromMinutes(5));
    }

    [Test]
    public static async Task ProcessWebhookAction_WhenSameEventProcessedTwice_ExtendsSiteSubscriptionOnce()
    {
        // Arrange
        // As with chapter subscriptions, a retry of the site-subscription webhook-processing action must not
        // extend the subscription twice. It is guarded by the initiating event id (InitiatorId = webhook.Id).
        using var context = CreateMockOdkContext();

        var member = context.CreateMember();
        var currency = context.CreateCurrency();
        var siteSubscription = context.CreateSiteSubscription();
        var siteSubscriptionPrice = context.CreateSiteSubscriptionPrice(
            siteSubscription: siteSubscription,
            currency: currency);
        var payment = context.CreatePayment(member: member, currency: currency);

        var webhook = CreatePaymentProviderWebhook(
            id: "wh_invoice",
            type: PaymentProviderWebhookType.InvoicePaymentSucceeded,
            subscriptionId: "sub_123",
            metadata: new PaymentMetadataModel(
                PlatformType.Default,
                PaymentReasonType.SiteSubscription,
                member,
                siteSubscriptionPrice,
                Guid.NewGuid(),
                payment.Id));

        var service = CreatePaymentService(context);
        var request = CreateServiceRequest();

        // Act - run the processing action twice, as a Hangfire retry would
        await service.ProcessWebhookAction(request, webhook);
        await service.ProcessWebhookAction(request, webhook);

        // Assert
        context.Set<MemberSiteSubscriptionRecord>()
            .Count()
            .Should()
            .Be(1);

        var currentRecord = context.Set<MemberSiteSubscriptionRecord>()
            .Single(x => x.MemberId == member.Id && x.IsCurrent);

        // Set once, not twice: the provider gives no next payment date here, so the expiry falls back to the
        // yearly plan's 12 months.
        currentRecord.ExpiresUtc
            .Should()
            .BeCloseTo(DateTime.UtcNow.AddMonths(12), TimeSpan.FromMinutes(5));
    }

    [Test]
    public static async Task ProcessWebhook_SiteSubscriptionRenewal_ExtendsExpiryByPlanMonths()
    {
        // Arrange
        // A renewal is a subsequent invoice.payment_succeeded (a distinct event) for an existing site
        // subscription. Recurring invoices reuse the original checkout Payment, so keying idempotency on the
        // payment id would wrongly skip the renewal. Keyed on the event id, the renewal extends exactly once.
        using var context = CreateMockOdkContext();

        var member = context.CreateMember();
        var currency = context.CreateCurrency();
        var siteSubscription = context.CreateSiteSubscription();
        var siteSubscriptionPrice = context.CreateSiteSubscriptionPrice(
            siteSubscription: siteSubscription,
            currency: currency);
        var payment = context.CreatePayment(
            member: member,
            currency: currency,
            paidUtc: DateTime.UtcNow.AddMonths(-12));

        // The member's current subscription from the first cycle - a current log record (the read source)
        // expiring at originalExpiry, plus the dual-written snapshot - seeded by the helper.
        var originalExpiry = DateTime.UtcNow.AddDays(10);
        context.CreateMemberSiteSubscription(
            member,
            siteSubscription: siteSubscription,
            expiresUtc: originalExpiry);

        var webhook = CreatePaymentProviderWebhook(
            id: "wh_renewal",
            type: PaymentProviderWebhookType.InvoicePaymentSucceeded,
            subscriptionId: "sub_123",
            metadata: new PaymentMetadataModel(
                PlatformType.Default,
                PaymentReasonType.SiteSubscription,
                member,
                siteSubscriptionPrice,
                Guid.NewGuid(),
                payment.Id));

        var service = CreatePaymentService(context);
        var request = CreateServiceRequest();

        // Act
        await service.ProcessWebhook(request, webhook);

        // Assert
        var currentRecord = context.Set<MemberSiteSubscriptionRecord>()
            .Single(x => x.MemberId == member.Id && x.IsCurrent);

        // The provider gives no next payment date here, so the expiry falls back to the plan's 12 months
        // from the existing expiry - the point being that the renewal was not skipped.
        currentRecord.ExpiresUtc
            .Should()
            .BeCloseTo(originalExpiry.AddMonths(12), TimeSpan.FromMinutes(5));

        // A new record is added for the renewal event alongside the original.
        context.Set<MemberSiteSubscriptionRecord>()
            .Count()
            .Should()
            .Be(2);
    }

    [Test]
    public static async Task ProcessWebhook_RecurringChapterSubscription_WhenProviderHasNoNextPaymentDate_CalculatesExpiry()
    {
        // Arrange - a provider lookup that comes back empty must not block the payment being recorded, so the
        // expiry degrades to the calculated date rather than being left unset.
        using var context = CreateMockOdkContext();

        var member = context.CreateMember();
        var chapter = context.CreateChapter(members: [member]);
        var chapterSubscription = context.CreateChapterSubscription(chapter: chapter);
        chapterSubscription.Months = 1;
        chapterSubscription.Recurring = true;

        var webhook = CreatePaymentProviderWebhook(
            id: "wh_inv",
            type: PaymentProviderWebhookType.InvoicePaymentSucceeded,
            subscriptionId: "sub_123",
            metadata: new PaymentMetadataModel(
                PlatformType.Default,
                PaymentReasonType.ChapterSubscription,
                member,
                chapterSubscription,
                Guid.NewGuid(),
                Guid.NewGuid()));

        // The default factory returns no subscription, so there is no next payment date to read.
        var service = CreatePaymentService(context);
        var request = CreateServiceRequest();

        // Act
        await service.ProcessWebhook(request, webhook);

        // Assert
        var current = context.Set<MemberSubscriptionRecord>()
            .Single(x => x.MemberId == member.Id && x.ChapterId == chapter.Id && x.IsCurrent);

        current.ExpiresUtc.Should().BeCloseTo(DateTime.UtcNow.AddMonths(1), TimeSpan.FromMinutes(5));
    }

    [Test]
    public static async Task ProcessWebhook_SiteSubscription_SetsExpiryToNextPaymentDate()
    {
        // Arrange - a site subscription is always a provider subscription, so its expiry is the date payment
        // is next taken rather than a date calculated from the plan's frequency.
        using var context = CreateMockOdkContext();

        var member = context.CreateMember();
        var currency = context.CreateCurrency();
        var siteSubscription = context.CreateSiteSubscription();
        var siteSubscriptionPrice = context.CreateSiteSubscriptionPrice(
            siteSubscription: siteSubscription,
            currency: currency);
        var payment = context.CreatePayment(member: member, currency: currency);

        // Deliberately not the year out the yearly plan would calculate.
        var nextPaymentDate = DateTime.UtcNow.AddMonths(11);

        var webhook = CreatePaymentProviderWebhook(
            id: "wh_invoice",
            type: PaymentProviderWebhookType.InvoicePaymentSucceeded,
            subscriptionId: "sub_123",
            metadata: new PaymentMetadataModel(
                PlatformType.Default,
                PaymentReasonType.SiteSubscription,
                member,
                siteSubscriptionPrice,
                Guid.NewGuid(),
                payment.Id));

        var service = CreatePaymentService(
            context,
            paymentProviderFactory: CreateMockPaymentProviderFactory(nextPaymentDate));
        var request = CreateServiceRequest();

        // Act
        await service.ProcessWebhook(request, webhook);

        // Assert
        var currentRecord = context.Set<MemberSiteSubscriptionRecord>()
            .Single(x => x.MemberId == member.Id && x.IsCurrent);

        currentRecord.ExpiresUtc.Should().BeCloseTo(nextPaymentDate, TimeSpan.FromMinutes(5));
    }

    [Test]
    public static async Task ProcessWebhook_WhenInvalidWebhookType_DoesNotSendEmail()
    {
        // Arrange
        var webhook = CreatePaymentProviderWebhook(type: PaymentProviderWebhookType.None);

        var memberEmailService = CreateMockMemberEmailService();

        var service = CreatePaymentService(memberEmailService: memberEmailService);
        var request = CreateServiceRequest();

        // Act
        await service.ProcessWebhook(request, webhook);

        // Assert
        Mock.Get(memberEmailService)
            .Verify(x => x.SendPaymentNotification(It.IsAny<IServiceRequest>(), It.IsAny<Member>(), It.IsAny<Chapter>(), It.IsAny<Payment>(), It.IsAny<Currency>(), It.IsAny<IEnumerable<Member>>()), Times.Never);
    }

    [Test]
    public static async Task ProcessWebhook_WhenWebhookIncomplete_DoesNotProcess()
    {
        // Arrange
        var webhook = CreatePaymentProviderWebhook(
            type: PaymentProviderWebhookType.CheckoutSessionCompleted,
            complete: false,
            paymentId: "pay_123");

        var loggingService = CreateMockLoggingService();

        var service = CreatePaymentService(loggingService: loggingService);
        var request = CreateServiceRequest();

        // Act
        await service.ProcessWebhook(request, webhook);

        // Assert
        Mock.Get(loggingService)
            .Verify(x => x.Warn(It.IsAny<string>()), Times.Once);
    }

    [Test]
    public static async Task ProcessWebhook_WhenPaymentAlreadyPaid_DoesNotUpdatePayment()
    {
        // Arrange
        using var context = CreateMockOdkContext();

        var member = context.CreateMember();

        var currency = context.CreateCurrency();

        var payment = context.CreatePayment(
            member: member,
            currency: currency,
            paidUtc: DateTime.UtcNow.AddDays(-1));

        var paymentCheckoutSession = context.CreatePaymentCheckoutSession(
            payment: payment);

        var siteSubscriptionPrice = context.CreateSiteSubscriptionPrice(
            currency: currency);

        var webhook = CreatePaymentProviderWebhook(
            type: PaymentProviderWebhookType.CheckoutSessionCompleted,
            paymentId: paymentCheckoutSession.PaymentId.ToString(),
            metadata: new PaymentMetadataModel(
                PlatformType.Default,
                PaymentReasonType.SiteSubscription,
                member,
                siteSubscriptionPrice,
                paymentCheckoutSession.Id,
                payment.Id));

        var loggingService = CreateMockLoggingService();
        var service = CreatePaymentService(context, loggingService: loggingService);
        var request = CreateServiceRequest();

        // Act
        await service.ProcessWebhook(request, webhook);

        // Assert
        Mock.Get(loggingService)
            .Verify(x => x.Warn(It.Is<string>(s => s.Contains("already paid"))), Times.Once);
    }

    [Test]
    public static async Task ProcessWebhook_WhenCheckoutSessionAlreadyCompleted_DoesNotUpdateSession()
    {
        // Arrange
        using var context = CreateMockOdkContext();

        var member = context.CreateMember();

        var currency = context.CreateCurrency();

        var payment = context.CreatePayment(
            member: member,
            currency: currency);
        var paymentCheckoutSession = context.CreatePaymentCheckoutSession(
            payment: payment,
            completedUtc: DateTime.UtcNow.AddDays(-1));

        var siteSubscriptionPrice = context.CreateSiteSubscriptionPrice(
            currency: currency);

        var webhook = CreatePaymentProviderWebhook(
            type: PaymentProviderWebhookType.CheckoutSessionCompleted,
            paymentId: payment.Id.ToString(),
            metadata: new PaymentMetadataModel(
                PlatformType.Default,
                PaymentReasonType.SiteSubscription,
                member,
                siteSubscriptionPrice,
                paymentCheckoutSession.Id,
                payment.Id));

        var loggingService = CreateMockLoggingService();
        var service = CreatePaymentService(context, loggingService: loggingService);
        var request = CreateServiceRequest();

        // Act
        await service.ProcessWebhook(request, webhook);

        // Assert
        Mock.Get(loggingService)
            .Verify(x => x.Warn(It.Is<string>(s => s.Contains("already completed"))), Times.Once);
    }

    [Test]
    public static async Task ProcessWebhook_WhenMissingRequiredMetadata_DoesNotProcess()
    {
        // Arrange
        var webhook = CreatePaymentProviderWebhook(
            type: PaymentProviderWebhookType.CheckoutSessionCompleted,
            paymentId: "pay_123",
            metadata: null);

        var loggingService = CreateMockLoggingService();

        var service = CreatePaymentService(loggingService: loggingService);
        var request = CreateServiceRequest();

        // Act
        await service.ProcessWebhook(request, webhook);

        // Assert
        Mock.Get(loggingService)
            .Verify(x => x.Warn(It.IsAny<string>()), Times.Once);
    }

    [Test]
    public static async Task ProcessWebhook_WhenCheckoutSessionExpired_WithoutPaymentCheckoutSessionId_ReturnsFailure()
    {
        // Arrange
        var webhook = CreatePaymentProviderWebhook(
            type: PaymentProviderWebhookType.CheckoutSessionExpired,
            metadata: null);

        var loggingService = CreateMockLoggingService();

        var service = CreatePaymentService(loggingService: loggingService);
        var request = CreateServiceRequest();

        // Act
        await service.ProcessWebhook(request, webhook);

        // Assert
        Mock.Get(loggingService)
            .Verify(x => x.Warn(It.IsAny<string>()), Times.Once);
    }

    [Test]
    public static async Task ProcessWebhook_WhenCheckoutSessionExpiredAlready_DoesNotUpdate()
    {
        // Arrange
        using var context = CreateMockOdkContext();

        var member = context.CreateMember();

        var currency = context.CreateCurrency();

        var payment = context.CreatePayment(
            member: member,
            currency: currency);
        var paymentCheckoutSession = context.CreatePaymentCheckoutSession(
            payment: payment,
            expiredUtc: DateTime.UtcNow.AddDays(-1));

        var siteSubscriptionPrice = context.CreateSiteSubscriptionPrice(
            currency: currency);

        var webhook = CreatePaymentProviderWebhook(
            type: PaymentProviderWebhookType.CheckoutSessionExpired,
            metadata: new PaymentMetadataModel(
                PlatformType.Default,
                PaymentReasonType.SiteSubscription,
                member,
                siteSubscriptionPrice,
                paymentCheckoutSession.Id,
                payment.Id));

        var loggingService = CreateMockLoggingService();

        var service = CreatePaymentService(context, loggingService: loggingService);
        var request = CreateServiceRequest();

        // Act
        await service.ProcessWebhook(request, webhook);

        // Assert
        Mock.Get(loggingService)
            .Verify(x => x.Warn(It.Is<string>(s => s.Contains("already expired"))), Times.Once);
    }

    private static IEventService CreateMockEventService()
    {
        var mock = new Mock<IEventService>();

        return mock.Object;
    }

    private static ILoggingService CreateMockLoggingService()
    {
        var mock = new Mock<ILoggingService>();
        mock.Setup(x => x.Warn(It.IsAny<string>())).Returns(Task.CompletedTask);
        mock.Setup(x => x.Error(It.IsAny<string>())).Returns(Task.CompletedTask);
        mock.Setup(x => x.Error(It.IsAny<string>(), It.IsAny<Exception>())).Returns(Task.CompletedTask);
        mock.Setup(x => x.Info(It.IsAny<string>())).Returns(Task.CompletedTask);
        return mock.Object;
    }

    private static IMemberEmailService CreateMockMemberEmailService()
    {
        var mock = new Mock<IMemberEmailService>();
        mock.Setup(x => x.SendPaymentNotification(
                It.IsAny<IServiceRequest>(),
                It.IsAny<Member>(),
                It.IsAny<Chapter>(),
                It.IsAny<Payment>(),
                It.IsAny<Currency>(),
                It.IsAny<IEnumerable<Member>>()))
            .Returns(Task.CompletedTask);
        return mock.Object;
    }

    private static MockOdkContext CreateMockOdkContext()
    {
        var context = new MockOdkContext();

        context.Add(new SiteEmailSettings { Platform = PlatformType.Default });

        return context;
    }

    // Returning no subscription by default leaves a recurring expiry falling back to the calculated date,
    // which is what most tests here assert. Pass a date to exercise the provider lookup.
    private static IPaymentProviderFactory CreateMockPaymentProviderFactory(DateTime? nextBillingDate = null)
    {
        var paymentProvider = new Mock<IPaymentProvider>();

        paymentProvider
            .Setup(x => x.GetSubscription(It.IsAny<string>()))
            .ReturnsAsync(nextBillingDate != null
                ? new ExternalSubscription
                {
                    CancelDate = null,
                    ConnectedAccountId = null,
                    ExternalId = "sub_123",
                    ExternalSubscriptionPlanId = "price_123",
                    LastPaymentDate = DateTime.UtcNow,
                    Metadata = new Dictionary<string, string>(),
                    NextBillingDate = nextBillingDate,
                    Status = ExternalSubscriptionStatus.Active
                }
                : null);

        var factory = new Mock<IPaymentProviderFactory>();

        factory
            .Setup(x => x.GetPaymentProvider(It.IsAny<SitePaymentSettings>(), It.IsAny<ChapterPaymentAccount?>()))
            .Returns(paymentProvider.Object);

        factory
            .Setup(x => x.GetSitePaymentProvider(
                It.IsAny<IReadOnlyCollection<SitePaymentSettings>>(), It.IsAny<Guid?>()))
            .Returns(paymentProvider.Object);

        return factory.Object;
    }

    private static IUnitOfWork CreateMockUnitOfWork(MockOdkContext? context = null) => MockUnitOfWork.Create(context);

    private static PaymentService CreatePaymentService(
        MockOdkContext? context = null,
        ILoggingService? loggingService = null,
        IMemberEmailService? memberEmailService = null,
        IPaymentProviderFactory? paymentProviderFactory = null,
        IEventService? eventService = null)
    {
        var unitOfWork = CreateMockUnitOfWork(context);
        return new PaymentService(
            unitOfWork,
            loggingService ?? CreateMockLoggingService(),
            memberEmailService ?? CreateMockMemberEmailService(),
            paymentProviderFactory ?? CreateMockPaymentProviderFactory(),
            eventService ?? CreateMockEventService(),
            new MockBackgroundTaskService(),
            new MemberChapterSubscriptionWriter(unitOfWork),
            new MemberSiteSubscriptionWriter(unitOfWork));
    }

    private static IServiceRequest CreateServiceRequest(PlatformType? platform = null)
    {
        var mock = new Mock<IServiceRequest>();

        mock.Setup(x => x.Platform)
            .Returns(platform ?? PlatformType.Default);

        return mock.Object;
    }

    private static PaymentProviderWebhook CreatePaymentProviderWebhook(
        string? id = null,
        PaymentProviderWebhookType? type = null,
        bool complete = true,
        string? paymentId = null,
        string? subscriptionId = null,
        PaymentMetadataModel? metadata = null,
        IReadOnlyDictionary<string, string>? metadataDictionary = null,
        decimal? amount = null)
        => new PaymentProviderWebhook
        {
            Id = id ?? "wh_123",
            Type = type ?? PaymentProviderWebhookType.CheckoutSessionCompleted,
            Complete = complete,
            PaymentId = paymentId ?? "pi_123",
            SubscriptionId = subscriptionId,
            Metadata = metadata?.ToDictionary() ?? metadataDictionary ?? new Dictionary<string, string>(),
            Amount = amount ?? 100m,
            OriginatedUtc = DateTime.UtcNow,
            PaymentProviderType = PaymentProviderType.Stripe
        };

    private static EventTicketPayment CreateEventTicketPayment(
        Event @event,
        Payment payment)
        => new EventTicketPayment
        {
            Id = Guid.NewGuid(),
            EventId = @event.Id,
            Payment = payment,
            PaymentId = payment.Id
        };
}