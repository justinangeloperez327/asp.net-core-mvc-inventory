using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
namespace InventoryMvc.Controllers;
public class AccountController(IConfiguration config):Controller
{
 [AllowAnonymous] public IActionResult Login()=>View();
 [AllowAnonymous,HttpPost,ValidateAntiForgeryToken]
 public async Task<IActionResult> Login(string username,string password)
 {
 foreach(var role in new[]{"Admin","Operator","Viewer"})
 {
 var name=config[$"InventoryUsers:{role}:Username"];var hash=config[$"InventoryUsers:{role}:PasswordHash"];
 if(string.IsNullOrEmpty(name)||string.IsNullOrEmpty(hash)||username!=name||string.IsNullOrEmpty(password))continue;
 var hasher=new Microsoft.AspNetCore.Identity.PasswordHasher<string>();
 try {if(hasher.VerifyHashedPassword(name,hash,password)==Microsoft.AspNetCore.Identity.PasswordVerificationResult.Failed)continue;}
 catch(FormatException){continue;}
 var identity=new ClaimsIdentity(new[]{new Claim(ClaimTypes.Name,name),new Claim(ClaimTypes.Role,role)},CookieAuthenticationDefaults.AuthenticationScheme);
 await HttpContext.SignInAsync(new ClaimsPrincipal(identity));return RedirectToAction("Index","Dashboard");
 }
 ModelState.AddModelError("","Invalid credentials.");return View();
 }
 [HttpPost,ValidateAntiForgeryToken] public async Task<IActionResult> Logout(){await HttpContext.SignOutAsync();return RedirectToAction(nameof(Login));}
 [AllowAnonymous] public IActionResult Denied()=>StatusCode(403);
}