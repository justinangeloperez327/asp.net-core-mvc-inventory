using InventoryMvc.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace InventoryMvc.Controllers;
public class DashboardController(InventoryDbContext db) : Controller
{
    public async Task<IActionResult> Index() => View(await db.Products.AsNoTracking().ToListAsync());
}