using System.Net;

namespace Sysnapse.OrdersExample.Test;

/// <summary>
/// A custom HttpMessageHandler for testing purposes.
/// </summary>
public class TestHttpMessageHandler : HttpMessageHandler
{

    private readonly string _response;
    private readonly HttpStatusCode _statusCode;
    private readonly bool _throwOperationCanceled;

    /// <summary>
    /// Initializes a new instance of the <see cref="TestHttpMessageHandler"/> class.
    /// </summary>
    /// <param name="response">The response content to return.</param>
    /// <param name="statusCode">The HTTP status code to return.</param>
    /// <param name="throwOperationCanceled">Indicates whether to throw an OperationCanceledException.</param>

    public TestHttpMessageHandler(string response, HttpStatusCode statusCode, bool throwOperationCanceled = false)
    {
        _response = response;
        _statusCode = statusCode;
        _throwOperationCanceled = throwOperationCanceled;
    }

    /// <summary>
    /// Sends an HTTP request asynchronously.
    /// </summary>
    /// <param name="request">The HTTP request message.</param>
    /// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
    /// <returns>The HTTP response message.</returns>
    /// <exception cref="OperationCanceledException">Thrown when the operation is canceled.</exception>
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (_throwOperationCanceled)
        {
            throw new OperationCanceledException();
        }

        return await Task.FromResult(new HttpResponseMessage
        {
            StatusCode = _statusCode,
            Content = new StringContent(_response)
        });
    }

}
