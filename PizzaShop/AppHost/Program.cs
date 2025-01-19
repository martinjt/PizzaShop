var builder = DistributedApplication.CreateBuilder(args);

//we want to use the connection string to allow us to use ASB local
var serviceBus = builder.AddConnectionString("pizza_shop_bus");

var kafka = builder.AddKafka("kafka")
    .WithKafkaUI();

builder.AddProject<Projects.StoreFront>("store_front")
    .WithReference(serviceBus);

builder.AddProject<Projects.PizzaShop>("pizza_shop")
    .WithReference(serviceBus);

builder.AddProject<Projects.Courier>("courier")
    .WithReference(serviceBus);

builder.Build().Run();