using InventoryMvc.Data;
using InventoryMvc.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
namespace InventoryMvc.Controllers;
public class StockController(InventoryDbContext db) : Controller
{
    public async Task<IActionResult> Index() => View(await db.Set<StockMovement>().Include(x=>x.Product).OrderByDescending(x=>x.Id).ToListAsync());
    public async Task<IActionResult> Create(int? productId)
    {
        await LoadProducts();
        return View(new MovementInput { ProductId = productId ?? 0 });
    }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(MovementInput input)
    {
        if (ModelState.IsValid)
        {
            await using var transaction = await db.Database.BeginTransactionAsync();
            var product = await db.Products.FindAsync(input.ProductId);
            if (product == null) ModelState.AddModelError("ProductId","Product not found.");
            else
            {
                int delta = input.Outbound ? -input.Quantity : input.Quantity;
                long balance = (long)product.Quantity + delta;
                if (balance < 0 || balance > int.MaxValue) ModelState.AddModelError("Quantity","Invalid resulting stock balance.");
                else
                {
                    product.Quantity = (int)balance;
                    product.UpdatedAtUtc = DateTime.UtcNow;
                    db.Set<StockMovement>().Add(new StockMovement { ProductId=product.Id, Delta=delta, BalanceAfter=product.Quantity, Reason=input.Reason });
                    await db.SaveChangesAsync();
                    await transaction.CommitAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
        }
        await LoadProducts();
        return View(input);
    }
    private async Task LoadProducts() => ViewBag.Products = new SelectList(await db.Products.OrderBy(x=>x.Name).ToListAsync(),"Id","Name");
}