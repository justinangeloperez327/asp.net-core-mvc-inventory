using InventoryMvc.Data;
using InventoryMvc.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace InventoryMvc.Controllers;
public class MasterDataController(InventoryDbContext db):Controller
{
 private static readonly string[] Kinds=["Category","Supplier","Warehouse"];
 public async Task<IActionResult> Index()=>View(await db.Set<MasterData>().OrderBy(x=>x.Kind).ThenBy(x=>x.Name).ToListAsync());
 [HttpPost,ValidateAntiForgeryToken]
 public async Task<IActionResult> Save([Bind("Id,Kind,Name,Notes")] MasterData input)
 {
 if(!Kinds.Contains(input.Kind)||!ModelState.IsValid)return BadRequest("Invalid master data.");
 input.Name=input.Name.Trim();
 if(input.Name.Length==0||await db.Set<MasterData>().AnyAsync(x=>x.Kind==input.Kind&&x.Name==input.Name&&x.Id!=input.Id))return BadRequest("Name is required and must be unique.");
 if(input.Id==0)db.Add(input);
 else {var item=await db.Set<MasterData>().FindAsync(input.Id);if(item==null||item.Kind!=input.Kind)return NotFound();item.Name=input.Name;item.Notes=input.Notes;}
 await db.SaveChangesAsync();return RedirectToAction(nameof(Index));
 }
}