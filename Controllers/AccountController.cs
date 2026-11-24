using InventoryMvc.Data;
using InventoryMvc.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
namespace InventoryMvc.Controllers;
public class AccountController(InventoryDbContext db):Controller {
 [AllowAnonymous] public IActionResult Login()=>View();
 [AllowAnonymous,HttpPost,ValidateAntiForgeryToken]
 public async Task<IActionResult> Login(string username,string password){
 var name=(username??"").Trim().ToUpperInvariant();
 var user=await db.Set<InventoryUser>().SingleOrDefaultAsync(x=>x.Username==name&&x.Active);
 if(user!=null&&!string.IsNullOrEmpty(password)&&password.Length<=256&&new PasswordHasher<string>().VerifyHashedPassword(name,user.PasswordHash,password)!=PasswordVerificationResult.Failed){
 var identity=new ClaimsIdentity(new[]{new Claim(ClaimTypes.Name,user.Username),new Claim(ClaimTypes.NameIdentifier,user.Id.ToString()),new Claim(ClaimTypes.Role,user.Role),new Claim("stamp",user.Stamp)},CookieAuthenticationDefaults.AuthenticationScheme);
 await HttpContext.SignInAsync(new ClaimsPrincipal(identity));return RedirectToAction("Index","Dashboard");
 }
 ModelState.AddModelError("","Invalid credentials.");return View();
 }
 [HttpPost,ValidateAntiForgeryToken] public async Task<IActionResult> Logout(){await HttpContext.SignOutAsync();return RedirectToAction(nameof(Login));}
 [AllowAnonymous] public IActionResult Denied()=>StatusCode(403);
}