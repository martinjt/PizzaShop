using System.ComponentModel;
using KafkaGateway;
using Microsoft.EntityFrameworkCore;
using StoreFront;
using StoreFront.Seed;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<PizzaShopDb>(opt => opt.UseInMemoryDatabase("TodoList"));
//builder.AddKafkaConsumer<int, string>("after-order");

// Listens to status updates about an order
// Normally, we would tend to run a Kafka worker in a separate process, so that we could scale out to the number of
// partitions we had, separate to scaling for the number of HTTP requests.
// To make this simpler, for now, we are just running it as a background process, as we don't need to scale it
//builder.Services.AddHostedService<KafkaMessagePumpService<OrderStatus>>();

builder.Services.AddOpenApi();

var app = builder.Build();

var scopeFactory = app.Services.GetRequiredService<IServiceScopeFactory>();
using (var scope = scopeFactory.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PizzaShopDb>();
    if (db.Database.EnsureCreated())
    {
        await MenuMaker.CreateToppings(db);
        await OrderMaker.DeliveredOrders(db);
    }
}

app.MapOpenApi();

app.MapGet("/orders", async (PizzaShopDb db) =>
    {
        return await db.Orders
            .Include(o => o.Pizzas)
            .ThenInclude(p => p.Toppings)
            .ThenInclude(t => t.Topping)
            .ToListAsync();
    })
    .WithSummary("Retrieve all orders")
    .WithDescription("The orders endpoint allows you to retrieve all orders.");

app.MapGet("/orders/pending", async (PizzaShopDb db) =>
    {
        return await db.Orders.Where(o => o.Status == OrderStatus.Pending)
                .Include(o => o.Pizzas)
                .ThenInclude(p => p.Toppings)
                .ThenInclude(t => t.Topping)
                .ToListAsync();
    })
    .WithSummary("Retrieve all pending orders")
        .WithDescription("The pending orders endpoint allows you to retrieve all orders that are currently pending.");


app.MapGet("/orders/{id}", async ([Description("The id of the order to watch")] int id, PizzaShopDb db) =>
    {
        var order =  await db.Orders
                .Where(o => o.OrderId == id)
                .Include(o => o.Pizzas)
                .ThenInclude(p => p.Toppings)
                .ThenInclude(t => t.Topping)
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
    
    //we already have the toppings, so we must attach them to the order
    foreach (var pizza in order.Pizzas)
    {
        foreach (var topping in pizza.Toppings)
        {
            topping.ToppingId = topping.Topping?.ToppingId ?? 0;
            topping.Topping = null;
        }
    }
    
    db.Orders.Attach(order);
    await db.SaveChangesAsync();
    
    //get the order we just saved
    var pendingOrder = await db.Orders
        .Where(o => o.OrderId == order.OrderId)
        .Include(o => o.Pizzas)
        .ThenInclude(p => p.Toppings)
        .ThenInclude(t => t.Topping)
        .SingleOrDefaultAsync();

    return Results.Accepted($"/orders/{order.OrderId}", pendingOrder);
})
.WithSummary("Allows new orders to be raised")
.WithDescription("The new orders endpoint is intended to allow orders to be raised. Orders are created with a status of 'Pending' and an ETA of 30 minutes.");

app.MapGet("/toppings", async (PizzaShopDb db) => await db.Toppings.ToListAsync())
    .WithSummary("Retrieve all toppings")
    .WithDescription("The toppings endpoint allows you to retrieve all toppings.");

//needs a background service that updates the status of the order in response to Kafka messages, which you can see via
//polling the page returned in the 204 Accept

await app.RunAsync();