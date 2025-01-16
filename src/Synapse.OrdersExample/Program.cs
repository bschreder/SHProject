using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;


namespace Synapse.OrdersExample;

/// <summary>
/// I Get a list of orders from the API
/// I check if the order is in a delviered state, If yes then send a delivery alert and add one to deliveryNotification
/// I then update the order.   
/// </summary>
public class Program
{
    /// <summary>
    /// Create host builder for configuration and dependency injection services
    /// </summary>
    /// <returns></returns>
    public static IHostBuilder CreateHostBuilder() =>
        Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                services.AddConfigurationService(context.Configuration);

                services.AddHttpClient();
                services.AddScoped<IRestService, RestService>();

                services.AddSingleton<IOrderService, OrderService>();
                services.AddSingleton<IAlertService, AlertService>();
            })

            //  Logging service configuration
            //  Could be a structured logging service like Serilog
            //  or an OpenTelemetry logging service
            //  or a SIEM/XDR service
            .ConfigureLogging(builder =>
            {
                builder.Configure(options =>
                {
                    options.ActivityTrackingOptions = ActivityTrackingOptions.SpanId |
                                                      ActivityTrackingOptions.TraceId |
                                                      ActivityTrackingOptions.ParentId;
                });
                builder.AddConsole();
            });


    /// <summary>
    /// Application entry point
    /// </summary>
    /// <param name="args"></param>
    /// <returns>Status code</returns>
    public static async Task<int> Main(string[] args)
    {
        using var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;

        EventHandler handler = (sender, eventArgs) =>
        {
            cancellationTokenSource.Cancel();
            Thread.Sleep(250);
        };
        AppDomain.CurrentDomain.ProcessExit += handler;

        try
        {
            using var host = CreateHostBuilder().Build();
            await host.StartAsync();

            using var scope = host.Services.CreateScope();

            var application = scope.ServiceProvider.GetRequiredService<IOrderService>();
            await application.RunAsync(cancellationToken);

            await host.StopAsync();
            return 0;
        }
        catch (OperationCanceledException ex)
        {
            Console.WriteLine($"The application was cancelled: {ex.Message}");
            return 1;
        }
        catch (Exception ex)
        {
            //  Many logging services provide a static logger that could be used here instead of Console
            Console.WriteLine($"An error occurred: {ex.Message}");
            return 1;
        }
        finally
        {
            AppDomain.CurrentDomain.ProcessExit -= handler;
        }
    }
}