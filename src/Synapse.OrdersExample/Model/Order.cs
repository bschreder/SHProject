namespace Synapse.OrdersExample.Model;

/// <summary>
/// Order Request Model
/// </summary>
public class Order
{
    /// <summary>
    /// Order Id
    /// </summary>
    public required string OrderId { get; set; }

    /// <summary>
    /// List of items in order
    /// </summary>
    public required List<OrderItem> Items { get; set; }
}
