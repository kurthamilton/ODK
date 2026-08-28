using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using ODK.Core.Payments;
using ODK.Services.Payments;
using ODK.Services.Tests.Helpers;

namespace ODK.Services.Tests.Payments;

[Parallelizable]
public static class PaymentAdminServiceTests
{
    [Test]
    public static async Task ReconcilePaymentSettlements_PaymentWithNoReference_IsCountedNotQueued()
    {
        /* Arrange - a payment taken before any provider reference was recorded. There is nothing to ask the
           provider about, so it is reported rather than queued for a job that could only fail. */
        using var context = new MockOdkContext();

        var payment = context.CreatePayment(paidUtc: DateTime.UtcNow);
        payment.ExternalId = null;

        var paymentService = new Mock<IPaymentService>();

        // Act
        var result = await CreateService(context, paymentService)
            .ReconcilePaymentSettlements(CreateSiteAdminRequest(context));

        // Assert
        result.Queued.Should().Be(0);
        result.Unidentifiable.Should().Be(1);

        paymentService.Verify(x => x.EnqueueResolvePaymentSettlementJob(It.IsAny<Guid>()), Times.Never);
    }

    [Test]
    public static async Task ReconcilePaymentSettlements_PaymentWithNoPaymentSettings_IsStillQueued()
    {
        /* Arrange - the account is not needed to queue the payment: the job finds which one holds its
           reference, and skips it if none does. */
        using var context = new MockOdkContext();

        var payment = context.CreatePayment(paidUtc: DateTime.UtcNow);
        payment.ExternalId = "sub_123";
        payment.SitePaymentSettingId = null;

        var paymentService = new Mock<IPaymentService>();

        // Act
        var result = await CreateService(context, paymentService)
            .ReconcilePaymentSettlements(CreateSiteAdminRequest(context));

        // Assert
        result.Queued.Should().Be(1);
        result.Unidentifiable.Should().Be(0);

        paymentService.Verify(x => x.EnqueueResolvePaymentSettlementJob(payment.Id), Times.Once);
    }

    [Test]
    public static async Task ReconcilePaymentSettlements_PaymentWithReference_IsQueued()
    {
        // Arrange
        using var context = new MockOdkContext();

        var payment = context.CreatePayment(paidUtc: DateTime.UtcNow);
        payment.ExternalId = "pi_123";

        var paymentService = new Mock<IPaymentService>();

        // Act
        var result = await CreateService(context, paymentService)
            .ReconcilePaymentSettlements(CreateSiteAdminRequest(context));

        // Assert
        result.Queued.Should().Be(1);
        result.Unidentifiable.Should().Be(0);

        paymentService.Verify(x => x.EnqueueResolvePaymentSettlementJob(payment.Id), Times.Once);
    }

    [Test]
    public static async Task ReconcilePaymentSettlements_PaymentAlreadySettled_IsLeftAlone()
    {
        // Arrange - running the reconcile again must not re-read what has already been read
        using var context = new MockOdkContext();

        var payment = context.CreatePayment(paidUtc: DateTime.UtcNow);
        payment.ExternalId = "pi_123";
        payment.ActualAmount = 100m;

        var paymentService = new Mock<IPaymentService>();

        // Act
        var result = await CreateService(context, paymentService)
            .ReconcilePaymentSettlements(CreateSiteAdminRequest(context));

        // Assert
        result.Queued.Should().Be(0);
        result.Unidentifiable.Should().Be(0);
    }

    [Test]
    public static async Task ReconcilePaymentSettlements_UnpaidPayment_IsLeftAlone()
    {
        // Arrange - a checkout that was never completed has nothing to settle
        using var context = new MockOdkContext();

        var payment = context.CreatePayment();
        payment.ExternalId = "pi_123";

        var paymentService = new Mock<IPaymentService>();

        // Act
        var result = await CreateService(context, paymentService)
            .ReconcilePaymentSettlements(CreateSiteAdminRequest(context));

        // Assert
        result.Queued.Should().Be(0);
        result.Unidentifiable.Should().Be(0);
    }

    private static PaymentAdminService CreateService(
        MockOdkContext context, Mock<IPaymentService> paymentService)
        => new PaymentAdminService(MockUnitOfWorkFactory.Create(context), paymentService.Object);

    private static IMemberServiceRequest CreateSiteAdminRequest(MockOdkContext context)
    {
        var member = context.CreateMember();
        member.SiteAdmin = true;

        var mock = new Mock<IMemberServiceRequest>();
        mock.Setup(x => x.CurrentMember).Returns(member);
        return mock.Object;
    }
}
