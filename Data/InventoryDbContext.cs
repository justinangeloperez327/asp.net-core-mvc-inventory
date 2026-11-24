using InventoryMvc.Models;
using Microsoft.EntityFrameworkCore;
namespace InventoryMvc.Data;
public class InventoryDbContext(DbContextOptions<InventoryDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();
    public DbSet<StockMovement> StockMovements => Set<StockMovement>();
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<StockMovement>().HasOne(x=>x.Product).WithMany().HasForeignKey(x=>x.ProductId).OnDelete(DeleteBehavior.Restrict);
        modelBuilder.Entity<Product>().HasIndex(p => p.Sku).IsUnique();
        modelBuilder.Entity<Product>().Property(p => p.UnitPrice).HasPrecision(18, 2);
    }
}