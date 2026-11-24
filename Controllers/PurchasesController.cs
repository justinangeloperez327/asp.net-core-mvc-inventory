using InventoryMvc.Data;
using InventoryMvc.Models;
using InventoryMvc.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace InventoryMvc.Controllers;
public class PurchasesController(InventoryDbContext db,WarehouseService service):Controller
{
 public async Task<IActionResult> Index()
 {
 ViewBag.Products=await db.Products.ToListAsync();ViewBag.Master=await db.Set<MasterData>().ToListAsync();
 return View(await db.Set<PurchaseOrder>().OrderByDescending(x=>x.Id).ToListAsync());
 }
 [HttpPost,ValidateAntiForgeryToken]
 public async Task<IActionResult> Create([Bind("SupplierId,ProductId,WarehouseId,Quantity,UnitCost")] PurchaseOrder input)
 {
 if(!ModelState.IsValid||!await db.Products.AnyAsync(x=>x.Id==input.ProductId)||!await db.Set<MasterData>().AnyAsync(x=>x.Id==input.SupplierId&&x.Kind=="Supplier")||!await db.Set<MasterData>().AnyAsync(x=>x.Id==input.WarehouseId&&x.Kind=="Warehouse"))return BadRequest("Invalid order.");
 db.Add(input);await db.SaveChangesAsync();return RedirectToAction(nameof(Index));
 }
 [HttpPost,ValidateAntiForgeryToken]
 public async Task<IActionResult> Transition(int id,string actionName)
 {
 await using var tx=await db.Database.BeginTransactionAsync();
 var order=await db.Set<PurchaseOrder>().FindAsync(id);if(order==null)return NotFound();
 if(actionName=="Approve"&&order.Status=="Draft")order.Status="Approved";
 else if(actionName=="Cancel"&&order.Status is "Draft" or "Approved")order.Status="Cancelled";
 else if(actionName=="Receive"&&order.Status=="Approved")
 {
 try{await service.ApplyAsync(new WarehouseInput{ProductId=order.ProductId,WarehouseId=order.WarehouseId,Quantity=order.Quantity,Operation="In",Reason=$"Purchase order {id}"});}
 catch(InvalidOperationException e){return BadRequest(e.Message);}
 order.Status="Received";
 }
 else return BadRequest("Invalid status transition.");
 await db.SaveChangesAsync();await tx.CommitAsync();return RedirectToAction(nameof(Index));
 }
}