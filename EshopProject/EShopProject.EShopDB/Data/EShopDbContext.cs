using EShopProject.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace EShopProject.EShopDB.Data;

/// <summary>
/// Database context for E-Shop application
/// </summary>
public class EShopDbContext : DbContext
{
    public EShopDbContext(DbContextOptions<EShopDbContext> options) : base(options)
    {
    }

    public DbSet<Product> Products { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Product entity
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(e => e.ImageUrl)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(e => e.Price)
                .HasPrecision(18, 2);

            entity.Property(e => e.Description)
                .HasMaxLength(2000);

            entity.Property(e => e.Quantity)
                .IsRequired();

            entity.Property(e => e.Idate)
                .IsRequired();
        });

        // Initial seed
        SeedData(modelBuilder);
    }

    private void SeedData(ModelBuilder modelBuilder)
    {
        var products = new[]
        {
            new Product
            {
                Id = 1,
                Name = "Gaming Laptop Pro X15",
                ImageUrl = "https://images.unsplash.com/photo-1603302576837-37561b2e2302",
                Price = 34999.99m,
                Description = "High-performance gaming laptop with RTX 4070, 32GB RAM, and 1TB SSD",
                Quantity = 15,
                Idate = new DateTime(2025, 10, 21, 14, 0, 0, DateTimeKind.Utc).AddDays(-30),
                Udate = new DateTime(2025, 10, 21, 14, 0, 0, DateTimeKind.Utc).AddDays(-30)
            },
            new Product
            {
                Id = 2,
                Name = "Wireless Bluetooth Headphones",
                ImageUrl = "https://images.unsplash.com/photo-1505740420928-5e560c06d30e",
                Price = 2499.00m,
                Description = "Premium noise-cancelling headphones with 30-hour battery life",
                Quantity = 45,
                Idate = new DateTime(2025, 10, 21, 14, 0, 0, DateTimeKind.Utc).AddDays(-25),
                Udate = new DateTime(2025, 10, 21, 14, 0, 0, DateTimeKind.Utc).AddDays(-25)
            },
            new Product
            {
                Id = 3,
                Name = "4K Ultra HD Monitor 32\"",
                ImageUrl = "https://images.unsplash.com/photo-1527443224154-c4a3942d3acf",
                Price = 12999.00m,
                Description = "Professional 4K monitor with HDR support and 144Hz refresh rate",
                Quantity = 8,
                Idate = new DateTime(2025, 10, 21, 14, 0, 0, DateTimeKind.Utc).AddDays(-20),
                Udate = new DateTime(2025, 10, 21, 14, 0, 0, DateTimeKind.Utc).AddDays(-20)
            },
            new Product
            {
                Id = 4,
                Name = "Mechanical Gaming Keyboard RGB",
                ImageUrl = "https://images.unsplash.com/photo-1587829741301-dc798b83add3",
                Price = 3299.00m,
                Description = "RGB mechanical keyboard with Cherry MX switches",
                Quantity = 32,
                Idate = new DateTime(2025, 10, 21, 14, 0, 0, DateTimeKind.Utc).AddDays(-15),
                Udate = new DateTime(2025, 10, 21, 14, 0, 0, DateTimeKind.Utc).AddDays(-15)
            },
            new Product
            {
                Id = 5,
                Name = "Smartphone Pro Max 256GB",
                ImageUrl = "https://images.unsplash.com/photo-1511707171634-5f897ff02aa9",
                Price = 28999.00m,
                Description = "Latest flagship smartphone with advanced camera system",
                Quantity = 22,
                Idate = new DateTime(2025, 10, 21, 14, 0, 0, DateTimeKind.Utc).AddDays(-10),
                Udate = new DateTime(2025, 10, 21, 14, 0, 0, DateTimeKind.Utc).AddDays(-10)
            }
        };

        modelBuilder.Entity<Product>().HasData(products);
    }
}