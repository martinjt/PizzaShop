using System.Threading.Channels;
using Confluent.Kafka;
using Courier;
using KafkaGateway;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Trace;
using Shared;

var hostBuilder = new HostApplicationBuilder();

hostBuilder.Services.AddPizzaShopTelemetry("Courier", 
   additionalResourceAttributes: new Dictionary<string, string> { 
    { "pizza.courier", hostBuilder.Configuration.GetValue<string>("Courier:Name") ?? "unknown-courier" } 
    });

hostBuilder.Services.AddAzureClients(clientBuilder => {
    clientBuilder.AddServiceBusClient(hostBuilder.Configuration["ServiceBus:ConnectionString"]);
});

hostBuilder.Services.AddSingleton(KafkaProducerFactory<int, string>
    .Create("localhost:9092")
    .AsInstrumentedProducerBuilder());
hostBuilder.Services.AddTransient(serviceProvider => 
    serviceProvider.GetRequiredService<InstrumentedProducerBuilder<int, string>>().Build());

hostBuilder.Services.AddOpenTelemetry().WithTracing(builder => builder.AddKafkaProducerInstrumentation<int, string>());

hostBuilder.Services.AddSingleton(Channel.CreateBounded<OrderStatus>(10));

hostBuilder.Services.AddHostedService(serviceProvider => AvailabilityServiceFactory.Create(hostBuilder.Configuration, serviceProvider));
hostBuilder.Services.AddHostedService(serviceProvider => OrderReadyServiceFactory.Create(hostBuilder.Configuration, serviceProvider));
hostBuilder.Services.AddHostedService(serviceProvider => DispatcherServiceFactory.Create(hostBuilder.Configuration, serviceProvider));

//only need the one producer for the order status updates

await hostBuilder.Build().RunAsync();