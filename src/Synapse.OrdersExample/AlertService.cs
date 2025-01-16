using Microsoft.Extensions.Logging;
using Synapse.OrdersExample.Model;

namespace Synapse.OrdersExample
{
    /// <summary>
    /// Interface:  Alert Service
    /// </summary>
    public interface IAlertService
    {
        /// <summary>
        /// Delivery alert
        /// </summary>
        /// <param name="item">The item to send the alert for</param>
        /// <param name="orderId">The order id for the alert</param>
        /// <param name="cancellationToken">cancellation token</param>
        Task SendAlertMessage(OrderItem item, string orderId, CancellationToken cancellationToken);
    }

    /// <summary>
    /// Alert Notification Service
    /// </summary>
    public class AlertService : IAlertService
    {
        private readonly IRestService _restService;
        private readonly ILogger _logger;
        private readonly string _alertApiUrl;

        /// <summary>
        /// CTOR
        /// </summary>
        /// <param name="alertService"></param>
        /// <param name="restService"></param>
        /// <param name="logger"></param>
        public AlertService(IRestService restService, UrlConfiguration urlConfiguration, ILogger<AlertService> logger)
        {
            _alertApiUrl = urlConfiguration.AlertApi;
            _restService = restService;
            _logger = logger;
        }

        /// <summary>
        /// Delivery alert
        /// </summary>
        /// <param name="item">The item to send the alert for</param>
        /// <param name="orderId">The order id for the alert</param>
        /// <param name="cancellationToken">cancellation token</param>
        public async Task SendAlertMessage(OrderItem item, string orderId, CancellationToken cancellationToken)
        {
            var alertData = new AlertData()
            {
                Message = $"Alert for delivered item: Order {orderId}, Item: {item.Description}, " +
                          $"Delivery Notifications: {item.DeliveryNotification}"
            };

            try
            {
                await _restService.PostAsync<AlertData>(_alertApiUrl, alertData, cancellationToken);
                _logger.LogInformation("Alert sent for delivered item: {description}", item.Description);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send alert for delivered item: {description}", item.Description);
            }
        }
    }
}

