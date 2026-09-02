using FluentAssertions;
using NUnit.Framework;
using ODK.E2E.Data;
using ODK.E2E.Data.Models;
using ODK.E2E.Tests.Config;
using ODK.E2E.Tests.Helpers;
using ODK.E2E.Tests.Pages;
using Stripe;

namespace ODK.E2E.Tests;

/// <summary>
/// Chapter-member Stripe purchase: a member buys a non-recurring chapter subscription on the Default
/// platform. The chapter's Connect payment account is seeded with a REAL onboarded sandbox connected
/// account (<c>Stripe:ConnectedAccountId</c>), because the purchase transfers funds to it and Stripe rejects
/// an un-onboarded destination. The owner creates the subscription (real Stripe product/price) on a
/// throwaway browser; a fresh member joins the chapter (membership is required for completion) and pays via
/// embedded Checkout. Completion is webhook-only, so the ngrok tunnel must be up; the test polls until the
/// member's chapter subscription is recorded and active. This exercises the one-off
/// (<c>checkout.session.completed</c>) completion path, distinct from the recurring site-subscription test.
///
/// This is the only flow with a real onboarded connected account, so it is also where the transfer itself is
/// covered - everywhere else seeds <c>acct_e2e_fake</c>, where a transfer fails outright.
/// </summary>
[TestFixture]
[Category("Stripe")]
public class ChapterSubscriptionPurchaseTests : DefaultPageTest
{
    /// <summary>
    /// What the group is made to owe before it is paid, comfortably under the share of a £5 purchase (the
    /// platform keeps a percentage and the provider its fee, so the share is a little under £4.50). Smaller
    /// than the share on purpose: this covers a transfer being *reduced*, which is the case that proves both
    /// halves of the arithmetic - a debt larger than the share would send nothing and prove only one.
    /// </summary>
    private const decimal SeededDebt = 0.50m;

    private static ChapterPaymentAccountDataHelper ChapterPaymentAccounts => new(E2ESettings.ConnectionString);

    private static ChapterPaymentAdjustmentDataHelper ChapterPaymentAdjustments => new(E2ESettings.ConnectionString);

    private static ChapterSubscriptionDataHelper ChapterSubscriptions => new(E2ESettings.ConnectionString);

    private static MemberChapterSubscriptionDataHelper MemberChapterSubscriptions => new(E2ESettings.ConnectionString);

    private static MemberDataHelper Members => new(E2ESettings.ConnectionString);

    private static MemberSiteSubscriptionDataHelper MemberSubscriptions => new(E2ESettings.ConnectionString);

    private static PaymentDataHelper Payments => new(E2ESettings.ConnectionString);

    [Test]
    public async Task PurchaseChapterSubscription_CompletesViaWebhook_RecordsMemberSubscription()
    {
        // Arrange
        var (group, subscriptionId, _) = await ArrangePurchasableSubscription();

        // A fresh member joins the chapter (membership is required for the purchase to complete).
        var member = await Provisioning.JoinGroupAsMember(group);
        var memberId = await Members.GetMemberId(member.Email);
        await new LoginPage(Page).LogIn(member.Email, member.Password);

        // Act - pay via embedded Checkout; the checkout.session.completed webhook records the subscription.
        await new ChapterSubscriptionCheckoutPage(Page).PayWithTestCard(
            PlatformRoutes.Default(group).SubscriptionCheckout(subscriptionId));

        // Assert - the purchase is recorded for this member + subscription, and their chapter subscription is
        // active (webhook-driven, so poll).
        (await PollForPurchaseRecord(memberId, subscriptionId))
            .Should().BeTrue("the purchase webhook should record the member's chapter subscription");
        var expiryUtc = await MemberChapterSubscriptions.GetExpiryUtc(memberId, group.ChapterId);
        expiryUtc.Should().NotBeNull();

        // The subscription's DurationMonths is 1, so a single purchase should set expiry ~1 month out.
        // Asserting it's close to one month (not ~two) guards against a completion-idempotency regression
        // double-extending the subscription.
        expiryUtc!.Value.Should().BeCloseTo(DateTime.UtcNow.AddMonths(1), TimeSpan.FromDays(3));

        // A one-off (non-recurring) purchase must NOT persist an ExternalId - the payment-intent id is not a
        // subscription, so storing it only produces "no such subscription" noise on later Stripe lookups.
        (await MemberChapterSubscriptions.GetCurrentExternalId(memberId, group.ChapterId))
            .Should().BeNull("a non-recurring purchase should not store an external subscription id");
    }

    /// <summary>
    /// The collection loop: what a group owes is netted off its next transfer. A refund's reversal is what
    /// normally leaves a balance behind, and nothing else in the app collects one - so a purchase against a
    /// group carrying a debt has to send the share less the debt, settle the debt by exactly what it sent
    /// less, and say on the payment how much it kept back.
    /// </summary>
    [Test]
    public async Task PurchaseChapterSubscription_GroupOwesABalance_TransfersTheReducedAmountAndSettlesIt()
    {
        // Arrange
        var (group, subscriptionId, currencyId) = await ArrangePurchasableSubscription();

        /* Seeded before the purchase, because the netting happens when the transfer is made - which is on a
           scheduled job a few seconds after the purchase webhook, not on a later run. Negative: owed to the
           platform by the group. In the payment's own currency, since balances in different currencies are
           never netted against each other. */
        var adjustmentId = await ChapterPaymentAdjustments.Create(
            group.ChapterId, currencyId, -SeededDebt, "e2e seeded balance");

        var member = await Provisioning.JoinGroupAsMember(group);
        var memberId = await Members.GetMemberId(member.Email);
        await new LoginPage(Page).LogIn(member.Email, member.Password);

        // Act
        await new ChapterSubscriptionCheckoutPage(Page).PayWithTestCard(
            PlatformRoutes.Default(group).SubscriptionCheckout(subscriptionId));

        (await PollForPurchaseRecord(memberId, subscriptionId))
            .Should().BeTrue("the purchase webhook should record the member's chapter subscription");

        var transfer = await PollForTransfer(memberId, group.ChapterId);

        // Assert
        transfer.Should().NotBeNull("the settlement job should read the charge and transfer the group's share");
        transfer!.TransferredUtc.Should().NotBeNull();
        transfer.ConnectedAccountAmount.Should()
            .BeGreaterThan(SeededDebt, "the seeded debt has to be smaller than the share for this to be the reduced-transfer case");

        // The payment states what was kept back, which is the only account of a transfer smaller than the
        // share beside it.
        transfer.WithheldAmount.Should().Be(SeededDebt);

        // The debt is settled by exactly what was withheld, and carries the sign its amount does.
        (await ChapterPaymentAdjustments.GetRecoveredAmount(adjustmentId)).Should().Be(-SeededDebt);

        // Recorded against the payment whose transfer absorbed it - what answers "why was this one smaller".
        var paymentId = await Payments.GetId(memberId, group.ChapterId);
        paymentId.Should().NotBeNull();
        (await ChapterPaymentAdjustments.GetRecoveryAmount(adjustmentId, paymentId!.Value))
            .Should().Be(-SeededDebt);

        /* And the money actually moved, reduced - read from Stripe rather than inferred from the columns
           above, which are the app agreeing with itself. Amounts are in the currency's minor units; every
           currency these tests run in has two decimal places. */
        transfer.ExternalTransferId.Should().NotBeNull("a reduced transfer is still a transfer");
        var stripeTransfer = await new TransferService(
                new StripeClient(E2ESettings.StripeSecretApiKey(PlatformTypeId)))
            .GetAsync(transfer.ExternalTransferId);
        stripeTransfer.Amount.Should().Be(
            (long)Math.Round((transfer.ConnectedAccountAmount!.Value - SeededDebt) * 100m));
    }

    /// <summary>
    /// A published group whose owner can take money, with a non-recurring £5 chapter subscription on a real
    /// Stripe price. Returns the group, the subscription to check out, and the currency it is priced in.
    /// </summary>
    private async Task<(TestGroup Group, Guid SubscriptionId, Guid CurrencyId)> ArrangePurchasableSubscription()
    {
        // A purchase needs the webhook tunnel up and a real onboarded connected account.
        await StripeWebhookTunnel.EnsureReachable(E2ESettings.StripeWebhookBaseUrl);
        if (string.IsNullOrWhiteSpace(E2ESettings.StripeConnectedAccountId(PlatformTypeId)))
        {
            Assert.Fail(
                $"Set 'Stripe:Platforms:{PlatformTypeIds.Name(PlatformTypeId)}:ConnectedAccountId' to a " +
                "pre-onboarded Stripe sandbox connected account (acct_...), created under that platform's " +
                "own Stripe account. A chapter-subscription purchase transfers funds to it, and Stripe " +
                "rejects an un-onboarded destination.");
        }

        var siteSubscription = await Provisioning.EnsurePurchasableSiteSubscription();

        var owner = await Provisioning.NewAccount("chapter-subscription-owner");
        var group = await Provisioning.CreatePublishedGroup(owner, $"e2echapbuy{Guid.NewGuid():N}");
        var ownerId = await Members.GetMemberId(owner.Email);
        await MemberSubscriptions.EnsureActive(ownerId, siteSubscription.Id, siteSubscription.PriceId);
        await ChapterPaymentAccounts.EnsureSetupComplete(
            group.ChapterId, ownerId, E2ESettings.StripeConnectedAccountId(PlatformTypeId),
            E2ESettings.EnvironmentTypeId);

        // The owner creates a non-recurring chapter subscription (real Stripe product/price), on a throwaway
        // browser so this test's own browser is free for the buyer.
        var subscriptionName = $"e2e-chaptersub-{Guid.NewGuid():N}";
        await Provisioning.CreateChapterSubscription(
            group, owner, subscriptionName, amount: 5m, durationMonths: 1, recurring: false);
        var subscriptionId = await ChapterSubscriptions.GetId(group.ChapterId, subscriptionName)
            ?? throw new InvalidOperationException($"Chapter subscription '{subscriptionName}' was not created.");
        var currencyId = await ChapterSubscriptions.GetCurrencyId(group.ChapterId, subscriptionName)
            ?? throw new InvalidOperationException($"Chapter subscription '{subscriptionName}' has no currency.");

        return (group, subscriptionId, currencyId);
    }

    private static async Task<bool> PollForPurchaseRecord(Guid memberId, Guid chapterSubscriptionId)
    {
        var deadline = DateTime.UtcNow.AddSeconds(90);
        while (DateTime.UtcNow < deadline)
        {
            if (await MemberChapterSubscriptions.HasSubscriptionRecord(memberId, chapterSubscriptionId))
            {
                return true;
            }

            await Task.Delay(2000);
        }

        return await MemberChapterSubscriptions.HasSubscriptionRecord(memberId, chapterSubscriptionId);
    }

    /* The slowest step in the flow, and the one furthest from the click: the settlement is read on a job
       scheduled for the provider's own settlement delay after the purchase webhook, and the transfer is made
       from what it read. TransferredUtc is what says the whole of that finished. */
    private static async Task<TestPaymentTransfer?> PollForTransfer(Guid memberId, Guid chapterId)
    {
        var deadline = DateTime.UtcNow.AddSeconds(120);
        while (DateTime.UtcNow < deadline)
        {
            var transfer = await Payments.GetTransfer(memberId, chapterId);
            if (transfer?.TransferredUtc != null)
            {
                return transfer;
            }

            await Task.Delay(2000);
        }

        return await Payments.GetTransfer(memberId, chapterId);
    }
}
