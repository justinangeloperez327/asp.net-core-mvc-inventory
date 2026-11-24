using InventoryMvc.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text;
namespace InventoryMvc.Controllers;
public class ReportsController(InventoryDbContext db):Controller
{
 public IActionResult Index()=>View();
 public async Task<IActionResult> Stock()
 {
 var rows=await db.Products.OrderBy(x=>x.Sku).ToListAsync();
 var csv=new StringBuilder("SKU,Name,Category,Quantity,UnitPrice,Value\r\n");
 foreach(var x in rows)csv.AppendLine(string.Join(",",Cell(x.Sku),Cell(x.Name),Cell(x.Category??""),x.Quantity.ToString(CultureInfo.InvariantCulture),x.UnitPrice.ToString(CultureInfo.InvariantCulture),(x.Quantity*x.UnitPrice).ToString(CultureInfo.InvariantCulture)));
 return File(Encoding.UTF8.GetBytes(csv.ToString()),"text/csv","stock.csv");
 }
 public async Task<IActionResult> Movements()
 {
 var rows=await db.StockMovements.Include(x=>x.Product).OrderBy(x=>x.Id).ToListAsync();
 var csv=new StringBuilder("DateUTC,SKU,Delta,Balance,Reason\r\n");
 foreach(var x in rows)csv.AppendLine(string.Join(",",x.CreatedAtUtc.ToString("O"),Cell(x.Product.Sku),x.Delta.ToString(CultureInfo.InvariantCulture),x.BalanceAfter.ToString(CultureInfo.InvariantCulture),Cell(x.Reason)));
 return File(Encoding.UTF8.GetBytes(csv.ToString()),"text/csv","movements.csv");
 }
 private static string Cell(string value)
 {
 if(value.TrimStart().StartsWith("=")||value.TrimStart().StartsWith("+")||value.TrimStart().StartsWith("-")||value.TrimStart().StartsWith("@")||value.StartsWith("\t")||value.StartsWith("\r"))value="'"+value;
 return "\"" + value.Replace("\"", "\"\"") + "\"";
 }
}