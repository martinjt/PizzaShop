using Microsoft.EntityFrameworkCore;

namespace StoreFront;

public class PizzaShopDb(DbContextOptions<PizzaShopDb> options) : DbContext(options)
{
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<Topping> Toppings => Set<Topping>();
}