using ODK.Core.Payments;

namespace ODK.Services.Payments;

/// <summary>
/// Tells whoever is watching a payment that something about it has moved. Declared here and implemented in
/// the web layer, the same way <see cref="Tasks.IBackgroundTaskService"/> is: the work that knows a payment
/// changed lives in this project and the transport that carries the news does not.
/// </summary>
public interface IPaymentUpdateBroadcaster
{
    /// <summary>
    /// A checkout session has completed or expired. Carries no status: a watcher re-reads it from the status
    /// endpoint, which is the only thing entitled to say what the session is now. See the plan - a broadcast
    /// can be lost to a reconnect, or raised in a process the watcher is not connected to, so it is a
    /// prompt to look rather than the answer itself.
    /// </summary>
    /// <param name="externalSessionId"><see cref="PaymentCheckoutSession.SessionId"/>.</param>
    Task CheckoutSessionUpdated(string externalSessionId);
}
