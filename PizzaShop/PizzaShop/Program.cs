using System.Text.Json;
using System.Threading.Channels;
using AsbGateway;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PizzaShop;
using Shared;

//our pizza shop has internal channels for its orchestration
// -- cookrequests are sent to the kitchen
// -- deliveryrequests are sent to the dispatch service
// -- courierstatusupdates are sent to the kitchen


//our collection of couriers, names are used within queues & streams as well
var hostBuilder = new HostBuilder()
    .ConfigureAppConfiguration((hostContext, config) =>
    {
        config.AddEnvironmentVariables();
    })
    .ConfigureServices((hostContext, services) =>
    {
        services.AddSingleton(Channel.CreateBounded<CookRequest>(10));
        services.AddSingleton(Channel.CreateBounded<DeliveryRequest>(10));
        services.AddSingleton(Channel.CreateBounded<CourierStatusUpdate>(10));

        services.Configure<CourierSettings>(hostContext.Configuration.GetSection("Courier"));
        services.Configure<ServiceBusSettings>(hostContext.Configuration.GetSection("ServiceBus"));
        var couriers = hostContext.Configuration.GetSection("Courier").Get<CourierSettings>()?.Names ?? Array.Empty<string>();

        services.AddPizzaShopTelemetry("PizzaShop");
        //we use multiple pumps, because we don't want all channels to become unresponsive because one gets busy!!! In practice, we would
        //want competing consumers here
        services.AddAzureClients(clientBuilder => {
            clientBuilder.AddServiceBusClient(hostContext.Configuration["ServiceBus:ConnectionString"]);
        });

        //we have distinct queues for job accepted and job rejected to listen to each courier - all post to the same channel
        foreach (var courier in couriers)
        {
            //work around the problem of multiple service registration by using a singleton explicity, see https://github.com/dotnet/runtime/issues/38751
            services.AddSingleton<IHostedService, AsbMessagePumpService<JobAccepted>>(serviceProvider => ServiceSetupHelpers.AddHostedJobAcceptedService(courier, serviceProvider));
            services.AddSingleton<IHostedService, AsbMessagePumpService<JobRejected>>(serviceProvider => ServiceSetupHelpers.AddHostedJobRejectedService(courier, serviceProvider));
        }

        services.AddHostedService(ServiceSetupHelpers.AddHostedOrderService);

        //We use channels for our internal pipeline. Channels let us easily wait on work without synchronization primitives
        services.AddHostedService(ServiceSetupHelpers.AddHostedKitchenService);
        services.AddHostedService(ServiceSetupHelpers.AddHostedDispatcherService);
    });

await hostBuilder.Build().RunAsync();

public class CourierSettings
{
    public string[]? Names { get; set; }
}

public class ServiceBusSettings
{
    public string OrderQueueName { get; set; } = string.Empty;
    public string JobAcceptedQueueName { get; set; } = string.Empty;
    public string JobRejectedQueueName { get; set; } = string.Empty;
}