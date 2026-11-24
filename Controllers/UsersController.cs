using InventoryMvc.Data;
using InventoryMvc.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace InventoryMvc.Controllers;
[Authorize(Roles="Admin")]
public class UsersController(InventoryDbContext db):Controller {
 public async Task<IActionResult> Index()=>View(await db.Set<InventoryUser>().AsNoTracking().OrderBy(x=>x.Username).ToListAsync());
 [HttpPost,ValidateAntiForgeryToken]
 public async Task<IActionResult> Save(int id,string username,string role,bool active,string? password){
 if(string.IsNullOrWhiteSpace(username)||username.Trim().Length>80||role is not ("Admin" or "Operator" or "Viewer"))return BadRequest("Invalid account.");
 username=username.Trim().ToUpperInvariant();
 await using var tx=await db.Database.BeginTransactionAsync();
 var users=db.Set<InventoryUser>();
 if(await users.AnyAsync(x=>x.Username==username&&x.Id!=id))return BadRequest("Username already exists.");
 var user=id==0?new InventoryUser():await users.FindAsync(id);
 if(user==null)return NotFound();
 if(id!=0&&user.Role=="Admin"&&user.Active&&(!active||role!="Admin")&&!await users.AnyAsync(x=>x.Id!=id&&x.Role=="Admin"&&x.Active))return BadRequest("At least one active administrator is required.");
 if(id==0||!string.IsNullOrEmpty(password)){
 if(password==null||password.Length<12||password.Length>256)return BadRequest("Password must contain 12–256 characters.");
 user.PasswordHash=new PasswordHasher<string>().HashPassword(username,password);
 }
 user.Username=username;user.Role=role;user.Active=active;user.Stamp=Guid.NewGuid().ToString("N");
 if(id==0)users.Add(user);
 await db.SaveChangesAsync();await tx.CommitAsync();return RedirectToAction(nameof(Index));
 }
}