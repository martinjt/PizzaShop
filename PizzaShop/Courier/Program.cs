using System.Threading.Channels;
using Confluent.Kafka;
using Courier;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Identity.Client;
using OpenTelemetry.Trace;
using Shared;
using static Courier.ServiceSetupHelpers;

var hostBuilder = new HostApplicationBuilder();

hostBuilder.Services.AddPizzaShopTelemetry("Courier", 
   additionalResourceAttributes: new Dictionary<string, string> { 
    { "pizza.courier", hostBuilder.Configuration.GetValue<string>("Courier:Name") ?? "unknown-courier" } 
    });

hostBuilder.Services.AddAzureClients(clientBuilder => {
    clientBuilder.AddServiceBusClient(hostBuilder.Configuration["ServiceBus:ConnectionString"]);
});

hostBuilder.Services.AddSingleton(Channel.CreateBounded<OrderStatus>(10));

hostBuilder.Services.AddHostedService(serviceProvider => AddHostedAvailabilityRequestService(hostBuilder.Configuration, serviceProvider));
hostBuilder.Services.AddHostedService(serviceProvider => AddHostedOrderReadyService(hostBuilder.Configuration, serviceProvider));
hostBuilder.Services.AddHostedService(serviceProvider => AddHostedDispatcherService(hostBuilder.Configuration, serviceProvider));

hostBuilder.AddKafkaProducer<int, string>("courier-order-status");

await hostBuilder.Build().RunAsync();