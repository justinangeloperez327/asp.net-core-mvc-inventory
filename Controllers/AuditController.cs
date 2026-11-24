using InventoryMvc.Data;
using InventoryMvc.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace InventoryMvc.Controllers;
[Authorize(Roles="Admin")]
public class AuditController(InventoryDbContext db):Controller {
 public async Task<IActionResult> Index()=>View(await db.Set<AuditEntry>().AsNoTracking().OrderByDescending(x=>x.Id).Take(500).ToListAsync());
}