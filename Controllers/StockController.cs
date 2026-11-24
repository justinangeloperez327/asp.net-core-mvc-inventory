using InventoryMvc.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace InventoryMvc.Controllers;
public class StockController(InventoryDbContext db):Controller
{
 public async Task<IActionResult> Index()=>View(await db.StockMovements.Include(x=>x.Product).OrderByDescending(x=>x.Id).ToListAsync());
 public IActionResult Create()=>RedirectToAction("Index","Warehouses");
}