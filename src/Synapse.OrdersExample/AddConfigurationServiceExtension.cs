using Microsoft.Extensions.Configuration;
using Synapse.OrdersExample.Model;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Configuration Service Collection Extension
/// </summary>
public static class AddConfigurationServiceCollectionExtension
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="services">service collection</param>
    /// <param name="configuration">configuration key/value store</param>
    /// <returns>service collection with api urls</returns>
    /// <exception cref="ArgumentNullException"></exception>
    [ExcludeFromCodeCoverage]
    public static IServiceCollection AddConfigurationService(this IServiceCollection services, IConfiguration configuration)
    {
        //  Using UrlConfiguration, the service urls can be changes based on environment

        //  Assume URLs section will not change at runtime (ie., requires a application restart to change)
        var urls = configuration.GetSection("Urls").Get<UrlConfiguration>() 
            ?? throw new ArgumentNullException("Urls section is missing in configuration");
        services.AddSingleton(urls);

        //  If URLs will change at runtime, use the following code but requires IOptionMonitor to be injected into service vs UrlConfiguration service
        //services.Configure<UrlConfiguration>(configuration.GetSection("Urls"));

        return services;
    }
}
