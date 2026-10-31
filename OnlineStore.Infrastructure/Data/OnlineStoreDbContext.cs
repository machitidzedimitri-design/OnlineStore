using Microsoft.EntityFrameworkCore;
using OnlineStore.Core.Entities;
using OnlineStore.Core.Interfaces;

namespace OnlineStore.Infrastructure.Data;

public class OnlineStoreDbContext : DbContext, IUnitOfWork
{
    public OnlineStoreDbContext(DbContextOptions<OnlineStoreDbContext> options)
        : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>(e =>
        {
            e.ToTable("Categories");
            e.HasKey(c => c.Id);
            e.Property(c => c.Name).IsRequired().HasMaxLength(100);
            e.Property(c => c.Description).IsRequired().HasMaxLength(300);

            e.HasData(
                new Category { Id = 1, Name = "Computers", Description = "Laptops, desktops and monitors" },
                new Category { Id = 2, Name = "Phones", Description = "Smartphones and mobile devices" },
                new Category { Id = 3, Name = "Accessories", Description = "Headphones, cables, chargers and more" });
        });

        modelBuilder.Entity<Product>(e =>
        {
            e.ToTable("Products", t =>
            {
                t.HasCheckConstraint("CK_Products_Price", "[Price] > 0");
                t.HasCheckConstraint("CK_Products_Stock", "[StockQuantity] >= 0");
            });
            e.HasKey(p => p.Id);
            e.Property(p => p.Name).IsRequired().HasMaxLength(100);
            e.Property(p => p.Description).IsRequired().HasMaxLength(500);
            e.Property(p => p.Price).HasPrecision(18, 2);

            e.HasOne(p => p.Category)
             .WithMany(c => c.Products)
             .HasForeignKey(p => p.CategoryId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Customer>(e =>
        {
            e.ToTable("Customers");
            e.HasKey(c => c.Id);
            e.Property(c => c.FirstName).IsRequired().HasMaxLength(50);
            e.Property(c => c.LastName).IsRequired().HasMaxLength(50);
            e.Property(c => c.Email).IsRequired().HasMaxLength(150);
            e.Property(c => c.Phone).HasMaxLength(30);
            e.HasIndex(c => c.Email).IsUnique();
        });

        modelBuilder.Entity<Order>(e =>
        {
            e.ToTable("Orders");
            e.HasKey(o => o.Id);
            e.Property(o => o.OrderDate).HasColumnType("datetime2");
            e.Property(o => o.Status).IsRequired().HasMaxLength(30);
            e.Property(o => o.TotalAmount).HasPrecision(18, 2);

            e.HasOne(o => o.Customer)
             .WithMany(c => c.Orders)
             .HasForeignKey(o => o.CustomerId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<OrderItem>(e =>
        {
            e.ToTable("OrderItems", t => t.HasCheckConstraint("CK_OrderItems_Quantity", "[Quantity] > 0"));
            e.HasKey(i => i.Id);
            e.Property(i => i.UnitPrice).HasPrecision(18, 2);

            e.HasOne(i => i.Order)
             .WithMany(o => o.Items)
             .HasForeignKey(i => i.OrderId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(i => i.Product)
             .WithMany(p => p.OrderItems)
             .HasForeignKey(i => i.ProductId)
             .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
