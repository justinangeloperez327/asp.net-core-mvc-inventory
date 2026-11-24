using System.ComponentModel.DataAnnotations;
namespace InventoryMvc.Models;
public class MasterData
{
 public int Id {get;set;}
 [Required,StringLength(20)] public string Kind {get;set;}="";
 [Required,StringLength(120)] public string Name {get;set;}="";
 [StringLength(250)] public string? Notes {get;set;}
}
public class PurchaseOrder
{
 public int Id {get;set;}
 public int SupplierId {get;set;}
 public int ProductId {get;set;}
 public int WarehouseId {get;set;}
 [Range(1,int.MaxValue)] public int Quantity {get;set;}
 [Range(typeof(decimal),"0","999999999.99")] public decimal UnitCost {get;set;}
 public string Status {get;set;}="Draft";
 public DateTime CreatedAtUtc {get;set;}=DateTime.UtcNow;
}
public class WarehouseBalance
{
 public int Id {get;set;}
 public int WarehouseId {get;set;}
 public int ProductId {get;set;}
 public int Quantity {get;set;}
}
public class WarehouseInput
{
 [Range(1,int.MaxValue)] public int ProductId {get;set;}
 [Range(1,int.MaxValue)] public int WarehouseId {get;set;}
 public int? DestinationId {get;set;}
 [Range(1,int.MaxValue)] public int Quantity {get;set;}
 [Required] public string Operation {get;set;}="In";
 [Required,StringLength(200)] public string Reason {get;set;}="";
}