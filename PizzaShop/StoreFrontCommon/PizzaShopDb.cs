using Microsoft.EntityFrameworkCore;

namespace StoreFrontCommon;

public class PizzaShopDb(DbContextOptions<PizzaShopDb> options) : DbContext(options)
{
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<Topping> Toppings => Set<Topping>();
}