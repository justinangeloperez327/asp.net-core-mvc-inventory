using InventoryMvc.Models;
using Microsoft.EntityFrameworkCore;
namespace InventoryMvc.Data;
public class InventoryDbContext(DbContextOptions<InventoryDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>().HasIndex(p => p.Sku).IsUnique();
        modelBuilder.Entity<Product>().Property(p => p.UnitPrice).HasPrecision(18, 2);
    }
}