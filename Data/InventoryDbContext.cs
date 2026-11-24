using InventoryMvc.Models;
using Microsoft.EntityFrameworkCore;
namespace InventoryMvc.Data;
public class InventoryDbContext(DbContextOptions<InventoryDbContext> options):DbContext(options)
{
 public DbSet<Product> Products=>Set<Product>();
 public DbSet<StockMovement> StockMovements=>Set<StockMovement>();
 public DbSet<MasterData> MasterRecords=>Set<MasterData>();
 public DbSet<WarehouseBalance> WarehouseBalances=>Set<WarehouseBalance>();
 public DbSet<PurchaseOrder> PurchaseOrders=>Set<PurchaseOrder>();
 protected override void OnModelCreating(ModelBuilder b)
 {
 b.Entity<StockMovement>().HasOne(x=>x.Product).WithMany().HasForeignKey(x=>x.ProductId).OnDelete(DeleteBehavior.Restrict);
 b.Entity<Product>().HasIndex(x=>x.Sku).IsUnique();
 b.Entity<MasterData>().HasIndex(x=>new{x.Kind,x.Name}).IsUnique();
 b.Entity<WarehouseBalance>().HasIndex(x=>new{x.WarehouseId,x.ProductId}).IsUnique();
 b.Entity<WarehouseBalance>().HasOne<Product>().WithMany().HasForeignKey(x=>x.ProductId).OnDelete(DeleteBehavior.Restrict);
 b.Entity<PurchaseOrder>().HasOne<Product>().WithMany().HasForeignKey(x=>x.ProductId).OnDelete(DeleteBehavior.Restrict);
 }
}