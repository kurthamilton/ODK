using System;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Http;

namespace ODK.Web.Common.Extensions;

public static class HttpContextExtensions
{
    /// <summary>
    /// Whether <paramref name="exception"/> is the client having gone away mid-request. Nothing can be
    /// concluded from a request body that was never fully read, and nothing can be served to a connection
    /// that no longer exists, so a failure carrying this is not something the app did.
    /// </summary>
    /// <remarks>
    /// The abort token is the signal that holds however the server reports the underlying error, but it is
    /// not guaranteed to have been raised by the instant the exception is caught, so the inner chain is
    /// checked too: IIS surfaces a disconnected client as <see cref="ConnectionResetException"/>.
    /// </remarks>
    public static bool ClientDisconnected(this HttpContext httpContext, Exception exception)
    {
        if (httpContext.RequestAborted.IsCancellationRequested)
        {
            return true;
        }

        for (var current = exception; current != null; current = current.InnerException)
        {
            if (current is ConnectionResetException)
            {
                return true;
            }
        }

        return false;
    }
}
