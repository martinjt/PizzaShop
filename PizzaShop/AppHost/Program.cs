var builder = DistributedApplication.CreateBuilder(args);

var kafka = builder.AddKafka("courier-order-status", 9092)
    .WithKafkaUI();

builder.AddProject<Projects.StoreFront>("store-front")
    .WithReference(kafka);

builder.AddProject<Projects.PizzaShop>("pizza-shop");

builder.AddProject<Projects.Courier>("courier")
    .WithReference(kafka);

builder.Build().Run();