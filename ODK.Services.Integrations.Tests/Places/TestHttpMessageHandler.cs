using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ODK.Services.Integrations.Tests.Places;

/// <summary>
/// Answers every request with one canned response, and records the request it was asked. Enough for a
/// client that makes a single call; a second canned response would want a queue.
/// </summary>
internal class TestHttpMessageHandler : HttpMessageHandler
{
    private readonly string _content;
    private readonly Exception? _exception;
    private readonly HttpStatusCode _statusCode;

    public TestHttpMessageHandler(HttpStatusCode statusCode, string content)
    {
        _content = content;
        _statusCode = statusCode;
    }

    public TestHttpMessageHandler(Exception exception)
    {
        _content = string.Empty;
        _exception = exception;
        _statusCode = HttpStatusCode.OK;
    }

    public HttpRequestMessage? Request { get; private set; }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Request = request;

        if (_exception != null)
        {
            throw _exception;
        }

        return Task.FromResult(new HttpResponseMessage(_statusCode)
        {
            Content = new StringContent(_content)
        });
    }
}
