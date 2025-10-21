using EShopProject.Business.Entities;
using Microsoft.EntityFrameworkCore;

namespace EShopProject.Infrastructure.Data;

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
    }
}