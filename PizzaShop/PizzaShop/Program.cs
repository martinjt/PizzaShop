using System.Threading.Channels;
using AsbGateway;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PizzaShop;
using Shared;
using static PizzaShop.ServiceSetupHelpers;

//our pizza shop has internal channels for its orchestration
// -- cookrequests are sent to the kitchen
// -- deliveryrequests are sent to the dispatch service
// -- courierstatusupdates are sent to the kitchen


//our collection of couriers, names are used within queues & streams as well
var hostBuilder = new HostApplicationBuilder();

hostBuilder.Services.AddSingleton(Channel.CreateBounded<CookRequest>(10));
hostBuilder.Services.AddSingleton(Channel.CreateBounded<DeliveryRequest>(10));
hostBuilder.Services.AddSingleton(Channel.CreateBounded<CourierStatusUpdate>(10));

hostBuilder.Services.Configure<CourierSettings>(hostBuilder.Configuration.GetSection("Courier"));
hostBuilder.Services.Configure<ServiceBusSettings>(hostBuilder.Configuration.GetSection("ServiceBus"));
var couriers = hostBuilder.Configuration.GetSection("Courier").Get<CourierSettings>()?.Names ?? Array.Empty<string>();

hostBuilder.Services.AddPizzaShopTelemetry("PizzaShop");

//we use multiple pumps, because we don't want all channels to become unresponsive because one gets busy!!! In practice, we would
//want competing consumers here
hostBuilder.Services.AddAzureClients(clientBuilder => {
    clientBuilder.AddServiceBusClient(hostBuilder.Configuration["ServiceBus:ConnectionString"]);
});

//we have distinct queues for job accepted and job rejected to listen to each courier - all post to the same channel
foreach (var courier in couriers)
{
    //work around the problem of multiple service registration by using a singleton explicity, see https://github.com/dotnet/runtime/issues/38751
    hostBuilder.Services.AddSingleton<IHostedService, AsbMessagePumpService<JobAccepted>>(serviceProvider => AddHostedJobAcceptedService(courier, serviceProvider));
    hostBuilder.Services.AddSingleton<IHostedService, AsbMessagePumpService<JobRejected>>(serviceProvider => AddHostedJobRejectedService(courier, serviceProvider));
}

hostBuilder.Services.AddHostedService(AddHostedOrderService);

//We use channels for our internal pipeline. Channels let us easily wait on work without synchronization primitives
hostBuilder.Services.AddHostedService(AddHostedKitchenService);
hostBuilder.Services.AddHostedService(AddHostedDispatcherService);


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