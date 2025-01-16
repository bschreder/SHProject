using Microsoft.Extensions.Logging;
using Moq;
using Synapse.OrdersExample;
using Synapse.OrdersExample.Model;

namespace Sysnapse.OrdersExample.Test;

/// <summary>
/// Order Service Test
/// </summary>
public class OrderServiceTest
{

    private readonly Mock<IAlertService> _mockAlertService;
    private readonly Mock<IRestService> _mockRestService;
    private readonly Mock<ILogger<OrderService>> _mockLogger;
    private readonly OrderService _orderService;
    private const string _alertApiUrl = "http://example.com/alert";
    private const string _ordersApiUrl = "http://example.com/orders";
    private const string _updateApiUrl = "http://example.com/update";


    /// <summary>
    /// CTOR
    /// </summary>
    public OrderServiceTest()
    {
        _mockAlertService = new Mock<IAlertService>();
        _mockRestService = new Mock<IRestService>();
        _mockLogger = new Mock<ILogger<OrderService>>();

        var urlConfiguration = new UrlConfiguration
        {
            AlertApi = _alertApiUrl,
            OrdersApi = _ordersApiUrl,
            UpdateApi = _updateApiUrl
        };

        _orderService = new OrderService(_mockAlertService.Object, _mockRestService.Object, urlConfiguration, _mockLogger.Object);
    }

    /// <summary>
    /// Send alert and update order sucessfully
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task SendAlertAndUpdateOrder_SendsAlertAndUpdatesOrderSuccessfully()
    {
        var order = new Order
        {
            OrderId = "12345",
            Items = new List<OrderItem>
                {
                    new OrderItem { Description = "Item 1", Status = ItemStatus.Delivered, DeliveryNotification = 0 }
                }
        };
        var cancellationToken = new CancellationToken();

        _mockRestService
            .Setup(r => r.PostAsync<Order>(It.IsAny<string>(), It.IsAny<Order>(), default))
            .Returns(Task.CompletedTask);

        await _orderService.SendAlertAndUpdateOrder(order, cancellationToken);

        _mockRestService.Verify(r => r.PostAsync<Order>(_updateApiUrl, order, default), Times.Once);
        _mockLogger.Verify(l => l.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => $"{v}".Contains("Updated order sent for processing: OrderId 12345")),
            null,
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }

    /// <summary>
    /// Send alert and update order logs and rethrows exception when post async fails
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task SendAlertAndUpdateOrder_LogsAndRethrowsException_WhenPostAsyncFails()
    {
        var order = new Order
        {
            OrderId = "12345",
            Items = new System.Collections.Generic.List<OrderItem>
                {
                    new OrderItem { Description = "Item 1", Status = ItemStatus.Delivered, DeliveryNotification = 0 }
                }
        };
        var cancellationToken = new CancellationToken();

        var exception = new Exception("API error");
        _mockRestService
            .Setup(r => r.PostAsync<Order>(It.IsAny<string>(), It.IsAny<Order>(), default))
            .ThrowsAsync(exception);

        var ex = await Assert.ThrowsAsync<Exception>(() => _orderService.SendAlertAndUpdateOrder(order, cancellationToken));

        Assert.Equal("API error", ex.Message);

        _mockLogger.Verify(l => l.Log(
            LogLevel.Error,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => $"{v}".Contains("Failed to send updated order for processing: OrderId 12345.")),
            exception,
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }

    /// <summary>
    /// Process order when item is marked "delivered"
    /// </summary>
    [Fact]
    public void ProcessOrder_SendsAlertAndIncrementsNotification_WhenItemIsDelivered()
    {
        var order = new Order
        {
            OrderId = "12345",
            Items = new List<OrderItem>
                {
                    new OrderItem { Description = "Item 1", Status = ItemStatus.Delivered, DeliveryNotification = 0 }
                }
        };
        var cancellationToken = new CancellationToken();

        var result = _orderService.ProcessOrder(order, cancellationToken);

        _mockAlertService.Verify(a => a.SendAlertMessage(It.IsAny<OrderItem>(), "12345", It.IsAny<CancellationToken>()), Times.Once);
        Assert.Equal(1, result.Items[0].DeliveryNotification);
    }

    /// <summary>
    /// Process order when item is not delivered
    /// </summary>
    [Fact]
    public void ProcessOrder_DoesNotSendAlert_WhenItemIsNotDelivered()
    {
        var order = new Order
        {
            OrderId = "12345",
            Items = new List<OrderItem>
                {
                    new OrderItem { Description = "Item 1", Status = ItemStatus.Unknown, DeliveryNotification = 0 }
                }
        };
        var cancellationToken = new CancellationToken();

        var result = _orderService.ProcessOrder(order, cancellationToken);

        _mockAlertService.Verify(a => a.SendAlertMessage(It.IsAny<OrderItem>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        Assert.Equal(0, result.Items[0].DeliveryNotification);
    }


    /// <summary>
    /// Stop Processing orders when cancellation is requested
    /// </summary>
    [Fact]
    public void ProcessOrder_CancellationRequested_ThrowsOperationCanceledException()
    {
        var order = new Order
        {
            OrderId = "12345",
            Items = new List<OrderItem>
                {
                    new OrderItem { Description = "Item 1", Status = ItemStatus.Delivered, DeliveryNotification = 0 }
                }
        };
        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;
        cancellationTokenSource.Cancel();

        Assert.Throws<OperationCanceledException>(() => _orderService.ProcessOrder(order, cancellationToken));

        // Verify that no further methods are called after cancellation
        _mockAlertService.Verify(a => a.SendAlertMessage(It.IsAny<OrderItem>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    /// <summary>
    /// Fetch medical equipment orders returns orders when GET API call is successful
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task FetchMedicalEquipmentOrders_ReturnsOrders_WhenApiCallIsSuccessful()
    {
        var orders = new[]
        {
                new Order
                {
                    OrderId = "12345",
                    Items = new List<OrderItem>
                    {
                        new OrderItem { Description = "Item 1", Status = ItemStatus.Delivered, DeliveryNotification = 0 }
                    }
                }
            };
        var cancellationToken = new CancellationToken();


        _mockRestService
            .Setup(r => r.GetAsync<Order[]?>(It.IsAny<string>(), default))
            .ReturnsAsync(orders);

        var result = await _orderService.FetchMedicalEquipmentOrders(cancellationToken);

        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("12345", result[0].OrderId);
        _mockLogger.Verify(l => l.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => $"{v}".Contains("Fetched 1 orders from API.")),
            null,
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }

    /// <summary>
    /// Fetch medical equipment orders returns empty array when GET API call fails
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task FetchMedicalEquipmentOrders_ReturnsEmptyArray_WhenApiCallFails()
    {
        var cancellationToken = new CancellationToken();

        _mockRestService
            .Setup(r => r.GetAsync<Order[]?>(It.IsAny<string>(), default))
            .ReturnsAsync((Order[]?)null);

        var result = await _orderService.FetchMedicalEquipmentOrders(cancellationToken);

        Assert.NotNull(result);
        Assert.Empty(result);
        _mockLogger.Verify(l => l.Log(
            LogLevel.Error,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => $"{v}".Contains("Failed to fetch orders from API.")),
            null,
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }

    /// <summary>
    /// Order service runs successfully
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task RunAsync_ProcessesOrdersSuccessfully()
    {
        var orders = new[]
        {
                new Order
                {
                    OrderId = "12345",
                    Items = new List<OrderItem>
                    {
                        new OrderItem { Description = "Item 1", Status = ItemStatus.Delivered, DeliveryNotification = 0 }
                    }
                }
            };
        var cancellationToken = new CancellationToken();

        _mockRestService
            .Setup(r => r.GetAsync<Order[]?>(It.IsAny<string>(), default))
            .ReturnsAsync(orders);

        _mockRestService
            .Setup(r => r.PostAsync<Order>(It.IsAny<string>(), It.IsAny<Order>(), default))
            .Returns(Task.CompletedTask);

        await _orderService.RunAsync(cancellationToken);

        _mockLogger.Verify(l => l.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => $"{v}".Contains("Start of App")),
            null,
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);

        _mockLogger.Verify(l => l.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => $"{v}".Contains("Fetched 1 orders from API.")),
            null,
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);

        _mockAlertService.Verify(a => a.SendAlertMessage(It.IsAny<OrderItem>(), "12345", It.IsAny<CancellationToken>()), Times.Once);

        _mockRestService.Verify(r => r.PostAsync<Order>(It.IsAny<string>(), It.IsAny<Order>(), default), Times.Once);

        _mockLogger.Verify(l => l.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => $"{v}".Contains("Results sent to relevant APIs.")),
            null,
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }

    /// <summary>
    /// Order service fails to run
    /// </summary>
    /// <returns></returns>
    [Fact]
    public async Task RunAsync_LogsError_WhenOrderProcessingFails()
    {
        var orders = new[]
        {
                new Order
                {
                    OrderId = "12345",
                    Items = new List<OrderItem>
                    {
                        new OrderItem { Description = "Item 1", Status = ItemStatus.Delivered, DeliveryNotification = 0 }
                    }
                }
            };
        var cancellationToken = new CancellationToken();

        _mockRestService
            .Setup(r => r.GetAsync<Order[]?>(It.IsAny<string>(), default))
            .ReturnsAsync(orders);

        _mockRestService
            .Setup(r => r.PostAsync<Order>(It.IsAny<string>(), It.IsAny<Order>(), default))
            .ThrowsAsync(new System.Exception("API error"));

        await _orderService.RunAsync(cancellationToken);

        _mockLogger.Verify(l => l.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => $"{v}".Contains("Start of App")),
            null,
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);

        _mockLogger.Verify(l => l.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => $"{v}".Contains("Fetched 1 orders from API.")),
            null,
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);

        _mockAlertService.Verify(a => a.SendAlertMessage(It.IsAny<OrderItem>(), "12345", It.IsAny<CancellationToken>()), Times.Once);

        _mockLogger.Verify(l => l.Log(
            LogLevel.Error,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => $"{v}".Contains("Failed to process order: OrderId 12345.")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);

        _mockLogger.Verify(l => l.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => $"{v}".Contains("Results sent to relevant APIs.")),
            null,
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }

    [Fact]
    public async Task RunAsync_CancellationRequested_ThrowsOperationCanceledException()
    {
        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;
        cancellationTokenSource.Cancel();

        _mockRestService.Setup(r => r.GetAsync<Order[]>(_ordersApiUrl, cancellationToken))
                        .ReturnsAsync(new Order[]
                        {
                                new Order
                                {
                                    OrderId = "12345",
                                    Items = new List<OrderItem>
                                    {
                                        new OrderItem { Description = "Test Item", Status = ItemStatus.Delivered, DeliveryNotification = 1 }
                                    }
                                }
                        });

        await Assert.ThrowsAsync<OperationCanceledException>(() => _orderService.RunAsync(cancellationToken));

        // Verify that no further methods are called after cancellation
        _mockAlertService.Verify(a => a.SendAlertMessage(It.IsAny<OrderItem>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockRestService.Verify(r => r.PostAsync<Order>(_updateApiUrl, It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
