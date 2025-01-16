using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Synapse.OrdersExample.Model;

/// <summary>
/// API service urls model
/// </summary>
public class UrlConfiguration
{
    /// <summary>
    /// Alert Service api url
    /// </summary>
    public required string AlertApi { get; set; }

    /// <summary>
    /// Orders Service api url
    /// </summary>
    public required string OrdersApi { get; set; }

    /// <summary>
    /// Update Service api url
    /// </summary>
    public required string UpdateApi { get; set; }
}
