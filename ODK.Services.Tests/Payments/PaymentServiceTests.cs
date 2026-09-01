using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using ODK.Core.Chapters;
using ODK.Core.Countries;
using ODK.Core.Events;
using ODK.Core.Members;
using ODK.Core.Payments;
using ODK.Core.Platforms;
using ODK.Core.Subscriptions;
using ODK.Data.Core;
using ODK.Services.Events;
using ODK.Services.Exceptions;
using ODK.Services.Logging;
using ODK.Services.Members;
using ODK.Services.Payments;
using ODK.Services.Payments.Models;
using ODK.Services.Subscriptions;
using ODK.Services.Tasks;
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

    [Test]
    public static async Task ProcessWebhook_WhenCheckoutSessionAlreadyExpired_BroadcastsNothing()
    {
        /* Arrange - a redelivered expiry webhook. Nothing moved, so nobody is told: a broadcast is what
           makes a watching page re-read, and a re-read that can only find what it already has is the cost
           the push exists to remove. */
        using var context = CreateMockOdkContext();

        var member = context.CreateMember();
        var currency = context.CreateCurrency();
        var payment = context.CreatePayment(member: member, currency: currency);
        var paymentCheckoutSession = context.CreatePaymentCheckoutSession(
            payment: payment,
            expiredUtc: DateTime.UtcNow.AddMinutes(-5),
            sessionId: "cs_already_expired");
        var siteSubscriptionPrice = context.CreateSiteSubscriptionPrice(currency: currency);

        var webhook = CreatePaymentProviderWebhook(
            id: "wh_expired_again",
            type: PaymentProviderWebhookType.CheckoutSessionExpired,
            metadata: new PaymentMetadataModel(
                PlatformType.Default,
                PaymentReasonType.SiteSubscription,
                member,
                siteSubscriptionPrice,
                paymentCheckoutSession.Id,
                payment.Id));

        var paymentUpdateBroadcaster = CreateMockPaymentUpdateBroadcaster();
        var service = CreatePaymentService(context, paymentUpdateBroadcaster: paymentUpdateBroadcaster);
        var request = CreateServiceRequest();

        // Act
        await service.ProcessWebhook(request, webhook);

        // Assert
        Mock.Get(paymentUpdateBroadcaster).Verify(
            x => x.CheckoutSessionUpdated(It.IsAny<string>()),
            Times.Never);
    }

    [Test]
    public static async Task ProcessWebhook_WhenCheckoutSessionCompleted_BroadcastsSessionUpdate()
    {
        // Arrange
        using var context = CreateMockOdkContext();

        var member = context.CreateMember();
        var chapter = context.CreateChapter(members: [member]);
        var chapterSubscription = context.CreateChapterSubscription(chapter: chapter);
        var paymentCheckoutSession = context.CreatePaymentCheckoutSession(sessionId: "cs_completed");

        var webhook = CreatePaymentProviderWebhook(
            id: "wh_completed_broadcast",
            type: PaymentProviderWebhookType.CheckoutSessionCompleted,
            subscriptionId: "sub_123",
            metadata: new PaymentMetadataModel(
                PlatformType.Default,
                PaymentReasonType.ChapterSubscription,
                member,
                chapterSubscription,
                paymentCheckoutSession.Id,
                paymentCheckoutSession.PaymentId));

        var paymentUpdateBroadcaster = CreateMockPaymentUpdateBroadcaster();
        var service = CreatePaymentService(context, paymentUpdateBroadcaster: paymentUpdateBroadcaster);
        var request = CreateServiceRequest();

        // Act
        await service.ProcessWebhook(request, webhook);

        // Assert - the provider's own session id, which is what a watching page names
        Mock.Get(paymentUpdateBroadcaster).Verify(
            x => x.CheckoutSessionUpdated("cs_completed"),
            Times.Once);
    }

    [Test]
    public static async Task ProcessWebhook_WhenCheckoutSessionExpired_BroadcastsSessionUpdate()
    {
        // Arrange
        using var context = CreateMockOdkContext();

        var member = context.CreateMember();
        var currency = context.CreateCurrency();
        var payment = context.CreatePayment(member: member, currency: currency);
        var paymentCheckoutSession = context.CreatePaymentCheckoutSession(
            payment: payment, sessionId: "cs_expired");
        var siteSubscriptionPrice = context.CreateSiteSubscriptionPrice(currency: currency);

        var webhook = CreatePaymentProviderWebhook(
            id: "wh_expired_broadcast",
            type: PaymentProviderWebhookType.CheckoutSessionExpired,
            metadata: new PaymentMetadataModel(
                PlatformType.Default,
                PaymentReasonType.SiteSubscription,
                member,
                siteSubscriptionPrice,
                paymentCheckoutSession.Id,
                payment.Id));

        var paymentUpdateBroadcaster = CreateMockPaymentUpdateBroadcaster();
        var service = CreatePaymentService(context, paymentUpdateBroadcaster: paymentUpdateBroadcaster);
        var request = CreateServiceRequest();

        // Act
        await service.ProcessWebhook(request, webhook);

        // Assert
        Mock.Get(paymentUpdateBroadcaster).Verify(
            x => x.CheckoutSessionUpdated("cs_expired"),
            Times.Once);
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
        context.CreateChapterPaymentAccount(chapter);
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
        context.CreateChapterPaymentAccount(chapter);
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
    public static async Task ProcessWebhook_RecurringChapterSubscription_ReadsTheMembersSubscriptionNotThePrice()
    {
        /* Arrange - a chapter subscription's own ExternalId names the provider's price, while the schedule
           belongs to the member's subscription, which the webhook names. Asking for a subscription by a
           price id answers nothing, and the expiry silently falls back to being calculated. */
        using var context = CreateMockOdkContext();

        var member = context.CreateMember();
        var chapter = context.CreateChapter(members: [member]);
        context.CreateChapterPaymentAccount(chapter);

        var chapterSubscription = context.CreateChapterSubscription(chapter: chapter);
        chapterSubscription.ExternalId = "price_123";
        chapterSubscription.Months = 1;
        chapterSubscription.Recurring = true;

        // Deliberately not a month out, so only reading the provider's date can satisfy the assertion.
        var nextPaymentDate = DateTime.UtcNow.AddDays(20);

        var paymentProvider = CreateMockPaymentProvider();
        paymentProvider
            .Setup(x => x.GetSubscription("sub_123"))
            .ReturnsAsync(new ExternalSubscription
            {
                CancelDate = null,
                ConnectedAccountId = null,
                ExternalId = "sub_123",
                ExternalSubscriptionPlanId = "price_123",
                LastPaymentDate = DateTime.UtcNow.AddDays(-10),
                Metadata = new Dictionary<string, string>(),
                NextBillingDate = nextPaymentDate,
                Status = ExternalSubscriptionStatus.Active
            });

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
            paymentProviderFactory: CreateMockPaymentProviderFactory(paymentProvider.Object));

        // Act
        await service.ProcessWebhook(CreateServiceRequest(), webhook);

        // Assert
        paymentProvider.Verify(x => x.GetSubscription("sub_123"), Times.Once);
        paymentProvider.Verify(x => x.GetSubscription("price_123"), Times.Never);

        context.Set<MemberSubscriptionRecord>()
            .Single(x => x.MemberId == member.Id && x.ChapterId == chapter.Id && x.IsCurrent)
            .ExpiresUtc.Should().BeCloseTo(nextPaymentDate, TimeSpan.FromMinutes(5));
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

    // Lapsed but inside the cooldown - still effectively a subscriber, so the period continues.
    [TestCase(-5, 1, true)]
    // Lapsed beyond the cooldown: a returning subscriber starts a new period.
    [TestCase(-70, 1, false)]
    // No cooldown configured, so only a live period continues.
    [TestCase(-5, 0, false)]
    public static async Task ProcessWebhook_SiteSubscription_ContinuesLapsedPeriodOnlyWithinCooldown(
        int expiryDaysFromNow,
        int cooldownMonths,
        bool continuesExistingPeriod)
    {
        // Arrange
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

        var originalExpiry = DateTime.UtcNow.AddDays(expiryDaysFromNow);
        context.CreateMemberSiteSubscription(
            member,
            siteSubscription: siteSubscription,
            expiresUtc: originalExpiry);

        var webhook = CreatePaymentProviderWebhook(
            id: "wh_lapsed",
            type: PaymentProviderWebhookType.InvoicePaymentSucceeded,
            subscriptionId: "sub_123",
            metadata: new PaymentMetadataModel(
                PlatformType.Default,
                PaymentReasonType.SiteSubscription,
                member,
                siteSubscriptionPrice,
                Guid.NewGuid(),
                payment.Id));

        // The default factory returns no subscription, so there is no next payment date to read and the
        // expiry is calculated from the current record.
        var service = CreatePaymentService(
            context,
            siteSubscriptionCooldown: new SiteSubscriptionCooldown(cooldownMonths));
        var request = CreateServiceRequest();

        // Act
        await service.ProcessWebhook(request, webhook);

        // Assert
        var currentRecord = context.Set<MemberSiteSubscriptionRecord>()
            .Single(x => x.MemberId == member.Id && x.IsCurrent);

        var expected = continuesExistingPeriod
            ? originalExpiry.AddMonths(12)
            : DateTime.UtcNow.AddMonths(12);

        currentRecord.ExpiresUtc.Should().BeCloseTo(expected, TimeSpan.FromMinutes(5));
    }

    [Test]
    public static async Task ProcessWebhook_RecurringChapterSubscription_WhenProviderHasNoNextPaymentDate_CalculatesExpiry()
    {
        // Arrange - a provider lookup that comes back empty must not block the payment being recorded, so the
        // expiry degrades to the calculated date rather than being left unset.
        using var context = CreateMockOdkContext();

        var member = context.CreateMember();
        var chapter = context.CreateChapter(members: [member]);
        context.CreateChapterPaymentAccount(chapter);
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
    public static async Task ProcessWebhook_ChapterSubscription_NotifiesTheMemberAsTheGroup()
    {
        /* Arrange - the receipt for a membership payment is sent as the group, so it carries the group's title,
           theme and layout. It went out with no group for as long as it existed, which took the site's. */
        using var context = CreateMockOdkContext();

        var member = context.CreateMember();
        var chapter = context.CreateChapter(members: [member]);
        var chapterSubscription = context.CreateChapterSubscription(chapter: chapter);
        var paymentCheckoutSession = context.CreatePaymentCheckoutSession();

        var webhook = CreatePaymentProviderWebhook(
            id: "wh_chapter_notify",
            type: PaymentProviderWebhookType.CheckoutSessionCompleted,
            subscriptionId: "sub_123",
            metadata: new PaymentMetadataModel(
                PlatformType.Default,
                PaymentReasonType.ChapterSubscription,
                member,
                chapterSubscription,
                paymentCheckoutSession.Id,
                paymentCheckoutSession.PaymentId));

        var memberEmailService = CreateMockMemberEmailService();
        var service = CreatePaymentService(context, memberEmailService: memberEmailService);
        var request = CreateServiceRequest();

        // Act
        await service.ProcessWebhook(request, webhook);

        /* Assert - matched on the request's platform rather than on the instance, because the actioning runs
           as a job and builds its own request from the ids the job carries. */
        Mock.Get(memberEmailService).Verify(
            x => x.SendPaymentNotification(
                It.Is<IServiceRequest>(x => x.Platform == PlatformType.Default),
                It.Is<Member>(x => x.Id == member.Id),
                It.Is<Chapter>(x => x.Id == chapter.Id),
                It.IsAny<Payment>(),
                It.IsAny<Currency>()),
            Times.Once);
    }

    [Test]
    public static async Task ProcessWebhook_ChapterSubscriptionOnAnotherPlatform_NotifiesAsThePaymentsPlatform()
    {
        /* Arrange - a payment provider posts to whichever endpoint was registered with it, which may be one
           site's for both platforms, so the webhook arrives on the Default platform for a Drunken Knitwits
           payment. The receipt has to be sent as the platform the payment was made on: that decides the site
           email settings it comes from and the site its links point at. */
        using var context = CreateMockOdkContext();

        var member = context.CreateMember();
        var chapter = context.CreateChapter(platform: PlatformType.DrunkenKnitwits, members: [member]);
        var chapterSubscription = context.CreateChapterSubscription(chapter: chapter);
        var paymentCheckoutSession = context.CreatePaymentCheckoutSession();

        var webhook = CreatePaymentProviderWebhook(
            id: "wh_chapter_notify_platform",
            type: PaymentProviderWebhookType.CheckoutSessionCompleted,
            subscriptionId: "sub_123",
            metadata: new PaymentMetadataModel(
                PlatformType.DrunkenKnitwits,
                PaymentReasonType.ChapterSubscription,
                member,
                chapterSubscription,
                paymentCheckoutSession.Id,
                paymentCheckoutSession.PaymentId));

        var memberEmailService = CreateMockMemberEmailService();
        var service = CreatePaymentService(context, memberEmailService: memberEmailService);
        var request = CreateServiceRequest(PlatformType.Default);

        // Act
        await service.ProcessWebhook(request, webhook);

        // Assert
        Mock.Get(memberEmailService).Verify(
            x => x.SendPaymentNotification(
                It.Is<IServiceRequest>(x =>
                    x.Platform == PlatformType.DrunkenKnitwits &&
                    x.HttpRequestContext.BaseUrl == TestPlatformProvider.DrunkenKnitwitsBaseUrl),
                It.Is<Member>(x => x.Id == member.Id),
                It.Is<Chapter>(x => x.Id == chapter.Id),
                It.IsAny<Payment>(),
                It.IsAny<Currency>()),
            Times.Once);
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
            .Verify(x => x.SendPaymentNotification(It.IsAny<IServiceRequest>(), It.IsAny<Member>(), It.IsAny<Chapter>(), It.IsAny<Payment>(), It.IsAny<Currency>()), Times.Never);
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

    [Test]
    public static async Task ProcessWebhook_PaymentSucceeded_RecordsWhatTheProviderSettled()
    {
        // Arrange
        using var context = CreateMockOdkContext();

        var member = context.CreateMember();
        var chapter = context.CreateChapter(members: [member]);
        var chapterSubscription = context.CreateChapterSubscription(chapter);

        var payment = context.CreatePayment(member: member);
        var paymentCheckoutSession = context.CreatePaymentCheckoutSession(payment: payment);

        var webhook = CreatePaymentProviderWebhook(
            paymentId: "pi_123",
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

        // Assert - processing a payment queues the settlement read, which fills in what actually moved
        payment = context.Set<Payment>().Single(x => x.Id == payment.Id);

        payment.ActualAmount.Should().Be(100m);
        payment.ActualFeeAmount.Should().Be(1.70m);
        payment.ActualNetAmount.Should().Be(98.30m);
        payment.SettlementCurrencyCode.Should().Be("GBP");
    }

    [Test]
    public static async Task ProcessWebhook_PaymentSucceeded_SchedulesTheSettlementReadForTheProvidersDelay()
    {
        // Arrange
        using var context = CreateMockOdkContext();

        var member = context.CreateMember();
        var chapter = context.CreateChapter(members: [member]);
        var chapterSubscription = context.CreateChapterSubscription(chapter);

        var payment = context.CreatePayment(member: member);
        var paymentCheckoutSession = context.CreatePaymentCheckoutSession(payment: payment);

        var webhook = CreatePaymentProviderWebhook(
            paymentId: "pi_123",
            metadata: new PaymentMetadataModel(
                PlatformType.Default,
                PaymentReasonType.None,
                member,
                chapterSubscription,
                paymentCheckoutSession.Id,
                payment.Id));

        var paymentProvider = CreateMockPaymentProvider();
        paymentProvider
            .Setup(x => x.SettlementReadDelay)
            .Returns(TimeSpan.FromMinutes(2));

        var scheduledFor = new List<DateTime>();

        var service = CreatePaymentService(
            context,
            paymentProviderFactory: CreateMockPaymentProviderFactory(paymentProvider.Object),
            backgroundTaskService: new MockBackgroundTaskService(onSchedule: scheduledFor.Add));

        // Act
        await service.ProcessWebhook(CreateServiceRequest(), webhook);

        /* Assert - the wait before the first read is the provider's to state, since it is the one that knows
           how long after taking the money it finishes moving it. */
        scheduledFor.Should().ContainSingle();
        scheduledFor[0].Should().BeCloseTo(DateTime.UtcNow.AddMinutes(2), TimeSpan.FromSeconds(30));
    }

    [Test]
    public static async Task ResolvePaymentSettlementJob_ReconcilingPaymentWithNoSettings_RecordsTheAccountThatHoldsIt()
    {
        /* Arrange - reconciling, so no ids are handed in and the payment names no account. The one holding
           its reference is found by asking, and recorded because asking proved it. */
        using var context = CreateMockOdkContext();

        var payment = context.CreatePayment();
        payment.ExternalId = "sub_123";
        payment.PaidUtc = DateTime.UtcNow;
        payment.PaymentProvider = PaymentProviderType.Stripe;
        payment.Platform = PlatformType.Default;

        var paymentProvider = CreateMockPaymentProvider();
        paymentProvider
            .Setup(x => x.GetPaymentIdForReference("sub_123", It.IsAny<DateTime>()))
            .ReturnsAsync("pi_456");

        var service = CreatePaymentService(
            context,
            paymentProviderFactory: CreateMockPaymentProviderFactory(paymentProvider.Object));

        // Act
        await service.ResolvePaymentSettlementJob(payment.Id, externalPaymentId: null, externalInvoiceId: null);

        // Assert
        var stored = context.Set<Payment>().Single(x => x.Id == payment.Id);
        stored.Platform.Should().Be(PlatformType.Default);
        stored.PaymentProvider.Should().Be(PaymentProviderType.Stripe);
        stored.ActualAmount.Should().Be(100m);

        paymentProvider.Verify(x => x.GetPaymentSettlement("pi_456"), Times.Once);
    }

    [Test]
    public static async Task ResolvePaymentSettlementJob_ReconcilingPaymentIntentNoAccountHolds_SkipsWithoutThrowing()
    {
        /* Arrange - a reference naming a payment directly, which no enabled account holds. Resolving such a
           reference to an id proves nothing, since it is answered without asking the provider; only reading
           the settlement does, and that is what has to decide whether the account is the right one. */
        using var context = CreateMockOdkContext();

        var payment = context.CreatePayment();
        payment.ExternalId = "pi_gone";
        payment.PaidUtc = DateTime.UtcNow;

        var paymentProvider = CreateMockPaymentProvider();
        paymentProvider
            .Setup(x => x.GetPaymentIdForReference("pi_gone", It.IsAny<DateTime>()))
            .ReturnsAsync("pi_gone");
        paymentProvider
            .Setup(x => x.GetPaymentSettlement(It.IsAny<string>()))
            .ReturnsAsync((ExternalPaymentSettlement?)null);

        var loggingService = CreateMockLoggingService();

        var service = CreatePaymentService(
            context,
            loggingService: loggingService,
            paymentProviderFactory: CreateMockPaymentProviderFactory(paymentProvider.Object));

        // Act
        await service.ResolvePaymentSettlementJob(payment.Id, externalPaymentId: null, externalInvoiceId: null);

        // Assert - no throw, so no retry, and nothing recorded against an account that does not hold it
        var stored = context.Set<Payment>().Single(x => x.Id == payment.Id);
        stored.ActualAmount.Should().BeNull();

        Mock.Get(loggingService)
            .Verify(x => x.Warn(It.Is<string>(m => m.Contains("pi_gone"))), Times.Once);
    }

    [Test]
    public static async Task ResolvePaymentSettlementJob_ReconcilingPaymentNoAccountHolds_SkipsWithoutThrowing()
    {
        /* Arrange - a reference no configured account knows about, which is what a payment taken through an
           account since replaced looks like. */
        using var context = CreateMockOdkContext();

        var payment = context.CreatePayment();
        payment.ExternalId = "sub_gone";
        payment.PaidUtc = DateTime.UtcNow;

        var paymentProvider = CreateMockPaymentProvider();
        paymentProvider
            .Setup(x => x.GetPaymentIdForReference(It.IsAny<string>(), It.IsAny<DateTime>()))
            .ReturnsAsync((string?)null);

        var loggingService = CreateMockLoggingService();

        var service = CreatePaymentService(
            context,
            loggingService: loggingService,
            paymentProviderFactory: CreateMockPaymentProviderFactory(paymentProvider.Object));

        // Act
        await service.ResolvePaymentSettlementJob(payment.Id, externalPaymentId: null, externalInvoiceId: null);

        /* Assert - no throw, so the job is not retried: no number of attempts changes which account holds
           an id. Warned rather than errored, since this is a fact about the data. */
        context.Set<Payment>().Single(x => x.Id == payment.Id)
            .ActualAmount.Should().BeNull();

        Mock.Get(loggingService)
            .Verify(x => x.Warn(It.Is<string>(m => m.Contains("sub_gone"))), Times.Once);

        paymentProvider.Verify(x => x.GetPaymentSettlement(It.IsAny<string>()), Times.Never);

        /* And the reason is kept on the payment, not only logged: it is what tells a site admin why the row
           is still listed, and it is the whole basis for deciding to exclude it. */
        var stored = context.Set<Payment>().Single(x => x.Id == payment.Id);
        stored.ReconciliationFailureReason.Should().Contain("sub_gone");
        stored.ReconciliationFailedUtc.Should().NotBeNull();
    }

    [Test]
    public static async Task ResolvePaymentSettlementJob_IgnoredPayment_ReadsNothing()
    {
        /* Arrange - ignoring is an instruction about the payment, so a read queued directly has to respect
           it too. Hiding it from the page alone would leave the retries running. */
        using var context = CreateMockOdkContext();

        var payment = context.CreatePayment();
        payment.PaidUtc = DateTime.UtcNow;
        payment.ReconciliationIgnoredUtc = DateTime.UtcNow;

        var paymentProvider = CreateMockPaymentProvider();

        var service = CreatePaymentService(
            context,
            paymentProviderFactory: CreateMockPaymentProviderFactory(paymentProvider.Object));

        // Act
        await service.ResolvePaymentSettlementJob(payment.Id, "pi_123", externalInvoiceId: null);

        // Assert
        paymentProvider.Verify(x => x.GetPaymentSettlement(It.IsAny<string>()), Times.Never);

        context.Set<Payment>().Single(x => x.Id == payment.Id)
            .ActualAmount.Should().BeNull();
    }

    [Test]
    public static async Task ResolvePaymentSettlementJob_AlreadyResolved_DoesNotAskTheProviderAgain()
    {
        // Arrange - a redelivered webhook queues the job a second time
        using var context = CreateMockOdkContext();

        var payment = context.CreatePayment();
        payment.ActualAmount = 100m;

        var paymentProvider = CreateMockPaymentProvider();

        var service = CreatePaymentService(
            context,
            paymentProviderFactory: CreateMockPaymentProviderFactory(paymentProvider.Object));

        // Act
        await service.ResolvePaymentSettlementJob(payment.Id, "pi_123", externalInvoiceId: null);

        // Assert
        paymentProvider.Verify(x => x.GetPaymentSettlement(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public static async Task ResolvePaymentSettlementJob_GroupPayment_SplitsTheNetAndTransfersTheGroupsShare()
    {
        // Arrange
        using var context = CreateMockOdkContext();

        var currency = context.CreateCurrency();
        var chapter = context.CreateChapter();
        context.CreateChapterPaymentAccount(chapter, externalId: "acct_123");

        var payment = context.CreatePayment(chapter: chapter, currency: currency);

        var paymentProvider = CreateMockPaymentProvider();

        var service = CreatePaymentService(
            context,
            paymentProviderFactory: CreateMockPaymentProviderFactory(paymentProvider.Object));

        // Act
        await service.ResolvePaymentSettlementJob(payment.Id, "pi_123", externalInvoiceId: null);

        // Assert - our commission comes out of the net, so the provider's fee is met before we take a cut
        var stored = context.Set<Payment>().Single(x => x.Id == payment.Id);
        stored.ActualNetAmount.Should().Be(98.30m);
        stored.ActualCommissionAmount.Should().Be(9.83m);
        stored.ActualConnectedAccountAmount.Should().Be(88.47m);
        stored.TransferredUtc.Should().NotBeNull();

        // The transfer is recorded, so a refund of this payment can reverse it
        stored.ExternalTransferId.Should().Be("tr_123");

        // And an earlier failure is cleared, so a stale reason cannot outlive what it described
        stored.ReconciliationFailureReason.Should().BeNull();
        stored.ReconciliationFailedUtc.Should().BeNull();

        paymentProvider.Verify(
            x => x.CreateTransfer(It.Is<ExternalTransfer>(t =>
                t.Amount == 88.47m &&
                t.ConnectedAccountId == "acct_123" &&
                t.ExternalChargeId == "ch_123")),
            Times.Once);
    }

    [Test]
    public static async Task ResolvePaymentSettlementJob_ProviderAlreadySplitTheCharge_RecordsWhatItDid()
    {
        /* Arrange - a payment taken before the transfer was decoupled: the provider collected our commission
           as part of the charge and made the transfer itself. */
        using var context = CreateMockOdkContext();

        var currency = context.CreateCurrency();
        var chapter = context.CreateChapter();
        context.CreateChapterPaymentAccount(chapter, externalId: "acct_123");

        var payment = context.CreatePayment(chapter: chapter, currency: currency);
        var transferredUtc = new DateTime(2026, 5, 7, 16, 30, 0, DateTimeKind.Utc);

        var paymentProvider = CreateMockPaymentProvider();
        paymentProvider
            .Setup(x => x.GetPaymentSettlement(It.IsAny<string>()))
            .ReturnsAsync(CreateExternalPaymentSettlement(
                collectedCommissionAmount: 5m, transferId: "tr_123", transferredUtc: transferredUtc));

        var service = CreatePaymentService(
            context,
            paymentProviderFactory: CreateMockPaymentProviderFactory(paymentProvider.Object));

        // Act
        await service.ResolvePaymentSettlementJob(payment.Id, "pi_123", externalInvoiceId: null);

        /* Assert - the group kept the charge less the commission the provider collected, and we kept the
           rest of the net. The current commission rate does not come into it: this is a record of what
           happened, not a split to be made. */
        var stored = context.Set<Payment>().Single(x => x.Id == payment.Id);
        stored.ActualNetAmount.Should().Be(98.30m);
        stored.ActualConnectedAccountAmount.Should().Be(95m);
        stored.ActualCommissionAmount.Should().Be(3.30m);
        stored.TransferredUtc.Should().Be(transferredUtc);

        // The provider's own transfer is recorded, so a refund of this payment can still reverse it
        stored.ExternalTransferId.Should().Be("tr_123");

        // And nothing is transferred: that money moved when the charge was made
        paymentProvider.Verify(x => x.CreateTransfer(It.IsAny<ExternalTransfer>()), Times.Never);
    }

    [Test]
    public static async Task ResolvePaymentSettlementJob_GroupPaymentAlreadyTransferred_DoesNotTransferAgain()
    {
        // Arrange - a redelivered webhook, or a retry after the transfer went through
        using var context = CreateMockOdkContext();

        var currency = context.CreateCurrency();
        var chapter = context.CreateChapter();
        context.CreateChapterPaymentAccount(chapter, externalId: "acct_123");

        var payment = context.CreatePayment(chapter: chapter, currency: currency);
        payment.ActualAmount = 100m;
        payment.ActualConnectedAccountAmount = 88.47m;
        payment.ExternalTransferId = "tr_123";
        payment.TransferredUtc = DateTime.UtcNow.AddMinutes(-5);

        var paymentProvider = CreateMockPaymentProvider();

        var service = CreatePaymentService(
            context,
            paymentProviderFactory: CreateMockPaymentProviderFactory(paymentProvider.Object));

        // Act
        await service.ResolvePaymentSettlementJob(payment.Id, "pi_123", externalInvoiceId: null);

        // Assert - paying a group twice is the one failure this whole path exists to avoid
        paymentProvider.Verify(x => x.CreateTransfer(It.IsAny<ExternalTransfer>()), Times.Never);

        // And nothing is re-read: the payment has everything the provider could tell it
        paymentProvider.Verify(x => x.GetPaymentSettlement(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public static async Task ResolvePaymentSettlementJob_SettledPaymentNamesNoTransfer_RecordsProvidersOwnTransfer()
    {
        /* Arrange - a payment settled before the transfer was recorded, which the provider transferred
           itself. Its transfer is named by the charge. */
        using var context = CreateMockOdkContext();

        var currency = context.CreateCurrency();
        var chapter = context.CreateChapter();
        context.CreateChapterPaymentAccount(chapter, externalId: "acct_123");

        var payment = context.CreatePayment(chapter: chapter, currency: currency);
        payment.ActualAmount = 100m;
        payment.ActualCommissionAmount = 42m;
        payment.ActualConnectedAccountAmount = 88.47m;
        payment.TransferredUtc = DateTime.UtcNow.AddMonths(-3);

        var paymentProvider = CreateMockPaymentProvider();
        paymentProvider
            .Setup(x => x.GetPaymentSettlement(It.IsAny<string>()))
            .ReturnsAsync(CreateExternalPaymentSettlement(transferId: "tr_123"));

        var service = CreatePaymentService(
            context,
            paymentProviderFactory: CreateMockPaymentProviderFactory(paymentProvider.Object));

        // Act
        await service.ResolvePaymentSettlementJob(payment.Id, "pi_123", externalInvoiceId: null);

        // Assert
        var stored = context.Set<Payment>().Single(x => x.Id == payment.Id);
        stored.ExternalTransferId.Should().Be("tr_123");

        /* And the split is left exactly as it was recorded. Re-running the settlement would work the
           commission out from the current rate and restate figures that never occurred. */
        stored.ActualCommissionAmount.Should().Be(42m);
        stored.ActualConnectedAccountAmount.Should().Be(88.47m);
    }

    [Test]
    public static async Task ResolvePaymentSettlementJob_SettledPaymentNamesNoTransfer_SearchesForOneAgainstTheCharge()
    {
        // Arrange - a charge we collected whole, whose transfer is only reachable from the transfer's side
        using var context = CreateMockOdkContext();

        var currency = context.CreateCurrency();
        var chapter = context.CreateChapter();
        context.CreateChapterPaymentAccount(chapter, externalId: "acct_123");

        var payment = context.CreatePayment(chapter: chapter, currency: currency);
        payment.ActualAmount = 100m;
        payment.ActualConnectedAccountAmount = 88.47m;
        payment.TransferredUtc = DateTime.UtcNow.AddMonths(-3);

        var chargedUtc = new DateTime(2026, 5, 7, 16, 0, 0, DateTimeKind.Utc);

        var paymentProvider = CreateMockPaymentProvider();
        paymentProvider
            .Setup(x => x.GetPaymentSettlement(It.IsAny<string>()))
            .ReturnsAsync(CreateExternalPaymentSettlement());

        paymentProvider
            .Setup(x => x.FindTransferIdForCharge("ch_123", "acct_123", chargedUtc))
            .ReturnsAsync("tr_456");

        var service = CreatePaymentService(
            context,
            paymentProviderFactory: CreateMockPaymentProviderFactory(paymentProvider.Object));

        // Act
        await service.ResolvePaymentSettlementJob(payment.Id, "pi_123", externalInvoiceId: null);

        // Assert
        context.Set<Payment>().Single(x => x.Id == payment.Id)
            .ExternalTransferId.Should().Be("tr_456");
    }

    [Test]
    public static async Task ResolvePaymentSettlementJob_SettledPaymentTransferNotFound_WarnsAndLeavesItUnrecorded()
    {
        // Arrange - the provider knows of no transfer out of the charge, by either route
        using var context = CreateMockOdkContext();

        var currency = context.CreateCurrency();
        var chapter = context.CreateChapter();
        context.CreateChapterPaymentAccount(chapter, externalId: "acct_123");

        var payment = context.CreatePayment(chapter: chapter, currency: currency);
        payment.ActualAmount = 100m;
        payment.ActualConnectedAccountAmount = 88.47m;
        payment.TransferredUtc = DateTime.UtcNow.AddMonths(-3);

        var loggingService = new Mock<ILoggingService>();

        var paymentProvider = CreateMockPaymentProvider();
        paymentProvider
            .Setup(x => x.GetPaymentSettlement(It.IsAny<string>()))
            .ReturnsAsync(CreateExternalPaymentSettlement());

        var service = CreatePaymentService(
            context,
            loggingService: loggingService.Object,
            paymentProviderFactory: CreateMockPaymentProviderFactory(paymentProvider.Object));

        // Act
        await service.ResolvePaymentSettlementJob(payment.Id, "pi_123", externalInvoiceId: null);

        /* Assert - warned rather than thrown, because no number of retries will make the provider name a
           transfer it does not have. The payment stays unrefundable from the group's share, which is what
           the warning is there to surface. */
        context.Set<Payment>().Single(x => x.Id == payment.Id)
            .ExternalTransferId.Should().BeNull();

        loggingService.Verify(
            x => x.Warn(It.Is<string>(m => m.Contains(payment.Id.ToString()))), Times.Once);
    }

    [Test]
    public static async Task ResolvePaymentSettlementJob_GroupPaymentTransferFails_ThrowsAndLeavesItOwed()
    {
        // Arrange
        using var context = CreateMockOdkContext();

        var currency = context.CreateCurrency();
        var chapter = context.CreateChapter();
        context.CreateChapterPaymentAccount(chapter, externalId: "acct_123");

        var payment = context.CreatePayment(chapter: chapter, currency: currency);

        var paymentProvider = CreateMockPaymentProvider();
        paymentProvider
            .Setup(x => x.CreateTransfer(It.IsAny<ExternalTransfer>()))
            .ReturnsAsync(CreateTransferResult.Failure("no such account"));

        var service = CreatePaymentService(
            context,
            paymentProviderFactory: CreateMockPaymentProviderFactory(paymentProvider.Object));

        // Act
        var act = async () => await service.ResolvePaymentSettlementJob(
            payment.Id, "pi_123", externalInvoiceId: null);

        /* Assert - the settlement stands, so the row states what is owed, and no transfer date is written.
           A payment with a chapter, a settlement and no date is money still to pay. */
        await act.Should().ThrowAsync<OdkServiceException>();

        var stored = context.Set<Payment>().Single(x => x.Id == payment.Id);
        stored.ActualConnectedAccountAmount.Should().Be(88.47m);
        stored.TransferredUtc.Should().BeNull();
    }

    [Test]
    public static async Task ResolvePaymentSettlementJob_SitePayment_KeepsTheNetAndTransfersNothing()
    {
        // Arrange - a payment to the site has no connected account to split with
        using var context = CreateMockOdkContext();

        var payment = context.CreatePayment();

        var paymentProvider = CreateMockPaymentProvider();

        var service = CreatePaymentService(
            context,
            paymentProviderFactory: CreateMockPaymentProviderFactory(paymentProvider.Object));

        // Act
        await service.ResolvePaymentSettlementJob(payment.Id, "pi_123", externalInvoiceId: null);

        // Assert
        var stored = context.Set<Payment>().Single(x => x.Id == payment.Id);
        stored.ActualNetAmount.Should().Be(98.30m);
        stored.ActualCommissionAmount.Should().BeNull();
        stored.ActualConnectedAccountAmount.Should().BeNull();
        stored.TransferredUtc.Should().BeNull();

        paymentProvider.Verify(x => x.CreateTransfer(It.IsAny<ExternalTransfer>()), Times.Never);
    }

    [Test]
    public static async Task ResolvePaymentSettlementJob_InvoiceWithNoPayment_Throws()
    {
        // Arrange - an invoice naming no payment leaves nothing to read a settlement off
        using var context = CreateMockOdkContext();

        var payment = context.CreatePayment();

        var paymentProvider = CreateMockPaymentProvider();
        paymentProvider
            .Setup(x => x.GetInvoicePaymentId(It.IsAny<string>()))
            .ReturnsAsync((string?)null);

        var service = CreatePaymentService(
            context,
            paymentProviderFactory: CreateMockPaymentProviderFactory(paymentProvider.Object));

        // Act
        var act = async () => await service.ResolvePaymentSettlementJob(
            payment.Id, externalPaymentId: null, externalInvoiceId: "in_123");

        // Assert
        await act.Should().ThrowAsync<OdkServiceException>();
    }

    [Test]
    public static async Task ResolvePaymentSettlementJob_NotSettledYet_Throws()
    {
        // Arrange - the charge exists but its balance transaction does not
        using var context = CreateMockOdkContext();

        var payment = context.CreatePayment();

        var paymentProvider = CreateMockPaymentProvider();
        paymentProvider
            .Setup(x => x.GetPaymentSettlement(It.IsAny<string>()))
            .ReturnsAsync(CreateExternalPaymentSettlement(settled: false));

        var service = CreatePaymentService(
            context,
            paymentProviderFactory: CreateMockPaymentProviderFactory(paymentProvider.Object));

        // Act
        var act = async () => await service.ResolvePaymentSettlementJob(
            payment.Id, "pi_123", externalInvoiceId: null);

        /* Assert - throwing is what earns the job a retry, and nothing is written in the meantime: a
           half-filled row would satisfy the already-resolved guard and never be completed. */
        await act.Should().ThrowAsync<OdkServiceException>();

        context.Set<Payment>().Single(x => x.Id == payment.Id)
            .ActualAmount.Should().BeNull();
    }

    [Test]
    public static async Task ResolvePaymentSettlementJob_SubscriptionRenewal_ReadsThePaymentOffTheInvoice()
    {
        // Arrange - a renewal's webhook names its invoice and no payment
        using var context = CreateMockOdkContext();

        var payment = context.CreatePayment();

        var paymentProvider = CreateMockPaymentProvider();
        paymentProvider
            .Setup(x => x.GetInvoicePaymentId("in_123"))
            .ReturnsAsync("pi_456");

        var service = CreatePaymentService(
            context,
            paymentProviderFactory: CreateMockPaymentProviderFactory(paymentProvider.Object));

        // Act
        await service.ResolvePaymentSettlementJob(
            payment.Id, externalPaymentId: null, externalInvoiceId: "in_123");

        // Assert
        paymentProvider.Verify(x => x.GetPaymentSettlement("pi_456"), Times.Once);

        context.Set<Payment>().Single(x => x.Id == payment.Id)
            .ActualAmount.Should().Be(100m);
    }

    [Test]
    public static async Task ResolvePaymentSettlementNow_NoAccountHoldsThePayment_ReturnsTheReason()
    {
        /* Arrange - the case the reconciliation page exists to explain: a reference no configured account
           knows about. A caller waiting on the answer has to be given it, not left reading the log. */
        using var context = CreateMockOdkContext();

        var payment = context.CreatePayment();
        payment.ExternalId = "sub_gone";
        payment.PaidUtc = DateTime.UtcNow;

        var paymentProvider = CreateMockPaymentProvider();
        paymentProvider
            .Setup(x => x.GetPaymentIdForReference(It.IsAny<string>(), It.IsAny<DateTime>()))
            .ReturnsAsync((string?)null);

        var service = CreatePaymentService(
            context,
            paymentProviderFactory: CreateMockPaymentProviderFactory(paymentProvider.Object));

        // Act
        var result = await service.ResolvePaymentSettlement(payment.Id);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("sub_gone");
        result.Transferred.Should().BeFalse();
    }

    [Test]
    public static async Task ResolvePaymentSettlementNow_NotSettledYet_ReturnsAFailureRatherThanThrowing()
    {
        /* Arrange - the queued job throws here so Hangfire retries it. Nothing is retrying a site admin's
           button press, so the same state has to come back as an answer. */
        using var context = CreateMockOdkContext();

        var payment = context.CreatePayment();

        var paymentProvider = CreateMockPaymentProvider();
        paymentProvider
            .Setup(x => x.GetPaymentSettlement(It.IsAny<string>()))
            .ReturnsAsync(CreateExternalPaymentSettlement(settled: false));

        var service = CreatePaymentService(
            context,
            paymentProviderFactory: CreateMockPaymentProviderFactory(paymentProvider.Object));

        // Act
        var result = await service.ResolvePaymentSettlement(payment.Id);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().NotBeNull();

        // And it is recorded, so the page states it beside the row until a later read clears it
        context.Set<Payment>().Single(x => x.Id == payment.Id)
            .ReconciliationFailureReason.Should().NotBeNull();
    }

    [Test]
    public static async Task ResolvePaymentSettlementNow_GroupPayment_ReportsThatMoneyMoved()
    {
        /* Arrange - the distinction a site admin needs: this reconcile paid the group, rather than only
           writing to our own records. Not derivable from the payment afterwards, where a transfer made
           months ago looks the same as one made just now. */
        using var context = CreateMockOdkContext();

        var currency = context.CreateCurrency();
        var chapter = context.CreateChapter();
        context.CreateChapterPaymentAccount(chapter, externalId: "acct_123");

        var payment = context.CreatePayment(
            chapter: chapter, currency: currency, paidUtc: DateTime.UtcNow.AddMinutes(-1));
        payment.ExternalId = "sub_123";

        var paymentProvider = CreateMockPaymentProvider();
        paymentProvider
            .Setup(x => x.GetPaymentIdForReference(It.IsAny<string>(), It.IsAny<DateTime>()))
            .ReturnsAsync("pi_123");

        var service = CreatePaymentService(
            context,
            paymentProviderFactory: CreateMockPaymentProviderFactory(paymentProvider.Object));

        // Act
        var result = await service.ResolvePaymentSettlement(payment.Id);

        // Assert
        result.Success.Should().BeTrue();
        result.Transferred.Should().BeTrue();
    }

    [Test]
    public static async Task ResolvePaymentSettlementNow_SitePayment_ReportsThatNoMoneyMoved()
    {
        // Arrange - no connected account, so the reconcile only reads
        using var context = CreateMockOdkContext();

        var payment = context.CreatePayment(paidUtc: DateTime.UtcNow.AddMinutes(-1));
        payment.ExternalId = "sub_123";

        var paymentProvider = CreateMockPaymentProvider();
        paymentProvider
            .Setup(x => x.GetPaymentIdForReference(It.IsAny<string>(), It.IsAny<DateTime>()))
            .ReturnsAsync("pi_123");

        var service = CreatePaymentService(
            context,
            paymentProviderFactory: CreateMockPaymentProviderFactory(paymentProvider.Object));

        // Act
        var result = await service.ResolvePaymentSettlement(payment.Id);

        // Assert
        result.Success.Should().BeTrue();
        result.Transferred.Should().BeFalse();
    }

    [Test]
    public static async Task ResolvePaymentSettlement_GroupPayment_UpdatesTheSamePaymentTwice()
    {
        /* Arrange - the settlement is recorded and committed before the transfer is made, so one payment is
           updated twice in one request. Reading without tracking gives each read its own instance, and the
           repository updates through a clone - so this is the case that has to keep working. */
        using var context = new MockOdkContext(noTracking: true);

        var currency = context.CreateCurrency();
        var chapter = context.CreateChapter(country: context.CreateCountry(currency));
        context.CreateChapterPaymentAccount(chapter, externalId: "acct_123");

        var payment = context.CreatePayment(
            chapter: chapter, currency: currency, paidUtc: DateTime.UtcNow.AddMinutes(-1));
        payment.ExternalId = "sub_123";

        var paymentProvider = CreateMockPaymentProvider();
        paymentProvider
            .Setup(x => x.GetPaymentIdForReference(It.IsAny<string>(), It.IsAny<DateTime>()))
            .ReturnsAsync("pi_123");

        var service = CreatePaymentService(
            context,
            paymentProviderFactory: CreateMockPaymentProviderFactory(paymentProvider.Object));

        // Seeding leaves its entities tracked, which a request never starts with.
        context.ChangeTracker.Clear();

        // Act
        var result = await service.ResolvePaymentSettlement(payment.Id);

        // Assert
        result.Success.Should().BeTrue();
        result.Transferred.Should().BeTrue();

        var stored = context.Set<Payment>().Single(x => x.Id == payment.Id);
        stored.ActualAmount.Should().Be(100m);
        stored.TransferredUtc.Should().NotBeNull();
    }

    private static ExternalPaymentSettlement CreateExternalPaymentSettlement(
        decimal? amount = null,
        bool settled = true,
        decimal? collectedCommissionAmount = null,
        string? transferId = null,
        DateTime? transferredUtc = null)
        => new ExternalPaymentSettlement
        {
            Amount = amount ?? 100m,
            ChargedUtc = new DateTime(2026, 5, 7, 16, 0, 0, DateTimeKind.Utc),
            ChargeId = "ch_123",
            CollectedCommissionAmount = collectedCommissionAmount,
            CurrencyCode = "GBP",
            FeeAmount = settled ? 1.70m : null,
            NetAmount = settled ? (amount ?? 100m) - 1.70m : null,
            SettlementCurrencyCode = settled ? "GBP" : null,
            TransferId = transferId,
            TransferredUtc = transferredUtc
        };

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

    private static IPaymentUpdateBroadcaster CreateMockPaymentUpdateBroadcaster()
    {
        var mock = new Mock<IPaymentUpdateBroadcaster>();
        mock.Setup(x => x.CheckoutSessionUpdated(It.IsAny<string>())).Returns(Task.CompletedTask);
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
                It.IsAny<Currency>()))
            .Returns(Task.CompletedTask);
        return mock.Object;
    }

    private static MockOdkContext CreateMockOdkContext()
    {
        var context = new MockOdkContext();

        return context;
    }

    // Returning no subscription by default leaves a recurring expiry falling back to the calculated date,
    // which is what most tests here assert. Pass a date to exercise the provider lookup.
    private static Mock<IPaymentProvider> CreateMockPaymentProvider(DateTime? nextBillingDate = null)
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

        /* Settling by default, because every processed payment queues a settlement read - one that throws
           to earn a retry while the provider has nothing to give, so a provider that never settles would
           fail every test here rather than only the ones about settling. */
        paymentProvider
            .Setup(x => x.GetPaymentSettlement(It.IsAny<string>()))
            .ReturnsAsync(CreateExternalPaymentSettlement());

        paymentProvider
            .Setup(x => x.CommissionPercentage)
            .Returns(10m);

        paymentProvider
            .Setup(x => x.CreateTransfer(It.IsAny<ExternalTransfer>()))
            .ReturnsAsync(CreateTransferResult.Transferred("tr_123"));

        // Read immediately, so a test's assertions do not depend on when the job was scheduled for.
        paymentProvider
            .Setup(x => x.SettlementReadDelay)
            .Returns(TimeSpan.Zero);

        paymentProvider
            .Setup(x => x.Type)
            .Returns(PaymentProviderType.Stripe);

        return paymentProvider;
    }

    private static IPaymentProviderFactory CreateMockPaymentProviderFactory(DateTime? nextBillingDate = null)
        => CreateMockPaymentProviderFactory(CreateMockPaymentProvider(nextBillingDate).Object);

    private static IPaymentProviderFactory CreateMockPaymentProviderFactory(IPaymentProvider paymentProvider)
    {
        var factory = new Mock<IPaymentProviderFactory>();

        factory
            .Setup(x => x.GetPaymentProvider(It.IsAny<PlatformType>()))
            .Returns(paymentProvider);

        factory
            .Setup(x => x.GetPaymentProvider(
                It.IsAny<PaymentProviderType>(), It.IsAny<PlatformType>()))
            .Returns(paymentProvider);

        factory
            .Setup(x => x.GetPaymentProvider(
                It.IsAny<PaymentProviderType>(),
                It.IsAny<PlatformType>()))
            .Returns(paymentProvider);


        factory
            .Setup(x => x.GetPaymentProviderOrDefault(
                It.IsAny<PaymentProviderType>(), It.IsAny<PlatformType>()))
            .Returns(paymentProvider);

        return factory.Object;
    }

    private static IUnitOfWork CreateMockUnitOfWork(MockOdkContext? context = null) => MockUnitOfWorkFactory.Create(context);

    private static PaymentService CreatePaymentService(
        MockOdkContext? context = null,
        ILoggingService? loggingService = null,
        IMemberEmailService? memberEmailService = null,
        IPaymentProviderFactory? paymentProviderFactory = null,
        IPaymentUpdateBroadcaster? paymentUpdateBroadcaster = null,
        IEventService? eventService = null,
        SiteSubscriptionCooldown? siteSubscriptionCooldown = null,
        IBackgroundTaskService? backgroundTaskService = null)
    {
        var unitOfWork = CreateMockUnitOfWork(context);
        return new PaymentService(
            unitOfWork,
            loggingService ?? CreateMockLoggingService(),
            memberEmailService ?? CreateMockMemberEmailService(),
            paymentProviderFactory ?? CreateMockPaymentProviderFactory(),
            paymentUpdateBroadcaster ?? CreateMockPaymentUpdateBroadcaster(),
            eventService ?? CreateMockEventService(),
            backgroundTaskService ?? new MockBackgroundTaskService(),
            new MemberChapterSubscriptionWriter(unitOfWork),
            new MemberSiteSubscriptionWriter(unitOfWork),
            TestPlatformProvider.Create(),
            new MockServiceRequestFactory(context),
            siteSubscriptionCooldown ?? new SiteSubscriptionCooldown(months: 0));
    }

    private static IServiceRequest CreateServiceRequest(PlatformType? platform = null)
    {
        var mock = new Mock<IServiceRequest>();

        // Set because anything queueing a job reads the base URL off it to build the job's request.
        mock.Setup(x => x.HttpRequestContext)
            .Returns(new JobHttpRequestContext { BaseUrl = "https://example.com" });

        mock.Setup(x => x.Environment)
            .Returns(EnvironmentType.Dev);

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
        string? invoiceId = null,
        PaymentMetadataModel? metadata = null,
        IReadOnlyDictionary<string, string>? metadataDictionary = null,
        decimal? amount = null)
        => new PaymentProviderWebhook
        {
            Id = id ?? "wh_123",
            Type = type ?? PaymentProviderWebhookType.CheckoutSessionCompleted,
            Complete = complete,
            InvoiceId = invoiceId,
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