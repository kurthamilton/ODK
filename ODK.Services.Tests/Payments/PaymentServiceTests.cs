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
using ODK.Data.Core.Repositories;
using ODK.Services.Events;
using ODK.Services.Logging;
using ODK.Services.Members;
using ODK.Services.Payments;
using ODK.Services.Payments.Models;
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
        var webhook = CreatePaymentProviderWebhook();
        var existingEvent = new PaymentProviderWebhookEvent
        {
            ExternalId = webhook.Id,
            PaymentProviderType = webhook.PaymentProviderType,
            ReceivedUtc = DateTime.UtcNow
        };

        var webhookEventRepository = CreateMockPaymentProviderWebhookEventRepository([existingEvent]);

        var unitOfWork = CreateMockUnitOfWork(
            paymentProviderWebhookEventRepository: webhookEventRepository);

        var service = CreatePaymentService(unitOfWork);
        var request = CreateServiceRequest();

        // Act
        await service.ProcessWebhook(request, webhook);

        // Assert
        Mock.Get(webhookEventRepository)
            .Verify(x => x.Add(It.IsAny<PaymentProviderWebhookEvent>()), Times.Never);
    }

    [Test]
    public static async Task ProcessWebhook_WhenEventIsNew_AddsWebhookEvent()
    {
        // Arrange
        var webhook = CreatePaymentProviderWebhook(type: PaymentProviderWebhookType.CheckoutSessionExpired);

        var webhookEventRepository = CreateMockPaymentProviderWebhookEventRepository([]);

        var unitOfWork = CreateMockUnitOfWork(
            paymentProviderWebhookEventRepository: webhookEventRepository);

        var service = CreatePaymentService(unitOfWork);
        var request = CreateServiceRequest();

        // Act
        await service.ProcessWebhook(request, webhook);

        // Assert
        Mock.Get(webhookEventRepository)
            .Verify(x => x.Add(It.Is<PaymentProviderWebhookEvent>(
                x => x.ExternalId == webhook.Id)), Times.Once);
    }

    [TestCase(PaymentProviderWebhookType.CheckoutSessionCompleted)]
    [TestCase(PaymentProviderWebhookType.PaymentSucceeded)]
    public static async Task ProcessWebhook_PaymentSucceeded(PaymentProviderWebhookType webhookType)
    {
        // Arrange
        var member = CreateMember();
        var chapter = CreateChapter();
        var currency = CreateCurrency();
        var payment = CreatePayment();
        var paymentCheckoutSession = CreatePaymentCheckoutSession();

        var webhook = CreatePaymentProviderWebhook(
            type: webhookType,
            paymentId: payment.Id.ToString(),
            metadata: new Dictionary<string, string>
            {
                { "MemberId", member.Id.ToString() },
                { "PaymentId", payment.Id.ToString() },
                { "PaymentCheckoutSessionId", paymentCheckoutSession.Id.ToString() }
            });

        var webhookEventRepository = CreateMockPaymentProviderWebhookEventRepository([]);
        var memberRepository = CreateMockMemberRepository(member);
        var paymentRepository = CreateMockPaymentRepository([payment]);

        var paymentCheckoutSessionRepository = CreateMockPaymentCheckoutSessionRepository(paymentCheckoutSession);
        var eventTicketPaymentRepository = CreateMockEventTicketPaymentRepository(null);

        var unitOfWork = CreateMockUnitOfWork(
            paymentProviderWebhookEventRepository: webhookEventRepository,
            memberRepository: memberRepository,
            paymentRepository: paymentRepository,
            paymentCheckoutSessionRepository: paymentCheckoutSessionRepository,
            eventTicketPaymentRepository: eventTicketPaymentRepository);

        var service = CreatePaymentService(unitOfWork);
        var request = CreateServiceRequest();

        // Act
        await service.ProcessWebhook(request, webhook);

        // Assert
        payment.PaidUtc.Should().NotBeNull();
        payment.ExternalId.Should().Be(webhook.PaymentId);

        Mock.Get(paymentRepository)
            .Verify(x => x.Update(payment), Times.Once);

        unitOfWork.Mock.Verify(x => x.SaveChangesAsync(), Times.AtLeastOnce);
    }

    [Test]
    public static async Task ProcessWebhook_WhenCheckoutSessionExpired_UpdatesSessionExpiry()
    {
        // Arrange
        var payment = CreatePayment();
        var paymentRepository = CreateMockPaymentRepository([payment]);

        var paymentCheckoutSession = CreatePaymentCheckoutSession();

        var webhook = CreatePaymentProviderWebhook(
            type: PaymentProviderWebhookType.CheckoutSessionExpired,
            metadata: new Dictionary<string, string>
            {
                { "PaymentCheckoutSessionId", paymentCheckoutSession.Id.ToString() },
                { "PaymentId", payment.Id.ToString() }
            });

        var webhookEventRepository = CreateMockPaymentProviderWebhookEventRepository([]);
        var paymentCheckoutSessionRepository = CreateMockPaymentCheckoutSessionRepository(paymentCheckoutSession);

        var unitOfWork = CreateMockUnitOfWork(
            paymentProviderWebhookEventRepository: webhookEventRepository,
            paymentCheckoutSessionRepository: paymentCheckoutSessionRepository,
            paymentRepository: paymentRepository);

        var service = CreatePaymentService(unitOfWork);
        var request = CreateServiceRequest();

        // Act
        await service.ProcessWebhook(request, webhook);

        // Assert
        paymentCheckoutSession.ExpiredUtc.Should().NotBeNull();

        Mock.Get(paymentCheckoutSessionRepository)
            .Verify(x => x.Update(paymentCheckoutSession), Times.Once);
    }

    [TestCase(PaymentProviderWebhookType.InvoicePaymentSucceeded)]
    [TestCase(PaymentProviderWebhookType.SubscriptionCancelled)]
    public static async Task ProcessWebhook_WhenSubscriptionWebhookType_ProcessesSubscription(PaymentProviderWebhookType webhookType)
    {
        // Arrange
        var webhook = CreatePaymentProviderWebhook(
            type: webhookType,
            subscriptionId: "sub_123",
            metadata: new Dictionary<string, string>
            {
                { "ChapterSubscriptionId", Guid.NewGuid().ToString() }
            });

        var webhookEventRepository = CreateMockPaymentProviderWebhookEventRepository([]);

        var memberSubscriptionRecord = CreateMemberSubscriptionRecord();
        var memberSubscriptionRecordRepository = CreateMockMemberSubscriptionRecordRepository([memberSubscriptionRecord]);

        var unitOfWork = CreateMockUnitOfWork(
            paymentProviderWebhookEventRepository: webhookEventRepository,
            memberSubscriptionRecordRepository: memberSubscriptionRecordRepository);

        var service = CreatePaymentService(unitOfWork);
        var request = CreateServiceRequest();

        // Act
        await service.ProcessWebhook(request, webhook);

        // Assert
        unitOfWork.Mock.Verify(x => x.SaveChangesAsync(), Times.AtLeastOnce);
    }

    [Test]
    public static async Task ProcessWebhook_WhenInvalidWebhookType_DoesNotSendEmail()
    {
        // Arrange
        var webhook = CreatePaymentProviderWebhook(type: PaymentProviderWebhookType.None);

        var webhookEventRepository = CreateMockPaymentProviderWebhookEventRepository([]);
        var memberEmailService = CreateMockMemberEmailService();

        var unitOfWork = CreateMockUnitOfWork(
            paymentProviderWebhookEventRepository: webhookEventRepository);

        var service = CreatePaymentService(unitOfWork, memberEmailService: memberEmailService);
        var request = CreateServiceRequest();

        // Act
        await service.ProcessWebhook(request, webhook);

        // Assert
        Mock.Get(memberEmailService)
            .Verify(x => x.SendPaymentNotification(It.IsAny<IServiceRequest>(), It.IsAny<Member>(), It.IsAny<Chapter>(), It.IsAny<Payment>(), It.IsAny<Currency>(), It.IsAny<SiteEmailSettings>()), Times.Never);
    }

    [Test]
    public static async Task ProcessWebhook_WhenWebhookIncomplete_DoesNotProcess()
    {
        // Arrange
        var webhook = CreatePaymentProviderWebhook(
            type: PaymentProviderWebhookType.PaymentSucceeded,
            complete: false,
            paymentId: "pay_123");

        var webhookEventRepository = CreateMockPaymentProviderWebhookEventRepository([]);
        var loggingService = CreateMockLoggingService();

        var unitOfWork = CreateMockUnitOfWork(
            paymentProviderWebhookEventRepository: webhookEventRepository);

        var service = CreatePaymentService(unitOfWork, loggingService: loggingService);
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
        var member = CreateMember();
        var payment = CreatePayment(paidUtc: DateTime.UtcNow.AddDays(-1));
        var paymentCheckoutSession = CreatePaymentCheckoutSession();

        var webhook = CreatePaymentProviderWebhook(
            type: PaymentProviderWebhookType.PaymentSucceeded,
            paymentId: payment.Id.ToString(),
            metadata: new Dictionary<string, string>
            {
                { "MemberId", member.Id.ToString() },
                { "PaymentId", payment.Id.ToString() },
                { "PaymentCheckoutSessionId", paymentCheckoutSession.Id.ToString() }
            });

        var webhookEventRepository = CreateMockPaymentProviderWebhookEventRepository([]);
        var memberRepository = CreateMockMemberRepository(member);
        var paymentRepository = CreateMockPaymentRepository([payment]);
        var paymentCheckoutSessionRepository = CreateMockPaymentCheckoutSessionRepository(paymentCheckoutSession);

        var unitOfWork = CreateMockUnitOfWork(
            paymentProviderWebhookEventRepository: webhookEventRepository,
            memberRepository: memberRepository,
            paymentRepository: paymentRepository,
            paymentCheckoutSessionRepository: paymentCheckoutSessionRepository);

        var loggingService = CreateMockLoggingService();
        var service = CreatePaymentService(unitOfWork, loggingService: loggingService);
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
        var member = CreateMember();
        var payment = CreatePayment();
        var paymentCheckoutSession = CreatePaymentCheckoutSession(completedUtc: DateTime.UtcNow.AddDays(-1));

        var webhook = CreatePaymentProviderWebhook(
            type: PaymentProviderWebhookType.PaymentSucceeded,
            paymentId: payment.Id.ToString(),
            metadata: new Dictionary<string, string>
            {
                { "MemberId", member.Id.ToString() },
                { "PaymentId", payment.Id.ToString() },
                { "PaymentCheckoutSessionId", paymentCheckoutSession.Id.ToString() }
            });

        var webhookEventRepository = CreateMockPaymentProviderWebhookEventRepository([]);
        var memberRepository = CreateMockMemberRepository(member);
        var paymentRepository = CreateMockPaymentRepository([payment]);
        var paymentCheckoutSessionRepository = CreateMockPaymentCheckoutSessionRepository(paymentCheckoutSession);

        var unitOfWork = CreateMockUnitOfWork(
            paymentProviderWebhookEventRepository: webhookEventRepository,
            memberRepository: memberRepository,
            paymentRepository: paymentRepository,
            paymentCheckoutSessionRepository: paymentCheckoutSessionRepository);

        var loggingService = CreateMockLoggingService();
        var service = CreatePaymentService(unitOfWork, loggingService: loggingService);
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
            type: PaymentProviderWebhookType.PaymentSucceeded,
            paymentId: "pay_123",
            metadata: new Dictionary<string, string>());

        var webhookEventRepository = CreateMockPaymentProviderWebhookEventRepository([]);
        var loggingService = CreateMockLoggingService();

        var unitOfWork = CreateMockUnitOfWork(
            paymentProviderWebhookEventRepository: webhookEventRepository);

        var service = CreatePaymentService(unitOfWork, loggingService: loggingService);
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
            metadata: new Dictionary<string, string>());

        var webhookEventRepository = CreateMockPaymentProviderWebhookEventRepository([]);
        var loggingService = CreateMockLoggingService();

        var unitOfWork = CreateMockUnitOfWork(
            paymentProviderWebhookEventRepository: webhookEventRepository);

        var service = CreatePaymentService(unitOfWork, loggingService: loggingService);
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
        var paymentCheckoutSession = CreatePaymentCheckoutSession(expiredUtc: DateTime.UtcNow.AddDays(-1));
        var webhook = CreatePaymentProviderWebhook(
            type: PaymentProviderWebhookType.CheckoutSessionExpired,
            metadata: new Dictionary<string, string>
            {
                { "PaymentCheckoutSessionId", paymentCheckoutSession.Id.ToString() }
            });

        var webhookEventRepository = CreateMockPaymentProviderWebhookEventRepository([]);
        var paymentCheckoutSessionRepository = CreateMockPaymentCheckoutSessionRepository(paymentCheckoutSession);
        var loggingService = CreateMockLoggingService();

        var unitOfWork = CreateMockUnitOfWork(
            paymentProviderWebhookEventRepository: webhookEventRepository,
            paymentCheckoutSessionRepository: paymentCheckoutSessionRepository);

        var service = CreatePaymentService(unitOfWork, loggingService: loggingService);
        var request = CreateServiceRequest();

        // Act
        await service.ProcessWebhook(request, webhook);

        // Assert
        Mock.Get(loggingService)
            .Verify(x => x.Warn(It.Is<string>(s => s.Contains("already expired"))), Times.Once);
    }

    private static IPaymentProviderWebhookEventRepository CreateMockPaymentProviderWebhookEventRepository(
        IEnumerable<PaymentProviderWebhookEvent>? events = null)
    {
        var mock = new Mock<IPaymentProviderWebhookEventRepository>();
        events ??= [];

        mock.Setup(x => x.GetByExternalId(It.IsAny<PaymentProviderType>(), It.IsAny<string>()))
            .Returns((PaymentProviderType type, string externalId) =>
                new MockDeferredQuerySingleOrDefault<PaymentProviderWebhookEvent>(
                    events.FirstOrDefault(x => x.ExternalId == externalId && x.PaymentProviderType == type)));

        mock.Setup(x => x.Add(It.IsAny<PaymentProviderWebhookEvent>()));

        return mock.Object;
    }

    private static IMemberRepository CreateMockMemberRepository(Member? member = null)
    {
        var mock = new Mock<IMemberRepository>();

        mock.Setup(x => x.GetById(It.IsAny<Guid>()))
            .Returns((Guid memberId) =>
                new MockDeferredQuerySingle<Member>(
                    member ?? CreateMember(id: memberId)));

        return mock.Object;
    }

    private static IMemberSubscriptionRecordRepository CreateMockMemberSubscriptionRecordRepository(
        IEnumerable<MemberSubscriptionRecord>? subscriptionRecords = null)
    {
        var mock = new Mock<IMemberSubscriptionRecordRepository>();
        mock.Setup(x => x.GetByExternalId(It.IsAny<string>()))
            .Returns((string externalId) =>
                new MockDeferredQuerySingle<MemberSubscriptionRecord>(
                    subscriptionRecords?.FirstOrDefault(x => x.ExternalId == externalId)));
        return mock.Object;
    }

    private static IPaymentRepository CreateMockPaymentRepository(IEnumerable<Payment>? payments = null)
    {
        var mock = new Mock<IPaymentRepository>();

        mock.Setup(x => x.GetById(It.IsAny<Guid>()))
            .Returns((Guid id) => new MockDeferredQuerySingle<Payment>(payments?.FirstOrDefault(x => x.Id == id)));

        return mock.Object;
    }

    private static IPaymentCheckoutSessionRepository CreateMockPaymentCheckoutSessionRepository(
        PaymentCheckoutSession? session = null)
    {
        var mock = new Mock<IPaymentCheckoutSessionRepository>();

        mock.Setup(x => x.GetById(It.IsAny<Guid>()))
            .Returns((Guid sessionId) =>
                new MockDeferredQuerySingle<PaymentCheckoutSession>(
                    session ?? CreatePaymentCheckoutSession(id: sessionId)));

        mock.Setup(x => x.GetByIdOrDefault(It.IsAny<Guid>()))
            .Returns((Guid sessionId) =>
                new MockDeferredQuerySingleOrDefault<PaymentCheckoutSession>(session));

        mock.Setup(x => x.Update(It.IsAny<PaymentCheckoutSession>()));

        return mock.Object;
    }

    private static IEventTicketPaymentRepository CreateMockEventTicketPaymentRepository(
        EventTicketPayment? eventTicketPayment = null)
    {
        var mock = new Mock<IEventTicketPaymentRepository>();

        mock.Setup(x => x.GetById(It.IsAny<Guid>()))
            .Returns((Guid id) =>
                new MockDeferredQuerySingle<EventTicketPayment>(
                    eventTicketPayment ?? CreateEventTicketPayment()));

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
                It.IsAny<SiteEmailSettings>()))
            .Returns(Task.CompletedTask);
        return mock.Object;
    }

    private static MockUnitOfWork CreateMockUnitOfWork(
        IPaymentProviderWebhookEventRepository? paymentProviderWebhookEventRepository = null,
        IMemberRepository? memberRepository = null,
        IPaymentRepository? paymentRepository = null,
        IPaymentCheckoutSessionRepository? paymentCheckoutSessionRepository = null,
        IEventTicketPaymentRepository? eventTicketPaymentRepository = null,
        IMemberSubscriptionRecordRepository? memberSubscriptionRecordRepository = null)
    {
        var mock = new Mock<IUnitOfWork>();

        mock.Setup(x => x.MemberRepository)
            .Returns(memberRepository ?? CreateMockMemberRepository());
        mock.Setup(x => x.MemberSubscriptionRecordRepository)
            .Returns(memberSubscriptionRecordRepository ?? CreateMockMemberSubscriptionRecordRepository());
        mock.Setup(x => x.PaymentProviderWebhookEventRepository)
            .Returns(paymentProviderWebhookEventRepository ?? CreateMockPaymentProviderWebhookEventRepository());
        mock.Setup(x => x.PaymentRepository)
            .Returns(paymentRepository ?? CreateMockPaymentRepository());
        mock.Setup(x => x.PaymentCheckoutSessionRepository)
            .Returns(paymentCheckoutSessionRepository ?? CreateMockPaymentCheckoutSessionRepository());
        mock.Setup(x => x.EventTicketPaymentRepository)
            .Returns(eventTicketPaymentRepository ?? CreateMockEventTicketPaymentRepository(null));
        mock.Setup(x => x.SiteEmailSettingsRepository.Get(It.IsAny<PlatformType>()))
            .Returns(new MockDeferredQuerySingle<SiteEmailSettings>(new SiteEmailSettings()));
        mock.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);

        return new MockUnitOfWork(mock);
    }

    private static PaymentService CreatePaymentService(
        IUnitOfWork unitOfWork,
        ILoggingService? loggingService = null,
        IMemberEmailService? memberEmailService = null,
        IPaymentProviderFactory? paymentProviderFactory = null,
        IEventService? eventService = null,
        IBackgroundTaskService? backgroundTaskService = null)
    {
        return new PaymentService(
            unitOfWork,
            loggingService ?? CreateMockLoggingService(),
            memberEmailService ?? CreateMockMemberEmailService(),
            paymentProviderFactory ?? new Mock<IPaymentProviderFactory>().Object,
            eventService ?? new Mock<IEventService>().Object,
            backgroundTaskService ?? new Mock<IBackgroundTaskService>().Object);
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
        IDictionary<string, string>? metadata = null,
        decimal? amount = null)
        => new PaymentProviderWebhook
        {
            Id = id ?? "wh_123",
            Type = type ?? PaymentProviderWebhookType.PaymentSucceeded,
            Complete = complete,
            PaymentId = paymentId ?? "pi_123",
            SubscriptionId = subscriptionId,
            Metadata = metadata ?? new Dictionary<string, string>(),
            Amount = amount ?? 100m,
            OriginatedUtc = DateTime.UtcNow,
            PaymentProviderType = PaymentProviderType.Stripe
        };

    private static Payment CreatePayment(
        Guid? id = null,
        DateTime? paidUtc = null)
        => new Payment
        {
            Id = id ?? Guid.NewGuid(),
            Amount = 100m,
            CreatedUtc = DateTime.UtcNow,
            CurrencyId = Guid.NewGuid(),
            MemberId = Guid.NewGuid(),
            PaidUtc = paidUtc,
            Reference = "REF123",
            SitePaymentSettingId = Guid.NewGuid()
        };

    private static PaymentCheckoutSession CreatePaymentCheckoutSession(
        Guid? id = null,
        DateTime? completedUtc = null,
        DateTime? expiredUtc = null)
        => new PaymentCheckoutSession
        {
            Id = id ?? Guid.NewGuid(),
            MemberId = Guid.NewGuid(),
            PaymentId = Guid.NewGuid(),
            CompletedUtc = completedUtc,
            ExpiredUtc = expiredUtc
        };

    private static Member CreateMember(Guid? id = null)
        => new Member
        {
            Id = id ?? Guid.NewGuid(),
            SiteAdmin = false
        };

    private static EventTicketPayment CreateEventTicketPayment(
        Guid? id = null,
        Guid? eventId = null)
        => new EventTicketPayment
        {
            Id = id ?? Guid.NewGuid(),
            EventId = eventId ?? Guid.NewGuid(),
            PaymentId = Guid.NewGuid()
        };

    private static Chapter CreateChapter(Guid? id = null)
        => new Chapter
        {
            Id = id ?? Guid.NewGuid(),
            Name = "Test Chapter",
            OwnerId = Guid.NewGuid(),
            CreatedUtc = DateTime.UtcNow,
            CountryId = Guid.NewGuid(),
            Platform = PlatformType.Default,
            Slug = "test-chapter"
        };

    private static Currency CreateCurrency(Guid? id = null)
        => new Currency
        {
            Id = id ?? Guid.NewGuid(),
            Code = "GBP"
        };

    private static MemberSubscriptionRecord CreateMemberSubscriptionRecord(
        Guid? id = null,
        Guid? memberId = null,
        string? externalId = null)
        => new MemberSubscriptionRecord
        {
            Id = id ?? Guid.NewGuid(),
            MemberId = memberId ?? Guid.NewGuid(),
            ChapterSubscriptionId = Guid.NewGuid(),
            ExternalId = externalId ?? "sub_123"
        };
}