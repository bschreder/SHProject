using Microsoft.Extensions.Logging;
using Synapse.OrdersExample.Exceptions;
using System.Net.Http.Json;
using System.Text.Json;

namespace Synapse.OrdersExample;

/// <summary>
/// REST Service Interface
/// </summary>
public interface IRestService
{
    /// <summary>
    /// HTTP GET request
    /// </summary>
    /// <param name="url">api url</param>
    /// <returns>TResponse object</returns>
    Task<TResponse?> GetAsync<TResponse>(string url, CancellationToken cancellationToken = default)
        where TResponse : class?;

    /// <summary>
    /// HTTP POST request
    /// </summary>
    /// <param name="url">api url</param>
    /// <param name="input">TRequest object</param>
    /// <returns>TResponse object</returns>
    Task<TResponse?> PostAsync<TRequest, TResponse>(string url, TRequest input, CancellationToken cancellationToken = default)
        where TRequest : class
        where TResponse : class?, new();

    /// <summary>
    /// HTTP POST request
    /// </summary>
    /// <param name="url">api url</param>
    /// <param name="input">TRequest object</param>
    /// <exception cref="InvalidPostException">api request failure</exception>
    Task PostAsync<TRequest>(string url, TRequest input, CancellationToken cancellationToken = default)
        where TRequest : class;
}

/// <summary>
/// REST Service
/// </summary>
public class RestService : IRestService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger _logger;

    /// <summary>
    /// CTOR
    /// </summary>
    /// <param name="httpClientFactory">http client factory</param>
    /// <param name="logger">logger serivce</param>
    public RestService(IHttpClientFactory httpClientFactory, ILogger<RestService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    /// <summary>
    /// HTTP GET request
    /// </summary>
    /// <param name="url">api url</param>
    /// <param name="ct">cancellation token</param>
    /// <returns>response object</returns>
    public async Task<TResponse?> GetAsync<TResponse>(string url, CancellationToken ct = default)
        where TResponse : class?
    {
        using var httpClient = _httpClientFactory.CreateClient();

        //  TODO:  Add headers (Authorization, RequestId, ...) to request
        //  TODO:  Response object should be "Problem Details" (or something similar) to handle errors
        //  TODO:  Add retry, circuit breaker, ...  with Polly
        try
        {
            using var response = await httpClient.GetAsync(url, ct);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("GET to {url} was successful with status code {statusCode}", url, response.StatusCode);
                var ordersData = await response.Content.ReadAsStringAsync(ct);
                return JsonSerializer.Deserialize<TResponse>(ordersData);
            }
            else
            {
                _logger.LogError("Failed POST to {url} with status code {statusCode}", url, response.StatusCode);
                throw new InvalidGetException($"Failed GET to {url}");
            }
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogError(ex, "GET request to {url} was cancelled", url);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Failed GET to {url}", url);
            throw;
        }

        return null;
    }

    /// <summary>
    /// Http POST request (with response object)
    /// </summary>
    /// <typeparam name="TRequest">type of request object</typeparam>
    /// <typeparam name="TResponse">type of response object</typeparam>
    /// <param name="url">post url</param>
    /// <param name="input">request object</param>
    /// <param name="ct">cancellation token</param>
    /// <returns>response object</returns>
    public async Task<TResponse?> PostAsync<TRequest, TResponse>(string url, TRequest input, CancellationToken ct = default)
        where TRequest : class
        where TResponse : class?, new()
    {
        using var httpClient = _httpClientFactory.CreateClient();

        //  TODO:  Add headers (Authorization, RequestId, ...) to request
        //  TODO:  Response object should be "Problem Details" (or something similar) to handle errors
        //  TODO:  Add retry, circuit breaker, ...  with Polly
        try
        {
#if NET9_0_OR_GREATER
            using var response = await httpClient.PostAsJsonAsync(url, input, ct);
#else
            using var content = new StringContent(JsonSerializer.Serialize(input), Encoding.UTF8, "application/json");
            using var response = await httpClient.PostAsync(url, content, ct);
#endif

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("POST to {url} was successful with status code {statusCode}", url, response.StatusCode);
                var contentLength = response.Content.Headers.ContentLength ?? 0;
                if (contentLength == 0)
                {
                    //  This handles the case where the POST doesn't return content (http status 204, 201, ...)
                    return new TResponse();
                }
                else
                {
                    var responseData = await response.Content.ReadAsStringAsync(ct);
                    return JsonSerializer.Deserialize<TResponse>(responseData);
                }
            }
            else
            {
                _logger.LogError("Failed POST to {url} with status code {statusCode}", url, response.StatusCode);
                throw new InvalidPostException($"Failed POST to {url}");
            }
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogError(ex, "POST request to {url} was cancelled", url);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Failed POST to {url}", url);
            throw;
        }
        return null;
    }

    /// <summary>
    /// Http POST request
    /// </summary>
    /// <typeparam name="TRequest">type of request object</typeparam>
    /// <param name="url">post url</param>
    /// <param name="input">request object</param>
    /// <param name="ct">cancellation token</param>
    /// <returns>void</returns>
    /// <exception cref="InvalidPostException">api request failure</exception>
    /// <remarks>Post doesn't return response object so assuming "Happy Path with Exception" pattern</remarks>
    public async Task PostAsync<TRequest>(string url, TRequest input, CancellationToken ct = default)
    where TRequest : class
    {
        using var httpClient = _httpClientFactory.CreateClient();

        //  TODO:  Add headers (Authorization, RequestId, ...) to request
        //  TODO:  Add retry, circuit breaker, ...  with Polly
        try
        {
            using var response = await httpClient.PostAsJsonAsync(url, input, ct);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("POST to {url} was successful with status code {statusCode}", url, response.StatusCode);
                return;
            }
            else
            {
                _logger.LogError("Failed POST to {url} with status code {statusCode}", url, response.StatusCode);
                throw new InvalidPostException($"Failed POST to {url}");
            }
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogError(ex, "POST request to {url} was cancelled", url);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Failed POST to {url}", url);
            throw;
        }
    }
}
