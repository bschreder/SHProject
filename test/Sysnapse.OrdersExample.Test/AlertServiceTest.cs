using Microsoft.Extensions.Logging;
using Moq;
using Synapse.OrdersExample;
using Synapse.OrdersExample.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace Sysnapse.OrdersExample.Test;

/// <summary>
/// Alert Service Test
/// </summary>
public class AlertServiceTest
{
    private readonly ITestOutputHelper _output;
    private readonly Mock<ILogger<AlertService>> _mockLogger;
    private readonly UrlConfiguration _urlConfiguration;
    private readonly Mock<IRestService> _mockRestService;
    private readonly AlertService _alertService;
    private const string _alertApi = "https://api.example.com/alert";

    /// <summary>
    /// CTOR
    /// </summary>
    /// <param name="output"></param>
    public AlertServiceTest(ITestOutputHelper output)
    {
        _output = output;
        _mockLogger = new Mock<ILogger<AlertService>>();
        _urlConfiguration = new UrlConfiguration()
        {
            AlertApi = _alertApi,
            OrdersApi = "",
            UpdateApi = ""
        };

        _mockRestService = new Mock<IRestService>();
        _alertService = new AlertService(_mockRestService.Object, _urlConfiguration, _mockLogger.Object);
    }

    /// <summary>
    /// Send alert message successfully
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task SendAlertMessage_SendsAlertSuccessfully()
    {
        var orderItem = new OrderItem { Description = "Test Item", Status = ItemStatus.Delivered, DeliveryNotification = 1 };
        var orderId = "12345";
        var cancellationToken = new CancellationToken();


        await _alertService.SendAlertMessage(orderItem, orderId, cancellationToken);

        _mockRestService.Verify(r => r.PostAsync<AlertData>(_urlConfiguration.AlertApi, It.IsAny<AlertData>(), default), Times.Once);
        _mockLogger.Verify(
            l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()), 
            Times.Once);

    }

    /// <summary>
    /// Send alert message logs error on failure
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task SendAlertMessage_LogsErrorOnFailure()
    {
        var orderItem = new OrderItem { Description = "Test Item", Status = ItemStatus.Delivered, DeliveryNotification = 1 };
        var orderId = "12345";
        var cancellationToken = new CancellationToken();

        _mockRestService.Setup(r => r.PostAsync<AlertData>(_urlConfiguration.AlertApi, It.IsAny<AlertData>(), default))
                        .ThrowsAsync(new System.Exception("Test exception"));

        await _alertService.SendAlertMessage(orderItem, orderId, cancellationToken);

        _mockRestService.Verify(r => r.PostAsync<AlertData>(_urlConfiguration.AlertApi, It.IsAny<AlertData>(), default), Times.Once);
        _mockLogger.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()),
            Times.Once);
    }
}
