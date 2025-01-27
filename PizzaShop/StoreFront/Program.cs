using System.ComponentModel;
using System.Text.Json;
using AsbGateway;
using Azure.Messaging.ServiceBus;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Shared;
using StoreFrontCommon;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddPizzaShopTelemetry("StoreFront");

var connectionString = "DataSource=storefront;mode=memory;cache=shared";
var keepAliveConnection = new SqliteConnection(connectionString);
keepAliveConnection.Open();

builder.AddSqlServerDbContext<PizzaShopDb>(connectionName: "pizza-shop-db", settings => 
{
    settings.DisableTracing = true;
});

builder.Services.AddOpenApi();

var app = builder.Build();


var scopeFactory = app.Services.GetRequiredService<IServiceScopeFactory>();
using (var scope = scopeFactory.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PizzaShopDb>();
    db.Database.EnsureCreated();
}

app.MapOpenApi();

app.MapGet("/orders", async (PizzaShopDb db) =>
    {
        return await db.Orders
            .Include(o => o.Pizzas)
            .ToListAsync();
    })
    .WithSummary("Retrieve all orders")
    .WithDescription("The orders endpoint allows you to retrieve all orders.");

app.MapGet("/orders/pending", async (PizzaShopDb db) =>
    {
        return await db.Orders.Where(o => o.Status == DeliveryStatus.Pending)
                .Include(o => o.Pizzas)
                .Include(o => o.DeliveryAddress)
                .ToListAsync();
    })
    .WithSummary("Retrieve all pending orders")
        .WithDescription("The pending orders endpoint allows you to retrieve all orders that are currently pending.");


app.MapGet("/orders/{id}", async ([Description("The id of the order to watch")] int id, PizzaShopDb db) =>
    {
        var order =  await db.Orders
                .Where(o => o.OrderId == id)
                .Include(o => o.Pizzas)
                .Include(o => o.DeliveryAddress)
                .SingleOrDefaultAsync();
        
        return order != null
            ? Results.Ok(order)
            : Results.NotFound();
    })
    .WithSummary("Retrieve an order by its ID")
    .WithDescription("The order endpoint allows you to retrieve an order by its ID.");

app.MapPost("/orders", async ([Description("The pizza order you wish to make")]Order order, PizzaShopDb db) =>
{
    order.CreatedTime = DateTimeOffset.UtcNow;
    
    db.Orders.Add(order);
    
    //really we should use an Outbox pattern here, to save the outgoing message, but for now we will just save the order
    await db.SaveChangesAsync();

    await SendOrderAsync(order);

    //get the order we just saved
    var pendingOrder = await db.Orders
        .Where(o => o.OrderId == order.OrderId)
        .Include(o => o.Pizzas)
        .Include(o => o.DeliveryAddress)
        .SingleOrDefaultAsync();

    return Results.Accepted($"/orders/{order.OrderId}", pendingOrder);
})
.WithSummary("Allows new orders to be raised")
.WithDescription("The new orders endpoint is intended to allow orders to be raised. Orders are created with a status of 'Pending' and an ETA of 30 minutes.");

//needs a background service that updates the status of the order in response to Kafka messages, which you can see via
//polling the page returned in the 204 Accept

await app.RunAsync();

async Task SendOrderAsync(Order order)
{
    var sbConnectionString = app.Configuration.GetValue<string>("ServiceBus:ConnectionString");
    var queueName = app.Configuration.GetValue<string>("ServiceBus:OrderQueueName");
            
    if (string.IsNullOrEmpty(sbConnectionString) || string.IsNullOrEmpty(queueName))
    {
        throw new InvalidOperationException("ServiceBus:ConnectionString and ServiceBus:OrderQueueName must be set in configuration");
    }
            
    var client = new ServiceBusClient(sbConnectionString);
    var orderProducer = new AsbProducer<Order>(
        client, 
        message => new ServiceBusMessage(JsonSerializer.Serialize(message.Content)));
    await orderProducer.SendMessageAsync(queueName, new Message<Order>(order));
}

