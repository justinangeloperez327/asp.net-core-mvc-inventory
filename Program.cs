using InventoryMvc.Data;
using Microsoft.EntityFrameworkCore;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<InventoryDbContext>(options => options.UseSqlite(builder.Configuration.GetConnectionString("Inventory") ?? "Data Source=inventory.db"));
var app = builder.Build();
if (!app.Environment.IsDevelopment()) { app.UseExceptionHandler("/Home/Error"); app.UseHsts(); }
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
using (var scope = app.Services.CreateScope()) { scope.ServiceProvider.GetRequiredService<InventoryDbContext>().Database.EnsureCreated(); var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>(); db.Database.ExecuteSqlRaw("CREATE TABLE IF NOT EXISTS StockMovements (Id INTEGER PRIMARY KEY AUTOINCREMENT, ProductId INTEGER NOT NULL REFERENCES Products(Id) ON DELETE RESTRICT, Delta INTEGER NOT NULL, BalanceAfter INTEGER NOT NULL, Reason TEXT NOT NULL, CreatedAtUtc TEXT NOT NULL)"); db.Database.ExecuteSqlRaw("CREATE INDEX IF NOT EXISTS IX_StockMovements_ProductId ON StockMovements(ProductId)"); }
app.MapControllerRoute(name: "default", pattern: "{controller=Dashboard}/{action=Index}/{id?}");
app.Run();