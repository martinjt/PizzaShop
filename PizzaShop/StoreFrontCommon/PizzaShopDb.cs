using Microsoft.EntityFrameworkCore;

namespace StoreFrontCommon;

public class PizzaShopDb(DbContextOptions<PizzaShopDb> options) : DbContext(options)
{
    public DbSet<Order> Orders { get; set; }
    public DbSet<Pizza> Pizzas { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>(entity =>
        {
            entity.OwnsOne(o => o.DeliveryAddress, address =>
            {
                address.Property(a => a.Name).HasMaxLength(100);
                address.Property(a => a.HouseNumer).HasMaxLength(50);
                address.Property(a => a.City).HasMaxLength(100);
                address.Property(a => a.PostalCode).HasMaxLength(20);
            });
        });
        
        base.OnModelCreating(modelBuilder);
    }
}