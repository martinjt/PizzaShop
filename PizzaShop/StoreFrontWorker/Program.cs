
using KafkaGateway;
using Shared;
using StoreFrontWorker;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddPizzaShopTelemetry("StoreFrontWorker");

// Listens to status updates about an order
// Normally, we would tend to run a Kafka worker in a separate process, so that we could scale out to the number of
// partitions we had, separate to scaling for the number of HTTP requests.
// To make this simpler, for now, we are just running it as a background process, as we don't need to scale it

var couriers = builder.Configuration.GetSection("Courier").Get<CourierSettings>()?.Names ?? [];
var topics = couriers.Select(courier => courier + "-order-status").ToArray();
//subscribe to all the topics with one pump
builder.Services.AddHostedService<KafkaMessagePumpService<int, string>>(serviceProvider =>  AfterOrderServiceFactory.Create(topics, serviceProvider));

var host = builder.Build();
host.Run();