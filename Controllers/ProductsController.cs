using InventoryMvc.Data;
using InventoryMvc.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace InventoryMvc.Controllers;
public class ProductsController(InventoryDbContext db) : Controller
{
    public async Task<IActionResult> Index(string? search)
    {
        var products = db.Products.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim();
            products = products.Where(p => p.Name.Contains(search) || p.Sku.Contains(search) || (p.Category != null && p.Category.Contains(search)));
        }
        ViewData["Search"] = search;
        return View(await products.OrderBy(p => p.Name).ToListAsync());
    }
    public async Task<IActionResult> Details(int id)
    {
        var product = await db.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
        return product == null ? NotFound() : View(product);
    }
    public IActionResult Create() => View(new Product());
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Name,Sku,Category,UnitPrice")] Product product)
    {
        await ValidateSkuAsync(product);
        if (!ModelState.IsValid) return View(product);
        product.UpdatedAtUtc = DateTime.UtcNow;
        db.Products.Add(product);
        await db.SaveChangesAsync();
        TempData["Success"] = "Product created.";
        return RedirectToAction(nameof(Index));
    }
    public async Task<IActionResult> Edit(int id)
    {
        var product = await db.Products.FindAsync(id);
        return product == null ? NotFound() : View(product);
    }
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Sku,Category,UnitPrice")] Product input)
    {
        if (id != input.Id) return BadRequest();
        var product = await db.Products.FindAsync(id);
        if (product == null) return NotFound();
        await ValidateSkuAsync(input, id);
        if (!ModelState.IsValid) return View(input);
        product.Name = input.Name; product.Sku = input.Sku; product.Category = input.Category;
        product.UnitPrice = input.UnitPrice; product.UpdatedAtUtc = DateTime.UtcNow;
        await db.SaveChangesAsync();
        TempData["Success"] = "Product updated.";
        return RedirectToAction(nameof(Index));
    }
    public async Task<IActionResult> Delete(int id)
    {
        var product = await db.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
        return product == null ? NotFound() : View(product);
    }
    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var product = await db.Products.FindAsync(id);
        if (product == null) return NotFound();
        if (product.Quantity != 0 || await db.StockMovements.AnyAsync(x=>x.ProductId==id)) { ModelState.AddModelError("", "Products with stock or movement history cannot be deleted."); return View("Delete", product); }
        db.Products.Remove(product);
        await db.SaveChangesAsync();
        TempData["Success"] = "Product deleted.";
        return RedirectToAction(nameof(Index));
    }
    private async Task ValidateSkuAsync(Product product, int? excludingId = null)
    {
        if (string.IsNullOrWhiteSpace(product.Sku)) return;
        product.Sku = product.Sku.Trim();
        if (await db.Products.AnyAsync(p => p.Sku == product.Sku && p.Id != excludingId))
            ModelState.AddModelError(nameof(Product.Sku), "SKU must be unique.");
    }
}