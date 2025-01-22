var builder = DistributedApplication.CreateBuilder(args);

string[] couriers = ["alice", "bob", "charlie"];
const string OrderQueueName = "store-front-order-queue";
const string ServiceBusConnectionString = "Endpoint=sb://localhost;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=SAS_KEY_VALUE;UseDevelopmentEmulator=true;";

var kafka = builder.AddKafka("courier-order-status", 9092)
    .WithKafkaUI();

builder.AddProject<Projects.StoreFront>("store-front")
    .WithReference(kafka);

var pizzashop = builder.AddProject<Projects.PizzaShop>("pizza-shop")
    .WithEnvironment("ServiceBus__OrderQueueName", OrderQueueName)
    .WithEnvironment("ServiceBus__ConnectionString", ServiceBusConnectionString);

for (int i = 0; i < couriers.Length; i++)
{
    pizzashop.WithEnvironment($"Courier__Names__{i}", couriers[i]);

    builder.AddProject<Projects.Courier>("courier-" + i)
        .WithReference(kafka)
        .WithEnvironment("Courier__Name", couriers[i]);
}

builder.AddProject<Projects.Courier>("courier")
    .WithReference(kafka);

builder.Build().Run();