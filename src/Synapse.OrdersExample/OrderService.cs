using Microsoft.Extensions.Logging;
using Synapse.OrdersExample.Model;
using System.Runtime.CompilerServices;

//  allow internal methodes to be visible to unit test project
[assembly: InternalsVisibleTo("Sysnapse.OrdersExample.Test")]

namespace Synapse.OrdersExample;

/// <summary>
/// Order Service Interface
/// </summary>
public interface IOrderService
{
    Task RunAsync(CancellationToken cancellationToken);
}

/// <summary>
/// Order Service
/// </summary>
public class OrderService : IOrderService
{
    private readonly IAlertService _alertService;
    private readonly ILogger _logger;
    private readonly IRestService _restService;

    private readonly string _ordersUrl;
    private readonly string _updateUrl;

    /// <summary>
    /// CTOR
    /// </summary>
    /// <param name="alertService">notification service</param>
    /// <param name="restService">http rest service</param>
    /// <param name="logger">logger service</param>
    public OrderService(IAlertService alertService, IRestService restService, UrlConfiguration urlConfiguration, ILogger<OrderService> logger)
    {
        _ordersUrl = urlConfiguration.OrdersApi;
        _updateUrl = urlConfiguration.UpdateApi;

        _alertService = alertService;
        _restService = restService;
        _logger = logger;
    }

    /// <summary>
    /// Order processing main method
    /// </summary>
    /// <param name="cancellationToken">cancellation token</param>
    /// <returns>void</returns>
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Start of App");

        //  Where is items.Status set to "Delivered"?
        var medicalEquipmentOrders = await FetchMedicalEquipmentOrders(cancellationToken);
        foreach (var order in medicalEquipmentOrders)
        {
            cancellationToken.ThrowIfCancellationRequested();

            //  Implement try/catch so that one order failure does not stop the processing of other orders
            try
            {
                var updatedOrder = ProcessOrder(order, cancellationToken);
                await SendAlertAndUpdateOrder(updatedOrder, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process order: OrderId {orderId}.", order.OrderId);
            }
        }

        _logger.LogInformation("Results sent to relevant APIs.");
    }

    /// <summary>
    /// Get medical equipment orders
    /// </summary>
    /// <param name="cancellationToken">cancellation token</param>
    /// <returns></returns>
    internal async Task<Order[]> FetchMedicalEquipmentOrders(CancellationToken cancellationToken)
    {
        var ordersData = await _restService.GetAsync<Order[]?>(_ordersUrl, cancellationToken);
        if (ordersData == null)
        {
            _logger.LogError("Failed to fetch orders from API.");
            ordersData = Array.Empty<Order>();
        }

        _logger.LogInformation("Fetched {ordersCount} orders from API.", ordersData.Length);
        return ordersData;
    }

    /// <summary>
    /// Process the order
    /// </summary>
    /// <param name="order"></param>
    /// <param name="cancellationToken">cancellation token</param>
    /// <returns></returns>
    internal Order ProcessOrder(Order order, CancellationToken cancellationToken)
    {
        foreach (var item in order.Items)
        {
            cancellationToken.ThrowIfCancellationRequested();

            //  not clear where the status is set to "Delivered"
            if (IsItemDelivered(item))
            {
                _alertService.SendAlertMessage(item, order.OrderId, cancellationToken);
                IncrementDeliveryNotification(item);
            }
        }

        return order;
    }

    /// <summary>
    /// Check if Item is delivered
    /// </summary>
    /// <param name="item"></param>
    /// <returns>true if status is delivered</returns>
    private bool IsItemDelivered(OrderItem item)
    {
        return item.Status == ItemStatus.Delivered;
    }

    /// <summary>
    /// Increment the delivery notification count
    /// </summary>
    /// <param name="item">order</param>
    /// <remarks>OrderItem is not persisted so can be removed</remarks>
    private void IncrementDeliveryNotification(OrderItem item)
    {
        item.DeliveryNotification += 1;
    }

    /// <summary>
    /// Send alert and update(?) order
    /// </summary>
    /// <param name="order"></param>
    /// <param name="cancellationToken">cancellation token</param>
    /// <returns>void</returns>
    /// <remarks>not sure how the order object is updated</remarks>
    internal async Task SendAlertAndUpdateOrder(Order order, CancellationToken cancellationToken)
    {
        try
        {
            await _restService.PostAsync<Order>(_updateUrl, order, cancellationToken);
            _logger.LogInformation("Updated order sent for processing: OrderId {orderId}", order.OrderId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send updated order for processing: OrderId {orderId}.", order.OrderId);
            throw;
        }
    }
}
