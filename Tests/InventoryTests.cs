using InventoryMvc.Data;
using InventoryMvc.Models;
using InventoryMvc.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using InventoryMvc.Controllers;
using Xunit;
public class InventoryTests {
 [Fact] public async Task StockReceiptAndTransferPreserveTotals(){
 await using var connection=new SqliteConnection("Data Source=:memory:");await connection.OpenAsync();
 await using var db=new InventoryDbContext(new DbContextOptionsBuilder<InventoryDbContext>().UseSqlite(connection).Options);
 await db.Database.EnsureCreatedAsync();
 var p=new Product{Name="Test",Sku="T"};var a=new MasterData{Kind="Warehouse",Name="A"};var b=new MasterData{Kind="Warehouse",Name="B"};db.AddRange(p,a,b);await db.SaveChangesAsync();
 var service=new WarehouseService(db);
 await service.ApplyAsync(new WarehouseInput{ProductId=p.Id,WarehouseId=a.Id,Quantity=10,Operation="In",Reason="Receipt"});await db.SaveChangesAsync();
 await service.ApplyAsync(new WarehouseInput{ProductId=p.Id,WarehouseId=a.Id,DestinationId=b.Id,Quantity=4,Operation="Transfer",Reason="Transfer"});await db.SaveChangesAsync();
 Assert.Equal(10,p.Quantity);Assert.Equal(6,(await db.WarehouseBalances.SingleAsync(x=>x.WarehouseId==a.Id)).Quantity);Assert.Equal(4,(await db.WarehouseBalances.SingleAsync(x=>x.WarehouseId==b.Id)).Quantity);
 Assert.Equal(2,await db.StockMovements.CountAsync());Assert.True(await db.AuditEntries.AnyAsync(x=>x.Record.StartsWith("StockMovement:")));
 await Assert.ThrowsAsync<InvalidOperationException>(()=>service.ApplyAsync(new WarehouseInput{ProductId=p.Id,WarehouseId=a.Id,Quantity=7,Operation="Out",Reason="Invalid"}));
 }
 [Theory][InlineData(typeof(UsersController))][InlineData(typeof(AuditController))]
 public void AdminControllersRequireAdmin(Type type){var attribute=(AuthorizeAttribute)Attribute.GetCustomAttribute(type,typeof(AuthorizeAttribute))!;Assert.Equal("Admin",attribute.Roles);}
}