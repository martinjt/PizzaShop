var builder = DistributedApplication.CreateBuilder(args);

const string orderQueueName = "order_queue";
const string deliveryManifestQueueName = "delivery_manifest_queue";

string[] courierNames = ["bob", "alice", "charlie"];

var serviceBus = builder.AddAzureServiceBus("pizza-shop-service-bus")
    .RunAsEmulator()
    .WithQueue(orderQueueName)
    .WithQueue(deliveryManifestQueueName);

foreach (var courierName in courierNames)
{
    serviceBus.WithQueue(courierName + "-availability");
    serviceBus.WithQueue(courierName + "-order-ready");
}

var kafka = builder.AddKafka("messaging")
    .WithKafkaUI();

builder.AddProject<Projects.StoreFront>("store-front")
    .WithReference(serviceBus);

var pizzashop = builder.AddProject<Projects.PizzaShop>("pizza-shop")
    .WithReference(serviceBus).WaitFor(serviceBus)
    .WithEnvironment("ServiceBus:OrderQueueName", orderQueueName)
    .WithEnvironment("ServiceBus:DeliveryManifestQueueName", deliveryManifestQueueName);

for (int index = 0; index < courierNames.Length; index++)
{
    pizzashop.WithEnvironment($"CourierSettings__Names__{index}", courierNames[index]);

    builder.AddProject<Projects.Courier>("courier-" + courierNames[index])
        .WithEnvironment("Courier__Name", courierNames[index])
        .WithReference(serviceBus)
        .WaitFor(serviceBus)
        .WithReference(kafka);
}

builder.Build().Run();