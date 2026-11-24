using InventoryMvc.Data;
using InventoryMvc.Models;
using Microsoft.EntityFrameworkCore;
namespace InventoryMvc.Services;
public class WarehouseService(InventoryDbContext db)
{
 // Caller owns the transaction and commits after saving all related records.
 public async Task ApplyAsync(WarehouseInput input)
 {
 if(input.Quantity<=0||input.Operation is not ("In" or "Out" or "Transfer"))throw new InvalidOperationException("Invalid movement.");
 var product=await db.Products.FindAsync(input.ProductId)??throw new InvalidOperationException("Unknown product.");
 if(!await db.Set<MasterData>().AnyAsync(x=>x.Id==input.WarehouseId&&x.Kind=="Warehouse"))throw new InvalidOperationException("Unknown warehouse.");
 var source=await Balance(input.WarehouseId,input.ProductId);
 int delta=input.Operation=="In"?input.Quantity:-input.Quantity;
 long next=(long)source.Quantity+delta;
 if(next<0||next>int.MaxValue)throw new InvalidOperationException("Insufficient stock or balance overflow.");
 if(input.Operation=="Transfer")
 {
 if(input.DestinationId is not int destination||destination==input.WarehouseId||!await db.Set<MasterData>().AnyAsync(x=>x.Id==destination&&x.Kind=="Warehouse"))throw new InvalidOperationException("Invalid destination.");
 var target=await Balance(destination,input.ProductId);
 if((long)target.Quantity+input.Quantity>int.MaxValue)throw new InvalidOperationException("Destination overflow.");
 target.Quantity+=input.Quantity;
 delta=0;
 }
 long total=(long)product.Quantity+delta;
 if(total<0||total>int.MaxValue)throw new InvalidOperationException("Invalid total stock.");
 source.Quantity=(int)next;product.Quantity=(int)total;product.UpdatedAtUtc=DateTime.UtcNow;
 var reason=$"{input.Operation} warehouse {input.WarehouseId} to {input.DestinationId}: {input.Reason}";
 db.StockMovements.Add(new StockMovement{ProductId=product.Id,Delta=delta,BalanceAfter=product.Quantity,Reason=reason[..Math.Min(200,reason.Length)]});
 }
 private async Task<WarehouseBalance> Balance(int warehouse,int product)
 {
 var item=await db.Set<WarehouseBalance>().SingleOrDefaultAsync(x=>x.WarehouseId==warehouse&&x.ProductId==product);
 if(item!=null)return item;
 item=new WarehouseBalance{WarehouseId=warehouse,ProductId=product};db.Add(item);return item;
 }
}