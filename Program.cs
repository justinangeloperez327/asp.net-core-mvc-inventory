using InventoryMvc.Data;
using InventoryMvc.Models;
using InventoryMvc.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
if(args.Contains("--hash-password"))
{
 Console.Write("Password: ");var password=new System.Text.StringBuilder();
 while(true){var key=Console.ReadKey(true);if(key.Key==ConsoleKey.Enter)break;if(key.Key==ConsoleKey.Backspace){if(password.Length>0)password.Length--; }else if(!char.IsControl(key.KeyChar))password.Append(key.KeyChar);}
 Console.WriteLine();if(password.Length<12)throw new InvalidOperationException("Use at least 12 characters.");
 Console.WriteLine(new Microsoft.AspNetCore.Identity.PasswordHasher<string>().HashPassword("",password.ToString()));return;
}
var builder=WebApplication.CreateBuilder(args);
builder.Services.AddHttpContextAccessor();
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<InventoryDbContext>(o=>o.UseSqlite(builder.Configuration.GetConnectionString("Inventory")??"Data Source=inventory.db"));
builder.Services.AddScoped<WarehouseService>();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(o=>{o.LoginPath="/Account/Login";o.AccessDeniedPath="/Account/Denied";o.Cookie.HttpOnly=true;o.Cookie.SameSite=SameSiteMode.Lax;o.Events.OnValidatePrincipal=async context=>{var db=context.HttpContext.RequestServices.GetRequiredService<InventoryDbContext>();var id=context.Principal?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;var stamp=context.Principal?.FindFirst("stamp")?.Value;if(!int.TryParse(id,out var key)||!await db.Set<InventoryUser>().AnyAsync(x=>x.Id==key&&x.Active&&x.Stamp==stamp)){context.RejectPrincipal();}};});
builder.Services.AddAuthorization(o=>o.FallbackPolicy=new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());
var app=builder.Build();
if(!app.Environment.IsDevelopment()){app.UseExceptionHandler("/Home/Error");app.UseHsts();app.UseHttpsRedirection();}
app.UseStaticFiles();app.UseRouting();app.UseAuthentication();app.UseAuthorization();
using(var scope=app.Services.CreateScope())
{
 var db=scope.ServiceProvider.GetRequiredService<InventoryDbContext>();db.Database.EnsureCreated();
 await using var tx=await db.Database.BeginTransactionAsync();
 foreach(var sql in new[]{
 "CREATE TABLE IF NOT EXISTS Users (Id INTEGER PRIMARY KEY AUTOINCREMENT, Username TEXT NOT NULL, PasswordHash TEXT NOT NULL, Role TEXT NOT NULL, Active INTEGER NOT NULL, Stamp TEXT NOT NULL)",
 "CREATE UNIQUE INDEX IF NOT EXISTS IX_Users_Username ON Users(Username)",
 "CREATE TABLE IF NOT EXISTS AuditEntries (Id INTEGER PRIMARY KEY AUTOINCREMENT, Actor TEXT NOT NULL, Action TEXT NOT NULL, Record TEXT NOT NULL, CreatedAtUtc TEXT NOT NULL)",
 "CREATE TABLE IF NOT EXISTS StockMovements (Id INTEGER PRIMARY KEY AUTOINCREMENT, ProductId INTEGER NOT NULL REFERENCES Products(Id) ON DELETE RESTRICT, Delta INTEGER NOT NULL, BalanceAfter INTEGER NOT NULL, Reason TEXT NOT NULL, CreatedAtUtc TEXT NOT NULL)",
 "CREATE TABLE IF NOT EXISTS MasterRecords (Id INTEGER PRIMARY KEY AUTOINCREMENT, Kind TEXT NOT NULL, Name TEXT NOT NULL, Notes TEXT)",
 "CREATE UNIQUE INDEX IF NOT EXISTS IX_MasterRecords_Kind_Name ON MasterRecords(Kind,Name)",
 "CREATE TABLE IF NOT EXISTS WarehouseBalances (Id INTEGER PRIMARY KEY AUTOINCREMENT, WarehouseId INTEGER NOT NULL, ProductId INTEGER NOT NULL REFERENCES Products(Id) ON DELETE RESTRICT, Quantity INTEGER NOT NULL)",
 "CREATE UNIQUE INDEX IF NOT EXISTS IX_WarehouseBalances_WarehouseId_ProductId ON WarehouseBalances(WarehouseId,ProductId)",
 "CREATE TABLE IF NOT EXISTS PurchaseOrders (Id INTEGER PRIMARY KEY AUTOINCREMENT, SupplierId INTEGER NOT NULL, ProductId INTEGER NOT NULL REFERENCES Products(Id) ON DELETE RESTRICT, WarehouseId INTEGER NOT NULL, Quantity INTEGER NOT NULL, UnitCost TEXT NOT NULL, Status TEXT NOT NULL, CreatedAtUtc TEXT NOT NULL)"
 })await db.Database.ExecuteSqlRawAsync(sql);
 if(!await db.Set<InventoryUser>().AnyAsync()){
 foreach(var role in new[]{"Admin","Operator","Viewer"}){
 var name=builder.Configuration[$"InventoryUsers:{role}:Username"];var hash=builder.Configuration[$"InventoryUsers:{role}:PasswordHash"];
 if(!string.IsNullOrWhiteSpace(name)&&!string.IsNullOrWhiteSpace(hash))db.Add(new InventoryUser{Username=name.Trim().ToUpperInvariant(),PasswordHash=hash,Role=role});
 }
 await db.SaveChangesAsync();
 }
 var warehouse=await db.MasterRecords.FirstOrDefaultAsync(x=>x.Kind=="Warehouse"&&x.Name=="Main");
 if(warehouse==null){warehouse=new MasterData{Kind="Warehouse",Name="Main"};db.Add(warehouse);await db.SaveChangesAsync();}
 foreach(var product in await db.Products.ToListAsync())
 if(!await db.WarehouseBalances.AnyAsync(x=>x.ProductId==product.Id))db.Add(new WarehouseBalance{ProductId=product.Id,WarehouseId=warehouse.Id,Quantity=product.Quantity});
 await db.SaveChangesAsync();await tx.CommitAsync();
}
app.MapControllerRoute("default","{controller=Dashboard}/{action=Index}/{id?}");
app.Run();
public partial class Program {}