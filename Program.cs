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
using (var scope = app.Services.CreateScope()) { scope.ServiceProvider.GetRequiredService<InventoryDbContext>().Database.EnsureCreated(); }
app.MapControllerRoute(name: "default", pattern: "{controller=Products}/{action=Index}/{id?}");
app.Run();