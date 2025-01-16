using Microsoft.Extensions.Logging;
using Moq;
using Synapse.OrdersExample;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Synapse.OrdersExample.Exceptions;

namespace Sysnapse.OrdersExample.Test;

/// <summary>
/// REST Service Test
/// </summary>
public class RestServiceTest
{
    private ITestOutputHelper _output;
    private ILogger<RestService> _logger;
    private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
    private readonly RestService _restService;


    /// <summary>
    /// CTOR
    /// </summary>
    /// <param name="output"></param>
    public RestServiceTest(ITestOutputHelper output)
    {
        _output = output;
        _logger = new TestLogger<RestService>(output);

        _httpClientFactoryMock = new Mock<IHttpClientFactory>();
        _restService = new RestService(_httpClientFactoryMock.Object, _logger);
    }


    [Fact]
    public async Task GetAsync_ReturnsDeserializedResponse_WhenRequestIsSuccessful()
    {
        var url = "https://api.example.com/data";
        var responseData = "{\"key\":\"value\"}";
        var httpClient = new HttpClient(new TestHttpMessageHandler(responseData, HttpStatusCode.OK));
        _httpClientFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient);

        var result = await _restService.GetAsync<Dictionary<string, string>>(url);

        Assert.NotNull(result);
        Assert.Equal("value", result["key"]);
    }

    [Fact]
    public async Task GetAsync_ThrowsInvalidGetException_WhenRequestFails()
    {
        var url = "https://api.example.com/data";
        var httpClient = new HttpClient(new TestHttpMessageHandler(string.Empty, HttpStatusCode.BadRequest));
        _httpClientFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient);

        await Assert.ThrowsAsync<InvalidGetException>(() => _restService.GetAsync<object>(url));
    }


    [Fact]
    public async Task GetAsync_LogsError_WhenRequestIsCancelled()
    {
        var url = "https://api.example.com/data";
        var httpClient = new HttpClient(new TestHttpMessageHandler(string.Empty, HttpStatusCode.OK, true));
        _httpClientFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient);
        var loggerMock = new Mock<ILogger<RestService>>();
        var restService = new RestService(_httpClientFactoryMock.Object, loggerMock.Object);

        await restService.GetAsync<object>(url, new CancellationToken(true));

        loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v,t) => $"{v}".Contains("GET request to")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }


    [Fact]
    public async Task PostAsync_SuccessfulRequest_LogsInformation()
    {
        var url = "https://api.example.com/data";
        var input = new { key = "value" };
        var httpClient = new HttpClient(new TestHttpMessageHandler(string.Empty, HttpStatusCode.OK));
        _httpClientFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient);
        var loggerMock = new Mock<ILogger<RestService>>();
        var restService = new RestService(_httpClientFactoryMock.Object, loggerMock.Object);

        await restService.PostAsync(url, input);

        loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => $"{v}".Contains("POST to")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task PostAsync_FailedRequest_ThrowsInvalidPostException()
    {
        var url = "https://api.example.com/data";
        var input = new { key = "value" };
        var httpClient = new HttpClient(new TestHttpMessageHandler(string.Empty, HttpStatusCode.BadRequest));
        _httpClientFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient);

        await Assert.ThrowsAsync<InvalidPostException>(() => _restService.PostAsync(url, input));
    }

    [Fact]
    public async Task PostAsync_RequestCancelled_LogsError()
    {
        var url = "https://api.example.com/data";
        var input = new { key = "value" };
        var httpClient = new HttpClient(new TestHttpMessageHandler(string.Empty, HttpStatusCode.OK, true));
        _httpClientFactoryMock.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient);
        var loggerMock = new Mock<ILogger<RestService>>();
        var restService = new RestService(_httpClientFactoryMock.Object, loggerMock.Object);

        await restService.PostAsync(url, input, new CancellationToken(true));

        loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => $"{v}".Contains("POST request to")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

}
