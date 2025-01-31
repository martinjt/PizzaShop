
using Confluent.Kafka;
using KafkaGateway;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Trace;
using Shared;
using StoreFrontCommon;
using StoreFrontWorker;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddPizzaShopTelemetry("StoreFrontWorker");

var connectionString = builder.Configuration.GetConnectionString("pizza-shop-db");
builder.Services.AddDbContextFactory<PizzaShopDb>(dbContextOptionsBuilder => dbContextOptionsBuilder.UseSqlServer(connectionString));
builder.EnrichSqlServerDbContext<PizzaShopDb>(options => {
    options.DisableTracing = true;
});

builder.Services.AddSingleton(
    KafkaConsumerFactory<int, string>
        .Create("localhost:9092", "storefront-consumer-group")
        .AsInstrumentedConsumerBuilder());
builder.Services.AddTransient(serviceProvider => 
    serviceProvider.GetRequiredService<InstrumentedConsumerBuilder<int, string>>().Build());

builder.Services.AddOpenTelemetry().WithTracing(b => b.AddKafkaConsumerInstrumentation<int, string>());

// Listens to status updates about an order
// Normally, we would tend to run a Kafka worker in a separate process, so that we could scale out to the number of
// partitions we had, separate to scaling for the number of HTTP requests.
// To make this simpler, for now, we are just running it as a background process, as we don't need to scale it

var couriers = builder.Configuration.GetSection("Courier").Get<CourierSettings>()?.Names ?? [];
var topics = couriers.Select(courier => courier + "-order-status").ToArray();

//subscribe to all the topics with one pump
builder.Services.AddHostedService<KafkaMessagePumpService<int, string, OrderStatusChange>>(serviceProvider =>  AfterOrderServiceFactory.Create(topics, serviceProvider));

var host = builder.Build();
host.Run();