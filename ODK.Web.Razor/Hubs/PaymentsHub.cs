using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using ODK.Core.Payments;
using ODK.Services;
using ODK.Services.Chapters;
using ODK.Services.Exceptions;
using ODK.Services.Payments;
using ODK.Services.Security;
using ODK.Web.Common.Extensions;
using ODK.Web.Common.Services;
using ODK.Web.Razor.Services;

namespace ODK.Web.Razor.Hubs;

/// <summary>
/// Lets a checkout page be told when the session it is waiting on moves, instead of asking every second.
/// Mapped at <c>/hubs/payments/{chapterId:guid?}</c> - the chapter is on the path so the request store
/// resolves it from a route value exactly as it does for a controller.
/// </summary>
[Authorize]
public class PaymentsHub : Hub
{
    /// <summary>The client-side handler a broadcast invokes.</summary>
    public const string CheckoutSessionUpdatedMessage = "checkoutSessionUpdated";

    private readonly IChapterAdminService _chapterAdminService;
    private readonly IPaymentService _paymentService;
    private readonly IRequestStore _requestStore;

    public PaymentsHub(
        IRequestStore requestStore,
        IPaymentService paymentService,
        IChapterAdminService chapterAdminService)
    {
        _chapterAdminService = chapterAdminService;
        _paymentService = paymentService;
        _requestStore = requestStore;
    }

    /// <summary>
    /// Keyed on the checkout session rather than on the member: a group's own subscription is bought by an
    /// admin who may not be the member the session belongs to, so nothing member-shaped identifies both
    /// ends of that case.
    /// </summary>
    public static string CheckoutSessionGroup(string externalSessionId)
        => $"checkout-session:{externalSessionId}";

    /// <summary>
    /// Watch a checkout session the group's owner is the buyer of, as an admin entitled to act for them.
    /// Authorised by the same call the matching status endpoint makes, so entitlement is stated once.
    /// </summary>
    /// <returns>The session's status now, so a completion between page render and connect is not waited on.</returns>
    public async Task<string> WatchChapterCheckoutSession(string externalSessionId)
    {
        var requestStore = await LoadRequestStore();

        var request = MemberChapterAdminServiceRequest.Create(
            ChapterAdminSecurable.SiteSubscription, requestStore.MemberChapterServiceRequest);

        var status = await _chapterAdminService.GetChapterPaymentCheckoutSessionStatus(
            request, externalSessionId);

        return await Watch(externalSessionId, status);
    }

    /// <inheritdoc cref="WatchChapterCheckoutSession"/>
    /// <summary>Watch a checkout session the connected member is the buyer of.</summary>
    public async Task<string> WatchCheckoutSession(string externalSessionId)
    {
        var requestStore = await LoadRequestStore();
        var chapter = requestStore.ChapterOrDefault;

        var status = chapter != null
            ? await _paymentService.GetMemberChapterPaymentCheckoutSessionStatus(
                requestStore.MemberServiceRequest, chapter.Id, externalSessionId)
            : await _paymentService.GetMemberSitePaymentCheckoutSessionStatus(
                requestStore.MemberServiceRequest, externalSessionId);

        return await Watch(externalSessionId, status);
    }

    /* SignalR gives each hub invocation its own scope, so the store the request-store middleware loads is
       never the one a hub method holds - which is why the hub endpoint skips that middleware and loads its
       own here. The connection's HttpContext is the request that opened the socket, so it carries the route
       values and the auth cookie the load needs. */
    private async Task<IRequestStore> LoadRequestStore()
    {
        var httpContext = Context.GetHttpContext()
            ?? throw new OdkNotAuthenticatedException();

        return await _requestStore.Load(
            HttpRequestContext.Create(httpContext.Request),
            Context.User?.MemberIdOrDefault(),
            Context.User?.SignedInMemberIds() ?? []);
    }

    private async Task<string> Watch(string externalSessionId, PaymentStatusType status)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, CheckoutSessionGroup(externalSessionId));
        return status.ToString();
    }
}
