using System.ComponentModel;
using Microsoft.EntityFrameworkCore;
using StoreFront;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<PizzaShopDb>(opt => opt.UseInMemoryDatabase("TodoList"));

builder.Services.AddOpenApi();

var app = builder.Build();

var scopeFactory = app.Services.GetRequiredService<IServiceScopeFactory>();
using (var scope = scopeFactory.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PizzaShopDb>();
    if (db.Database.EnsureCreated())
    {
        MenuMaker.CreateToppings(db);
    }
}

app.MapOpenApi();

app.MapGet("/orders", async (PizzaShopDb db) => await db.Orders.ToListAsync())
    .WithSummary("Retrieve all orders")
    .WithDescription("The orders endpoint allows you to retrieve all orders.");

app.MapGet("/orders/pending", async (PizzaShopDb db) => 
    await db.Orders.Where(o => o.Status == OrderStatus.Pending).ToListAsync())
    .WithSummary("Retrieve all pending orders")
    .WithDescription("The pending orders endpoint allows you to retrieve all orders that are currently pending.");

app.MapGet("/orders/{id}", async ([Description("The id of the order to watch")]int id, PizzaShopDb db) => 
    await db.Orders.FindAsync(id)
        is Order order
            ? Results.Ok(order)
            : Results.NotFound())
    .WithSummary("Retrieve an order by its ID")
    .WithDescription("The order endpoint allows you to retrieve an order by its ID.");

app.MapPost("/orders", async ([Description("The pizza order you wish to make")]Order order, PizzaShopDb db) =>
{
    order.CreatedTime = DateTimeOffset.UtcNow;
    db.Orders.Add(order);
    await db.SaveChangesAsync();

    return Results.Accepted($"/orders/{order.Id}", order);
})
.WithSummary("Allows new orders to be raised")
.WithDescription("The new orders endpoint is intended to allow orders to be raised. Orders are created with a status of 'Pending' and an ETA of 30 minutes.");

app.MapGet("/toppings", async (PizzaShopDb db) => await db.Toppings.ToListAsync())
    .WithSummary("Retrieve all toppings")
    .WithDescription("The toppings endpoint allows you to retrieve all toppings.");

//needs a background service that updates the status of the order in response to Kafka messages, which you can see via
//polling the page returned in the 204 Accept

await app.RunAsync();