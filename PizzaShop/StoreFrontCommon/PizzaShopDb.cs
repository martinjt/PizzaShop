using Microsoft.EntityFrameworkCore;

namespace StoreFrontCommon;

public class PizzaShopDb(DbContextOptions<PizzaShopDb> options) : DbContext(options)
{
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<Topping> Toppings => Set<Topping>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PizzaTopping>().HasKey(pst => new { pst.PizzaId, pst.ToppingId });
        modelBuilder.Entity<PizzaTopping>().HasOne<Pizza>().WithMany(ps => ps.Toppings);
        modelBuilder.Entity<PizzaTopping>().HasOne(pst => pst.Topping).WithMany();

        modelBuilder.Entity<Order>().OwnsOne(o => o.DeliveryAddress);
        
        modelBuilder.Entity<Address>()
            .Property(a => a.Name)
            .HasMaxLength(100);

        modelBuilder.Entity<Address>()
            .Property(a => a.HouseNumer)
            .HasMaxLength(50);

        modelBuilder.Entity<Address>()
            .Property(a => a.City)
            .HasMaxLength(100);

        modelBuilder.Entity<Address>()
            .Property(a => a.PostalCode)
            .HasMaxLength(20);
        
        modelBuilder.Entity<Topping>()
            .Property(t => t.Price)
            .HasColumnType("decimal(18,2)");

        base.OnModelCreating(modelBuilder);
    }
}