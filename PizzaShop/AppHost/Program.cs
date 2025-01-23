using Confluent.Kafka;
using Confluent.Kafka.Admin;

var builder = DistributedApplication.CreateBuilder(args);

string[] couriers = ["alice", "bob", "charlie"];
const string OrderQueueName = "store-front-order-queue";
const string ServiceBusConnectionString = "Endpoint=sb://localhost;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=SAS_KEY_VALUE;UseDevelopmentEmulator=true;";

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
    .WithReference(kafka)
    .WithEnvironment("ServiceBus__OrderQueueName", OrderQueueName)
    .WithEnvironment("ServiceBus__ConnectionString", ServiceBusConnectionString);

var pizzashop = builder.AddProject<Projects.PizzaShop>("pizza-shop")
    .WithEnvironment("ServiceBus__OrderQueueName", OrderQueueName)
    .WithEnvironment("ServiceBus__ConnectionString", ServiceBusConnectionString);

for (int i = 0; i < couriers.Length; i++)
{
    pizzashop.WithEnvironment($"Courier__Names__{i}", couriers[i]);
    storefront.WithEnvironment($"Courier__Names__{i}", couriers[i]);

    builder.AddProject<Projects.Courier>("courier-" + i)
        .WithReference(kafka)
        .WithEnvironment("ServiceBus__ConnectionString", ServiceBusConnectionString)
        .WithEnvironment("Courier__Name", couriers[i]);
}

builder.Build().Run();