using InventoryMvc.Models;
using Microsoft.EntityFrameworkCore;
namespace InventoryMvc.Data;
public class InventoryDbContext(DbContextOptions<InventoryDbContext> options, Microsoft.AspNetCore.Http.IHttpContextAccessor? accessor=null):DbContext(options)
{
 public DbSet<Product> Products=>Set<Product>();
 public DbSet<StockMovement> StockMovements=>Set<StockMovement>();
 public DbSet<MasterData> MasterRecords=>Set<MasterData>();
 public DbSet<WarehouseBalance> WarehouseBalances=>Set<WarehouseBalance>();
 public DbSet<PurchaseOrder> PurchaseOrders=>Set<PurchaseOrder>();
 public DbSet<InventoryUser> Users=>Set<InventoryUser>();
 public DbSet<AuditEntry> AuditEntries=>Set<AuditEntry>();
 public override async Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess,CancellationToken cancellationToken=default){
 var actor=accessor?.HttpContext?.User.Identity?.Name??"System";
 var changes=ChangeTracker.Entries().Where(e=>e.Entity is not AuditEntry && e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted).ToList();
 var own=Database.CurrentTransaction==null?await Database.BeginTransactionAsync(cancellationToken):null;
 try{
 var actions=changes.Select(e=>(Entry:e,Action:e.State.ToString())).ToList();
 var result=await base.SaveChangesAsync(true,cancellationToken);
 foreach(var item in actions){var key=string.Join(",",item.Entry.Properties.Where(p=>p.Metadata.IsPrimaryKey()).Select(p=>p.CurrentValue?.ToString()));AuditEntries.Add(new AuditEntry{Actor=actor,Action=item.Action,Record=item.Entry.Metadata.ClrType.Name+":"+key});}
 if(actions.Count>0)await base.SaveChangesAsync(true,cancellationToken);
 if(own!=null)await own.CommitAsync(cancellationToken);return result;
 }finally{if(own!=null)await own.DisposeAsync();}
 }
 protected override void OnModelCreating(ModelBuilder b)
 {
 b.Entity<InventoryUser>().HasIndex(x=>x.Username).IsUnique();
 b.Entity<StockMovement>().HasOne(x=>x.Product).WithMany().HasForeignKey(x=>x.ProductId).OnDelete(DeleteBehavior.Restrict);
 b.Entity<Product>().HasIndex(x=>x.Sku).IsUnique();
 b.Entity<MasterData>().HasIndex(x=>new{x.Kind,x.Name}).IsUnique();
 b.Entity<WarehouseBalance>().HasIndex(x=>new{x.WarehouseId,x.ProductId}).IsUnique();
 b.Entity<WarehouseBalance>().HasOne<Product>().WithMany().HasForeignKey(x=>x.ProductId).OnDelete(DeleteBehavior.Restrict);
 b.Entity<PurchaseOrder>().HasOne<Product>().WithMany().HasForeignKey(x=>x.ProductId).OnDelete(DeleteBehavior.Restrict);
 }
}