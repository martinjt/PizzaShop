using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Microsoft.Extensions.Configuration;
using PracticalOtel.OtelCollector.Aspire;

var builder = DistributedApplication.CreateBuilder(args);

const bool UseCollector = false;

string[] couriers = ["alice", "bob", "charlie"];
const string OrderQueueName = "store-front-order-queue";
const string ServiceBusConnectionString = "Endpoint=sb://localhost;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=SAS_KEY_VALUE;UseDevelopmentEmulator=true;";

var sql = builder.AddSqlServer("sql");
var db = sql.AddDatabase("pizza-shop-db");

var kafka = builder.AddKafka("courier-order-status", 9092)
    .WithKafkaUI();

builder.Eventing.Subscribe<ResourceReadyEvent>(kafka.Resource, async (@event, ct) =>
{
    var config = new AdminClientConfig
    {
        BootstrapServers = await kafka.Resource.ConnectionStringExpression.GetValueAsync(ct)
    };

    using var adminClient = new AdminClientBuilder(config).Build();

    var topics = new TopicSpecification[3];
    
    for (int i =0; i<3;i ++ )
    {
        topics[i] = new TopicSpecification { Name = couriers[i] + "-order-status", NumPartitions = 1, ReplicationFactor = 1 };
    }

    try
    {
        await adminClient.CreateTopicsAsync(topics);
    }
    catch (CreateTopicsException e)
    {
        Console.WriteLine($"An error occurred creating topic: {e.Message}");
        throw;
    }
});

var storefront = builder.AddProject<Projects.StoreFront>("store-front")
    .WithReference(db)
    .WaitFor(db)
    .WithEnvironment("ServiceBus__OrderQueueName", OrderQueueName)
    .WithEnvironment("ConnectionStrings__ServiceBus", ServiceBusConnectionString);

var pizzashop = builder.AddProject<Projects.PizzaShop>("pizza-shop")
    .WithEnvironment("ServiceBus__OrderQueueName", OrderQueueName)
    .WithEnvironment("ConnectionStrings__ServiceBus", ServiceBusConnectionString);

var storeFrontWorker = builder.AddProject<Projects.StoreFrontWorker>("store-front-worker")
    .WithReference(db)
    .WaitFor(db)
    .WithReference(kafka)
    .WaitFor(kafka);

for (int i = 0; i < couriers.Length; i++)
{
    pizzashop.WithEnvironment($"Courier__Names__{i}", couriers[i]);
    storefront.WithEnvironment($"Courier__Names__{i}", couriers[i]);
    storeFrontWorker.WithEnvironment($"Courier__Names__{i}", couriers[i]);

    builder.AddProject<Projects.Courier>("courier-" + i)
        .WithReference(kafka)
        .WaitFor(kafka)
        .WithEnvironment("ConnectionStrings__ServiceBus",ServiceBusConnectionString)
        .WithEnvironment("Courier__Name", couriers[i]);
}

if (UseCollector)
{
    var collector = builder.AddOpenTelemetryCollector("otel-collector", "./config/collector-config.yaml")
        .WithArgs($"--config=/etc/otelcol-contrib/config.yaml")
        .WithAppForwarding();

    collector.AddConfig("./config/collector-config-receive-filters.yaml");

    if (builder.Configuration.GetValue<string>("HONEYCOMB_API_KEY") is string apiKey
        && !string.IsNullOrWhiteSpace(apiKey))
    {
        collector.AddConfig("./config/collector-config-with-honeycomb.yaml")
            .WithEnvironment("HONEYCOMB_API_KEY", apiKey);
    }
}

builder.Build().Run();

public static class CollectorExtensions
{
    public static IResourceBuilder<CollectorResource> AddConfig(this IResourceBuilder<CollectorResource> builder, string configPath)
    {
        var configFileInfo = new FileInfo(configPath);
        return builder.WithBindMount(configPath, $"/etc/otelcol-contrib/{configFileInfo.Name}")
            .WithArgs($"--config=/etc/otelcol-contrib/{configFileInfo.Name}");
    }
}