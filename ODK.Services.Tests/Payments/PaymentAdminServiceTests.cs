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
using ODK.Services.Payments.Models;
using ODK.Services.Payments.ViewModels;
using ODK.Services.Security;
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

        context.Set<PaymentReconciliation>()
            .Count(x => x.IgnoredUtc != null)
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
        context.Set<PaymentReconciliation>()
            .Where(x => x.PaymentId == payment.Id && x.IgnoredUtc != null)
            .Should().BeEmpty();
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
        CreateTransfer(context, payment, completed: true);

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
    public static async Task GetPaymentReconciliationViewModel_PendingRefund_IsListedAsARefundOutcome()
    {
        // Arrange - settled and transferred, and still waiting on what became of a refund
        using var context = CreateMockOdkContext();

        var chapter = context.CreateChapter(name: "Group one");

        var payment = CreateSettledPayment(context, chapter);
        CreateRefund(context, payment, 40m, PaymentRefundStatusType.Pending);

        var service = CreatePaymentAdminService(context);

        // Act
        var result = await service.GetPaymentReconciliationViewModel(SiteAdminRequest(context));

        // Assert
        result.Payments.Single().Pending.Should().Be(PaymentReconciliationType.RefundRecord);
    }

    [Test]
    public static async Task GetPaymentReconciliationViewModel_ConfirmedRefund_IsNotListed()
    {
        // Arrange - the provider has said what became of it, so there is nothing left to ask
        using var context = CreateMockOdkContext();

        var chapter = context.CreateChapter(name: "Group one");

        var payment = CreateSettledPayment(context, chapter);
        CreateRefund(context, payment, 40m, PaymentRefundStatusType.Refunded);

        var service = CreatePaymentAdminService(context);

        // Act
        var result = await service.GetPaymentReconciliationViewModel(SiteAdminRequest(context));

        // Assert
        result.Payments.Should().BeEmpty();
    }

    [Test]
    public static async Task ReconcilePayment_PendingRefund_RunsNow()
    {
        // Arrange - the row action has to reach every kind the page lists
        using var context = CreateMockOdkContext();

        var payment = CreateSettledPayment(context, context.CreateChapter());
        CreateRefund(context, payment, 40m, PaymentRefundStatusType.Pending);

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
    public static async Task GetPaymentReconciliationViewModel_FullyReconciledPayment_IsNotListed()
    {
        // Arrange
        using var context = CreateMockOdkContext();

        var chapter = context.CreateChapter(name: "Group one");

        var payment = CreateUnsettledPayment(context, chapter);
        payment.ActualAmount = 100m;
        CreateTransfer(context, payment, completed: true, externalId: "tr_123");

        var service = CreatePaymentAdminService(context);

        // Act
        var result = await service.GetPaymentReconciliationViewModel(SiteAdminRequest(context));

        // Assert
        result.Payments.Should().BeEmpty();
    }

    [Test]
    public static async Task GetPayments_UnrefundedPayment_OffersARefund()
    {
        // Arrange
        using var context = CreateMockOdkContext();

        var chapter = context.CreateChapter();
        CreateSettledPayment(context, chapter);

        var request = ChapterAdminRequest(context, chapter);
        var service = CreatePaymentAdminService(context);

        // Act
        var result = await service.GetPayments(request);

        // Assert
        var item = result.Payments.Single();
        item.HasRefund.Should().BeFalse();
        item.RefundedAmount.Should().BeNull();
    }

    [Test]
    public static async Task GetPayments_RefundedPayment_ShowsWhatWasGivenBack()
    {
        // Arrange
        using var context = CreateMockOdkContext();

        var chapter = context.CreateChapter();
        var payment = CreateSettledPayment(context, chapter);

        CreateRefund(context, payment, 40m, PaymentRefundStatusType.Refunded);

        var request = ChapterAdminRequest(context, chapter);
        var service = CreatePaymentAdminService(context);

        // Act
        var result = await service.GetPayments(request);

        // Assert
        var item = result.Payments.Single();
        item.HasRefund.Should().BeTrue();
        item.RefundedAmount.Should().Be(40m);
    }

    [Test]
    public static async Task GetPayments_RefundNotYetPaid_HasARefundWithNoAmount()
    {
        // Arrange
        using var context = CreateMockOdkContext();

        var chapter = context.CreateChapter();
        var payment = CreateSettledPayment(context, chapter);

        CreateRefund(context, payment, 40m, PaymentRefundStatusType.Pending);

        var request = ChapterAdminRequest(context, chapter);
        var service = CreatePaymentAdminService(context);

        // Act
        var result = await service.GetPayments(request);

        // Assert
        var item = result.Payments.Single();
        item.HasRefund.Should().BeTrue();
        item.RefundedAmount.Should().BeNull();
    }

    [Test]
    public static async Task GetPayments_FailedRefund_IsNotARefundThePaymentHas()
    {
        // Arrange
        using var context = CreateMockOdkContext();

        var chapter = context.CreateChapter();
        var payment = CreateSettledPayment(context, chapter);

        /* A failed refund carries a refunded date and returned the money to us, so it must not read as
           either a refund the payment has or an amount given back. */
        CreateRefund(context, payment, 40m, PaymentRefundStatusType.Failed);

        var request = ChapterAdminRequest(context, chapter);
        var service = CreatePaymentAdminService(context);

        // Act
        var result = await service.GetPayments(request);

        // Assert
        var item = result.Payments.Single();
        item.HasRefund.Should().BeFalse();
        item.RefundedAmount.Should().BeNull();
    }

    [Test]
    public static async Task GetPayments_PartiallyRefundedTwice_AddsUpWhatWasGivenBack()
    {
        // Arrange
        using var context = CreateMockOdkContext();

        var chapter = context.CreateChapter();
        var payment = CreateSettledPayment(context, chapter);

        CreateRefund(context, payment, 30m, PaymentRefundStatusType.Refunded);
        CreateRefund(context, payment, 25m, PaymentRefundStatusType.Refunded);

        var request = ChapterAdminRequest(context, chapter);
        var service = CreatePaymentAdminService(context);

        // Act
        var result = await service.GetPayments(request);

        // Assert
        result.Payments.Single().RefundedAmount.Should().Be(55m);
    }

    [Test]
    public static async Task GetPayments_SettledPayment_OffersTheWholeOfItAsRefundable()
    {
        // Arrange
        using var context = CreateMockOdkContext();

        var chapter = context.CreateChapter();
        CreateSettledPayment(context, chapter);

        var request = ChapterAdminRequest(context, chapter);
        var service = CreatePaymentAdminService(context);

        // Act
        var result = await service.GetPayments(request);

        // Assert
        result.Payments.Single().RefundableAmount.Should().Be(100m);
    }

    [Test]
    public static async Task GetPayments_PartlyRefundedPayment_OffersOnlyWhatIsLeft()
    {
        /* Arrange - measured against every live refund, including one the provider has not confirmed: it
           has claimed its amount either way. */
        using var context = CreateMockOdkContext();

        var chapter = context.CreateChapter();
        var payment = CreateSettledPayment(context, chapter);

        CreateRefund(context, payment, 40m, PaymentRefundStatusType.Pending);

        var request = ChapterAdminRequest(context, chapter);
        var service = CreatePaymentAdminService(context);

        // Act
        var result = await service.GetPayments(request);

        // Assert
        result.Payments.Single().RefundableAmount.Should().Be(60m);
    }

    [Test]
    public static async Task GetPayments_UnsettledPayment_OffersNothingAsRefundable()
    {
        // Arrange - nothing says what the charge took, and no charge is named to refund against
        using var context = CreateMockOdkContext();

        var chapter = context.CreateChapter();
        CreateUnsettledPayment(context, chapter);

        var request = ChapterAdminRequest(context, chapter);
        var service = CreatePaymentAdminService(context);

        // Act
        var result = await service.GetPayments(request);

        // Assert
        result.Payments.Single().RefundableAmount.Should().BeNull();
    }

    [Test]
    public static async Task GetPaymentRefundsViewModel_ListsWhatEachRefundLeftOwing()
    {
        /* Arrange - a refund of the whole payment whose reversal could only reach what the group was
           actually sent, leaving the rest on the group's ledger. */
        using var context = CreateMockOdkContext();

        var chapter = context.CreateChapter(name: "Group one");
        var payment = CreateSettledPayment(context, chapter);
        var transfer = CreateTransfer(context, payment, completed: true, externalId: "tr_456");

        var refund = CreateRefund(context, payment, 100m, PaymentRefundStatusType.Refunded);
        CreateReversal(context, transfer, refund, 88m);
        CreateRefundShortfall(context, chapter, payment, refund, 12m);

        var service = CreatePaymentAdminService(context);

        // Act
        var result = await service.GetPaymentRefundsViewModel(SiteAdminRequest(context));

        // Assert
        var item = result.Refunds.Single();
        item.ChapterName.Should().Be("Group one");
        item.OutstandingAmount.Should().Be(12m);
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
        CreateTransfer(context, payment, completed: true);

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
        CreateTransfer(context, payment, completed: true, externalId: "tr_123");

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
        context.Set<PaymentReconciliation>()
            .Where(x => x.PaymentId == payment.Id && x.IgnoredUtc != null)
            .Should().BeEmpty();
    }

    private static IMemberChapterAdminServiceRequest ChapterAdminRequest(
        MockOdkContext context, Chapter chapter)
    {
        var admin = context.CreateMember();
        context.CreateChapterAdminMember(chapter, admin);

        return Mock.Of<IMemberChapterAdminServiceRequest>(x =>
            x.Chapter == chapter &&
            x.CurrentMember == admin &&
            x.Environment == EnvironmentType.Dev &&
            x.Platform == PlatformType.Default &&
            x.Securable == ChapterAdminSecurable.Any);
    }

    private static PaymentRefund CreateRefund(
        MockOdkContext context, Payment payment, decimal amount, PaymentRefundStatusType status)
    {
        var utcNow = DateTime.UtcNow;

        return context.Create(new PaymentRefund
        {
            ActualAmount = amount,
            Amount = amount,
            ChapterAmount = payment.ChapterId != null ? amount : null,
            Id = Guid.NewGuid(),
            PaymentId = payment.Id,
            Reason = "Event cancelled",
            RefundedUtc = utcNow,
            RequestedByMemberId = payment.MemberId,
            RequestedUtc = utcNow,
            Status = status
        });
    }

    /* What a reversal could not take back, carried on the group's ledger. Keyed to the refund, which is
       how the page finds what each one left owing. */
    private static ChapterPaymentAdjustment CreateRefundShortfall(
        MockOdkContext context, Chapter chapter, Payment payment, PaymentRefund refund, decimal amount)
        => context.Create(new ChapterPaymentAdjustment
        {
            Amount = -amount,
            ChapterId = chapter.Id,
            CreatedUtc = DateTime.UtcNow,
            CurrencyId = payment.CurrencyId,
            Description = $"Refund of payment {payment.Reference}",
            Id = Guid.NewGuid(),
            PaymentRefundId = refund.Id,
            RecoveredAmount = 0m,
            Type = ChapterPaymentAdjustmentType.RefundShortfall
        });

    private static PaymentTransferReversal CreateReversal(
        MockOdkContext context, PaymentTransfer transfer, PaymentRefund refund, decimal amount)
        => context.Create(new PaymentTransferReversal
        {
            ActualAmount = amount,
            Amount = amount,
            CompletedUtc = DateTime.UtcNow,
            CreatedUtc = DateTime.UtcNow,
            ExternalId = "trr_123",
            Id = Guid.NewGuid(),
            PaymentRefundId = refund.Id,
            PaymentTransferId = transfer.Id
        });

    private static PaymentTransfer CreateTransfer(
        MockOdkContext context, Payment payment, bool completed, string? externalId = null)
        => context.Create(new PaymentTransfer
        {
            Amount = 88m,
            CommissionAmount = 10m,
            CompletedUtc = completed ? DateTime.UtcNow.AddMonths(-3) : null,
            CreatedUtc = DateTime.UtcNow.AddMonths(-3),
            ExternalId = externalId,
            Id = Guid.NewGuid(),
            PaymentId = payment.Id
        });

    /* A payment whose settlement has been read, which is what says what the charge and the group's share
       actually were - and so what a refund can be checked against. */
    private static Payment CreateSettledPayment(MockOdkContext context, Chapter? chapter)
    {
        var payment = CreateUnsettledPayment(context, chapter);

        payment.ActualAmount = 100m;
        payment.ActualNetAmount = 98m;
        payment.ExternalChargeId = $"ch_{payment.Id:N}";
        payment.SettlementCurrencyCode = "GBP";

        if (chapter != null)
        {
            CreateTransfer(context, payment, completed: true, externalId: "tr_123");
        }

        return payment;
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
