using System.Threading.Channels;
using Courier;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shared;

var hostBuilder = new HostApplicationBuilder();

hostBuilder.Services.AddPizzaShopTelemetry("Courier", 
   additionalResourceAttributes: new Dictionary<string, string> { 
    { "pizza.courier", hostBuilder.Configuration.GetValue<string>("Courier:Name") ?? "unknown-courier" } 
    });

hostBuilder.Services.AddAzureClients(clientBuilder => {
    clientBuilder.AddServiceBusClient(hostBuilder.Configuration["ServiceBus:ConnectionString"]);
});

hostBuilder.Services.AddSingleton(Channel.CreateBounded<OrderStatus>(10));

hostBuilder.Services.AddHostedService(serviceProvider =>AvailabilityServiceFactory. Create(hostBuilder.Configuration, serviceProvider));
hostBuilder.Services.AddHostedService(serviceProvider => OrderReadyServiceFactory.Create(hostBuilder.Configuration, serviceProvider));
hostBuilder.Services.AddHostedService(serviceProvider => DispatcherServiceFactory.Create(hostBuilder.Configuration, serviceProvider));

hostBuilder.AddKafkaProducer<int, string>("courier-order-status");

await hostBuilder.Build().RunAsync();