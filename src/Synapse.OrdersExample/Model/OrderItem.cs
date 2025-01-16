namespace Synapse.OrdersExample.Model;

/// <summary>
/// Order item details
/// </summary>
public class OrderItem
{
    /// <summary>
    /// Order description
    /// </summary>
    public required string Description { get; set; }

    /// <summary>
    /// Order item status
    /// </summary>
    public required ItemStatus Status { get; set; }

    /// <summary>
    /// Delivery notification sent
    /// </summary>
    public required int DeliveryNotification { get; set; }
}
