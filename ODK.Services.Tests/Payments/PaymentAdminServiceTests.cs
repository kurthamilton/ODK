using System;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using ODK.Core.Chapters;
using ODK.Core.Payments;
using ODK.Core.Platforms;
using ODK.Data.Core;
using ODK.Services.Payments;
using ODK.Services.Payments.ViewModels;
using ODK.Services.Tests.Helpers;

namespace ODK.Services.Tests.Payments;

[Parallelizable]
public static class PaymentAdminServiceTests
{
    [Test]
    public static async Task IgnorePayment_RemovesItFromThePendingTables()
    {
        // Arrange
        using var context = CreateMockOdkContext();

        var payment = CreateUnsettledPayment(context, context.CreateChapter());

        var paymentService = new Mock<IPaymentService>();
        var service = CreatePaymentAdminService(context, paymentService.Object);

        // Act
        var result = await service.IgnorePayment(SiteAdminRequest(context), payment.Id);

        // Assert
        result.Success.Should().BeTrue();

        var viewModel = await service.GetPaymentReconciliationViewModel(SiteAdminRequest(context));
        viewModel.Payments.Should().BeEmpty();
        viewModel.Ignored.Select(x => x.Payment.Id).Should().Equal(payment.Id);

        // And the bulk action stops queueing it, or ignoring would only hide the retries
        await service.ReconcilePayments(SiteAdminRequest(context), [payment.Id]);
        paymentService.Verify(x => x.EnqueueResolvePaymentSettlementJob(It.IsAny<Guid>()), Times.Never);
    }

    [Test]
    public static async Task IgnorePayments_PaymentsSharingACurrency_AreAllIgnored()
    {
        /* Arrange - reading without tracking builds a separate currency instance per payment, so a write
           that carried them would attach one currency twice. */
        using var context = CreateMockOdkContext();

        var currency = context.CreateCurrency();
        var chapter = context.CreateChapter(country: context.CreateCountry(currency));

        var first = context.CreatePayment(
            chapter: chapter, currency: currency, paidUtc: DateTime.UtcNow.AddDays(-2));
        var second = context.CreatePayment(
            chapter: chapter, currency: currency, paidUtc: DateTime.UtcNow.AddDays(-1));

        var service = CreatePaymentAdminService(context);

        // Act
        var result = await service.IgnorePayments(SiteAdminRequest(context), [first.Id, second.Id]);

        // Assert
        result.Success.Should().BeTrue();

        context.Set<Payment>()
            .Count(x => x.ReconciliationIgnoredUtc != null)
            .Should().Be(2);
    }

    [Test]
    public static async Task IgnorePayment_IgnoredPayment_CannotBeReconciledByRow()
    {
        // Arrange
        using var context = CreateMockOdkContext();

        var payment = CreateUnsettledPayment(context, context.CreateChapter());

        var paymentService = new Mock<IPaymentService>();
        var service = CreatePaymentAdminService(context, paymentService.Object);

        await service.IgnorePayment(SiteAdminRequest(context), payment.Id);

        // Act
        var result = await service.ReconcilePayment(SiteAdminRequest(context), payment.Id);

        // Assert
        result.Success.Should().BeFalse();
        paymentService.Verify(x => x.ResolvePaymentSettlement(It.IsAny<Guid>()), Times.Never);
    }

    [Test]
    public static async Task UnignorePayment_PutsItBack()
    {
        /* Arrange - an instruction to ignore that could not be undone the same way it was made would be a
           trap, so the round trip is what this holds. */
        using var context = CreateMockOdkContext();

        var payment = CreateUnsettledPayment(context, context.CreateChapter());

        var service = CreatePaymentAdminService(context);

        await service.IgnorePayment(SiteAdminRequest(context), payment.Id);

        /* The two happen in separate requests, so the second starts with nothing tracked. Without this the
           test writes the same payment twice through one context, which the app never does. */
        context.ChangeTracker.Clear();

        // Act
        var result = await service.UnignorePayment(SiteAdminRequest(context), payment.Id);

        // Assert
        result.Success.Should().BeTrue();

        var viewModel = await service.GetPaymentReconciliationViewModel(SiteAdminRequest(context));
        viewModel.Ignored.Should().BeEmpty();
        viewModel.Payments.Select(x => x.Payment.Id).Should().Equal(payment.Id);
    }

    [Test]
    public static async Task IgnorePayment_PaymentOnAnotherPlatform_Fails()
    {
        // Arrange
        using var context = CreateMockOdkContext();

        var chapter = context.CreateChapter(platform: PlatformType.DrunkenKnitwits);
        var payment = CreateUnsettledPayment(context, chapter);

        var service = CreatePaymentAdminService(context);

        // Act
        var result = await service.IgnorePayment(SiteAdminRequest(context), payment.Id);

        // Assert
        result.Success.Should().BeFalse();
        context.Set<Payment>().Single(x => x.Id == payment.Id)
            .ReconciliationIgnoredUtc.Should().BeNull();
    }

    [Test]
    public static async Task GetPaymentReconciliationViewModel_NamesTheGroupThePaymentWasFor()
    {
        // Arrange
        using var context = CreateMockOdkContext();

        var chapter = context.CreateChapter(name: "Group one");

        CreateUnsettledPayment(context, chapter);

        var service = CreatePaymentAdminService(context);

        // Act
        var result = await service.GetPaymentReconciliationViewModel(SiteAdminRequest(context));

        // Assert
        result.Payments.Single().ChapterName.Should().Be("Group one");
    }

    [Test]
    public static async Task GetPaymentReconciliationViewModel_SitePayment_NamesNoGroup()
    {
        // Arrange - a site payment belongs to no group, which is not a gap in the data
        using var context = CreateMockOdkContext();

        CreateUnsettledPayment(context, chapter: null);

        var service = CreatePaymentAdminService(context);

        // Act
        var result = await service.GetPaymentReconciliationViewModel(SiteAdminRequest(context));

        // Assert
        result.Payments.Single().ChapterName.Should().BeNull();
    }

    [Test]
    public static async Task GetPaymentReconciliationViewModel_PaymentOnAnotherPlatform_IsNotListed()
    {
        /* Arrange - a payment carries the platform of the group it was taken for, wherever the member
           paid, so the other platform's payments are reconciled from the other platform's site. */
        using var context = CreateMockOdkContext();

        var chapter = context.CreateChapter(name: "Group one");
        var drunkenKnitwitsChapter = context.CreateChapter(
            name: "Group two", platform: PlatformType.DrunkenKnitwits);

        var payment = CreateUnsettledPayment(context, chapter);
        CreateUnsettledPayment(context, drunkenKnitwitsChapter);

        var service = CreatePaymentAdminService(context);

        // Act
        var result = await service.GetPaymentReconciliationViewModel(SiteAdminRequest(context));

        // Assert
        result.Payments.Select(x => x.Payment.Id).Should().Equal(payment.Id);
    }

    [Test]
    public static async Task GetPaymentReconciliationViewModel_PaymentInAnotherEnvironment_IsNotListed()
    {
        // Arrange - a payment taken in another deployment is read through another provider account
        using var context = CreateMockOdkContext();

        var chapter = context.CreateChapter(name: "Group one");

        var payment = CreateUnsettledPayment(context, chapter);

        var otherEnvironment = CreateUnsettledPayment(context, chapter);
        otherEnvironment.Environment = EnvironmentType.Prod;

        var service = CreatePaymentAdminService(context);

        // Act
        var result = await service.GetPaymentReconciliationViewModel(SiteAdminRequest(context));

        // Assert
        result.Payments.Select(x => x.Payment.Id).Should().Equal(payment.Id);
    }

    [Test]
    public static async Task GetPaymentReconciliationViewModel_TransferredWithNoTransferRecorded_IsListed()
    {
        // Arrange - the group has its share but nothing names the transfer a refund would reverse
        using var context = CreateMockOdkContext();

        var chapter = context.CreateChapter(name: "Group one");

        var payment = CreateUnsettledPayment(context, chapter);
        payment.ActualAmount = 100m;
        payment.TransferredUtc = DateTime.UtcNow.AddMonths(-3);

        var service = CreatePaymentAdminService(context);

        // Act
        var result = await service.GetPaymentReconciliationViewModel(SiteAdminRequest(context));

        // Assert
        /* Assert - and it moves no money, however it is named: the share already reached the group, so
           TransferConnectedAccountShare returns early and only the id is written. */
        var item = result.Payments.Single();
        item.Payment.Id.Should().Be(payment.Id);
        item.Pending.Should().Be(PaymentReconciliationType.TransferRecord);
    }

    [Test]
    public static async Task GetPaymentReconciliationViewModel_GroupWithAPaymentAccount_SendsMoney()
    {
        // Arrange - reading this settlement goes on to transfer the group its share
        using var context = CreateMockOdkContext();

        var chapter = context.CreateChapter(name: "Group one");
        context.CreateChapterPaymentAccount(chapter, externalId: "acct_123");

        CreateUnsettledPayment(context, chapter);

        var service = CreatePaymentAdminService(context);

        // Act
        var result = await service.GetPaymentReconciliationViewModel(SiteAdminRequest(context));

        // Assert
        result.Payments.Single().Pending
            .Should().Be(PaymentReconciliationType.SettlementAndTransfer);
    }

    [Test]
    public static async Task GetPaymentReconciliationViewModel_GroupWithNoPaymentAccount_SendsNothing()
    {
        /* Arrange - a connected account to pay is what decides whether money moves, not the payment
           belonging to a group: a group that never finished setting payments up has nothing to be paid
           through. */
        using var context = CreateMockOdkContext();

        var chapter = context.CreateChapter(name: "Group one");

        CreateUnsettledPayment(context, chapter);

        var service = CreatePaymentAdminService(context);

        // Act
        var result = await service.GetPaymentReconciliationViewModel(SiteAdminRequest(context));

        // Assert
        result.Payments.Single().Pending.Should().Be(PaymentReconciliationType.Settlement);
    }

    [Test]
    public static async Task GetPaymentReconciliationViewModel_SitePayment_SendsNothing()
    {
        // Arrange - no group, so nothing to send to
        using var context = CreateMockOdkContext();

        CreateUnsettledPayment(context, chapter: null);

        var service = CreatePaymentAdminService(context);

        // Act
        var result = await service.GetPaymentReconciliationViewModel(SiteAdminRequest(context));

        // Assert
        result.Payments.Single().Pending.Should().Be(PaymentReconciliationType.Settlement);
    }

    [Test]
    public static async Task GetPaymentReconciliationViewModel_FullyReconciledPayment_IsNotListed()
    {
        // Arrange
        using var context = CreateMockOdkContext();

        var chapter = context.CreateChapter(name: "Group one");

        var payment = CreateUnsettledPayment(context, chapter);
        payment.ActualAmount = 100m;
        payment.ExternalTransferId = "tr_123";
        payment.TransferredUtc = DateTime.UtcNow.AddMonths(-3);

        var service = CreatePaymentAdminService(context);

        // Act
        var result = await service.GetPaymentReconciliationViewModel(SiteAdminRequest(context));

        // Assert
        result.Payments.Should().BeEmpty();
    }

    [Test]
    public static async Task ReconcilePayment_PendingPayment_RunsNowAndReportsTheOutcome()
    {
        /* Arrange - the row action runs the reconcile rather than queueing it, so the site admin is told
           what happened rather than that something will happen. */
        using var context = CreateMockOdkContext();

        var payment = CreateUnsettledPayment(context, context.CreateChapter());

        var paymentService = new Mock<IPaymentService>();
        paymentService
            .Setup(x => x.ResolvePaymentSettlement(payment.Id))
            .ReturnsAsync(ResolvePaymentSettlementResult.Resolved(transferred: false));

        var service = CreatePaymentAdminService(context, paymentService.Object);

        // Act
        var result = await service.ReconcilePayment(SiteAdminRequest(context), payment.Id);

        // Assert
        result.Success.Should().BeTrue();
        paymentService.Verify(x => x.ResolvePaymentSettlement(payment.Id), Times.Once);
        paymentService.Verify(x => x.EnqueueResolvePaymentSettlementJob(It.IsAny<Guid>()), Times.Never);
    }

    [Test]
    public static async Task ReconcilePayment_MoneyMoved_SaysSo()
    {
        // Arrange - what a site admin most needs telling apart from a read that only wrote to our records
        using var context = CreateMockOdkContext();

        var payment = CreateUnsettledPayment(context, context.CreateChapter());

        var paymentService = new Mock<IPaymentService>();
        paymentService
            .Setup(x => x.ResolvePaymentSettlement(payment.Id))
            .ReturnsAsync(ResolvePaymentSettlementResult.Resolved(transferred: true));

        var service = CreatePaymentAdminService(context, paymentService.Object);

        // Act
        var result = await service.ReconcilePayment(SiteAdminRequest(context), payment.Id);

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Contain("sent");
    }

    [Test]
    public static async Task ReconcilePayment_ProviderCouldNotAnswer_ReturnsTheReason()
    {
        // Arrange - the reason reaches the site admin, not just the payment row
        using var context = CreateMockOdkContext();

        var payment = CreateUnsettledPayment(context, context.CreateChapter());

        var paymentService = new Mock<IPaymentService>();
        paymentService
            .Setup(x => x.ResolvePaymentSettlement(payment.Id))
            .ReturnsAsync(ResolvePaymentSettlementResult.Failure("No configured account holds it"));

        var service = CreatePaymentAdminService(context, paymentService.Object);

        // Act
        var result = await service.ReconcilePayment(SiteAdminRequest(context), payment.Id);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("No configured account");
    }

    [Test]
    public static async Task ReconcilePayment_TransferredWithNoTransferRecorded_RunsNow()
    {
        // Arrange - pending for the other of the two reasons, which the row action has to reach as well
        using var context = CreateMockOdkContext();

        var payment = CreateUnsettledPayment(context, context.CreateChapter());
        payment.ActualAmount = 100m;
        payment.TransferredUtc = DateTime.UtcNow.AddMonths(-3);

        var paymentService = new Mock<IPaymentService>();
        paymentService
            .Setup(x => x.ResolvePaymentSettlement(payment.Id))
            .ReturnsAsync(ResolvePaymentSettlementResult.Resolved(transferred: false));

        var service = CreatePaymentAdminService(context, paymentService.Object);

        // Act
        var result = await service.ReconcilePayment(SiteAdminRequest(context), payment.Id);

        // Assert
        result.Success.Should().BeTrue();
        paymentService.Verify(x => x.ResolvePaymentSettlement(payment.Id), Times.Once);
    }

    [Test]
    public static async Task ReconcilePayment_FullyReconciledPayment_IsNotRun()
    {
        // Arrange
        using var context = CreateMockOdkContext();

        var payment = CreateUnsettledPayment(context, context.CreateChapter());
        payment.ActualAmount = 100m;
        payment.ExternalTransferId = "tr_123";
        payment.TransferredUtc = DateTime.UtcNow.AddMonths(-3);

        var paymentService = new Mock<IPaymentService>();
        var service = CreatePaymentAdminService(context, paymentService.Object);

        // Act
        var result = await service.ReconcilePayment(SiteAdminRequest(context), payment.Id);

        // Assert
        result.Success.Should().BeFalse();
        paymentService.Verify(x => x.ResolvePaymentSettlement(It.IsAny<Guid>()), Times.Never);
    }

    [Test]
    public static async Task ReconcilePayment_PaymentOnAnotherPlatform_IsNotRun()
    {
        /* Arrange - the row action is looked up through the same definition the page lists, so an id from
           another platform reaches nothing even when it is posted directly. */
        using var context = CreateMockOdkContext();

        var chapter = context.CreateChapter(platform: PlatformType.DrunkenKnitwits);
        var payment = CreateUnsettledPayment(context, chapter);

        var paymentService = new Mock<IPaymentService>();
        var service = CreatePaymentAdminService(context, paymentService.Object);

        // Act
        var result = await service.ReconcilePayment(SiteAdminRequest(context), payment.Id);

        // Assert
        result.Success.Should().BeFalse();
        paymentService.Verify(x => x.ResolvePaymentSettlement(It.IsAny<Guid>()), Times.Never);
    }

    [Test]
    public static async Task ReconcilePayments_QueuesOnlyTheIdsGiven()
    {
        /* Arrange - the ids come from the page, so a bulk action covers the rows the site admin was looking
           at and not whatever else is pending by the time the form arrives. */
        using var context = CreateMockOdkContext();

        var chapter = context.CreateChapter(name: "Group one");

        var pressed = CreateUnsettledPayment(context, chapter);
        var otherTable = CreateUnsettledPayment(context, chapter);

        var paymentService = new Mock<IPaymentService>();
        var service = CreatePaymentAdminService(context, paymentService.Object);

        // Act
        var result = await service.ReconcilePayments(SiteAdminRequest(context), [pressed.Id]);

        // Assert
        result.Success.Should().BeTrue();
        paymentService.Verify(x => x.EnqueueResolvePaymentSettlementJob(pressed.Id), Times.Once);
        paymentService.Verify(
            x => x.EnqueueResolvePaymentSettlementJob(otherTable.Id), Times.Never);
    }

    [Test]
    public static async Task ReconcilePayments_PaymentOnAnotherPlatform_IsNotQueued()
    {
        // Arrange - a posted id can no more reach past the page than a row action can
        using var context = CreateMockOdkContext();

        var chapter = context.CreateChapter(platform: PlatformType.DrunkenKnitwits);
        var payment = CreateUnsettledPayment(context, chapter);

        var paymentService = new Mock<IPaymentService>();
        var service = CreatePaymentAdminService(context, paymentService.Object);

        // Act
        var result = await service.ReconcilePayments(SiteAdminRequest(context), [payment.Id]);

        // Assert
        result.Success.Should().BeFalse();
        paymentService.Verify(x => x.EnqueueResolvePaymentSettlementJob(It.IsAny<Guid>()), Times.Never);
    }

    [Test]
    public static async Task ReconcilePayments_SomeIdsNoLongerOutstanding_SaysHowManyWereActedOn()
    {
        /* Arrange - the page had gone stale. Queueing fewer than were pressed is worth stating rather than
           reporting a number that looks like success. */
        using var context = CreateMockOdkContext();

        var chapter = context.CreateChapter();

        var pending = CreateUnsettledPayment(context, chapter);

        var settled = CreateUnsettledPayment(context, chapter);
        settled.ActualAmount = 100m;

        var paymentService = new Mock<IPaymentService>();
        var service = CreatePaymentAdminService(context, paymentService.Object);

        // Act
        var result = await service.ReconcilePayments(
            SiteAdminRequest(context), [pending.Id, settled.Id]);

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Contain("1 of 2");

        paymentService.Verify(x => x.EnqueueResolvePaymentSettlementJob(pending.Id), Times.Once);
        paymentService.Verify(x => x.EnqueueResolvePaymentSettlementJob(It.IsAny<Guid>()), Times.Once);
    }

    [Test]
    public static async Task ReconcilePayments_UnpaidPayment_IsNotQueued()
    {
        // Arrange - an abandoned checkout has nothing at the provider to read
        using var context = CreateMockOdkContext();

        var payment = context.CreatePayment(chapter: context.CreateChapter(), paidUtc: null);

        var paymentService = new Mock<IPaymentService>();
        var service = CreatePaymentAdminService(context, paymentService.Object);

        // Act
        var result = await service.ReconcilePayments(SiteAdminRequest(context), [payment.Id]);

        // Assert
        result.Success.Should().BeFalse();
        paymentService.Verify(x => x.EnqueueResolvePaymentSettlementJob(It.IsAny<Guid>()), Times.Never);
    }

    [Test]
    public static async Task IgnorePayments_IgnoresOnlyTheIdsGiven()
    {
        // Arrange
        using var context = CreateMockOdkContext();

        var chapter = context.CreateChapter();

        var pressed = CreateUnsettledPayment(context, chapter);
        var otherTable = CreateUnsettledPayment(context, chapter);

        var service = CreatePaymentAdminService(context);

        // Act
        var result = await service.IgnorePayments(
            SiteAdminRequest(context), [pressed.Id]);

        // Assert
        result.Success.Should().BeTrue();

        var viewModel = await service.GetPaymentReconciliationViewModel(SiteAdminRequest(context));
        viewModel.Ignored.Select(x => x.Payment.Id).Should().Equal(pressed.Id);
        viewModel.Payments.Select(x => x.Payment.Id).Should().Equal(otherTable.Id);
    }

    [Test]
    public static async Task IgnorePayments_PaymentOnAnotherPlatform_Fails()
    {
        // Arrange
        using var context = CreateMockOdkContext();

        var chapter = context.CreateChapter(platform: PlatformType.DrunkenKnitwits);
        var payment = CreateUnsettledPayment(context, chapter);

        var service = CreatePaymentAdminService(context);

        // Act
        var result = await service.IgnorePayments(
            SiteAdminRequest(context), [payment.Id]);

        // Assert
        result.Success.Should().BeFalse();
        context.Set<Payment>().Single(x => x.Id == payment.Id)
            .ReconciliationIgnoredUtc.Should().BeNull();
    }

    private static PaymentAdminService CreatePaymentAdminService(
        MockOdkContext context, IPaymentService? paymentService = null)
    {
        var unitOfWork = CreateMockUnitOfWork(context);

        /* Seeding leaves its entities in the change tracker, which a request never starts with - and a
           write that attaches a clone of one of them would collide with the instance still sitting there.
           Forgetting them is what makes the tests read and write the way the app does. */
        context.ChangeTracker.Clear();

        return new PaymentAdminService(unitOfWork, paymentService ?? Mock.Of<IPaymentService>());
    }

    /* Without tracking, as the app runs. It matters here rather than being a detail: a write submits a
       clone of the payment, and a tracking read would hand back an instance EF is already following, which
       the clone would then collide with. A tracking context would pass tests the app cannot. */
    private static MockOdkContext CreateMockOdkContext() => new(noTracking: true);

    private static IUnitOfWork CreateMockUnitOfWork(MockOdkContext? context = null)
        => MockUnitOfWorkFactory.Create(context);

    private static Payment CreateUnsettledPayment(MockOdkContext context, Chapter? chapter)
        => context.CreatePayment(chapter: chapter, paidUtc: DateTime.UtcNow.AddDays(-1));

    private static IMemberServiceRequest SiteAdminRequest(MockOdkContext context)
    {
        var siteAdmin = context.CreateMember(afterCreate: x => x.SiteAdmin = true);

        return Mock.Of<IMemberServiceRequest>(x =>
            x.Environment == EnvironmentType.Dev &&
            x.Platform == PlatformType.Default &&
            x.CurrentMember == siteAdmin);
    }
}
