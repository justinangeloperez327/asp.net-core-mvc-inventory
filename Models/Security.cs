using System.ComponentModel.DataAnnotations;
namespace InventoryMvc.Models;
public class InventoryUser {
 public int Id {get;set;}
 [Required,StringLength(80)] public string Username {get;set;}="";
 public string PasswordHash {get;set;}="";
 public string Role {get;set;}="Viewer";
 public bool Active {get;set;}=true;
 public string Stamp {get;set;}=Guid.NewGuid().ToString("N");
}
public class AuditEntry {
 public int Id {get;set;}
 public string Actor {get;set;}="";
 public string Action {get;set;}="";
 public string Record {get;set;}="";
 public DateTime CreatedAtUtc {get;set;}=DateTime.UtcNow;
}
