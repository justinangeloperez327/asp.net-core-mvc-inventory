using Microsoft.AspNetCore.Authorization;
using InventoryMvc.Data;
using InventoryMvc.Models;
using InventoryMvc.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace InventoryMvc.Controllers;
public class WarehousesController(InventoryDbContext db,WarehouseService service):Controller
{
 public async Task<IActionResult> Index()
 {
 ViewBag.Products=await db.Products.ToListAsync();ViewBag.Warehouses=await db.Set<MasterData>().Where(x=>x.Kind=="Warehouse").ToListAsync();
 return View(await db.Set<WarehouseBalance>().ToListAsync());
 }
 [Authorize(Roles="Admin,Operator"),HttpPost,ValidateAntiForgeryToken]
 public async Task<IActionResult> Move(WarehouseInput input)
 {
 if(!ModelState.IsValid)return BadRequest("Invalid movement.");
 await using var tx=await db.Database.BeginTransactionAsync();
 try{await service.ApplyAsync(input);await db.SaveChangesAsync();await tx.CommitAsync();}
 catch(InvalidOperationException e){return BadRequest(e.Message);}
 return RedirectToAction(nameof(Index));
 }
}