﻿using System.Diagnostics;
using System.Threading.Channels;
using AsbGateway;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PizzaShop;
using Shared;

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

hostBuilder.Services.AddPizzaShopTelemetry("PizzaShop", additionalSources: [DiagnosticConfig.Source.Name]);

//we use multiple pumps, because we don't want all channels to become unresponsive because one gets busy!!! In practice, we would
//want competing consumers here
hostBuilder.Services.AddAzureClients(clientBuilder => {
    clientBuilder.AddServiceBusClient(hostBuilder.Configuration.GetConnectionString("ServiceBus"));
});

//we have distinct queues for job accepted and job rejected to listen to each courier - all post to the same channel
foreach (var courier in couriers)
{
    //work around the problem of multiple service registration by using a singleton explicity, see https://github.com/dotnet/runtime/issues/38751
    hostBuilder.Services.AddSingleton<IHostedService, AsbMessagePumpService<JobAccepted>>(serviceProvider => JobServiceFactory.CreateAccepted(courier, serviceProvider));
    hostBuilder.Services.AddSingleton<IHostedService, AsbMessagePumpService<JobRejected>>(serviceProvider => JobServiceFactory.CreateRejected(courier, serviceProvider));
}

hostBuilder.Services.AddHostedService(serviceProvider => OrderServiceFactory.AddHostedOrderService(serviceProvider));

//We use channels for our internal pipeline. Channels let us easily wait on work without synchronization primitives
hostBuilder.Services.AddHostedService(serviceProvider => KitchenServiceFactory.Create(serviceProvider));
hostBuilder.Services.AddHostedService(serviceProvider => DispatcherServiceFactory.Create(serviceProvider));


await hostBuilder.Build().RunAsync();

public static class DiagnosticConfig
{
    public const string ServiceName = "PizzaShop";
    public static ActivitySource Source = new (ServiceName);
}