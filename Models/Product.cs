using System.ComponentModel.DataAnnotations;
namespace InventoryMvc.Models;
public class Product
{
    public int Id { get; set; }
    [Required, StringLength(120)] public string Name { get; set; } = string.Empty;
    [Required, StringLength(50), Display(Name = "SKU")] public string Sku { get; set; } = string.Empty;
    [StringLength(100)] public string? Category { get; set; }
    [Range(0, int.MaxValue)] public int Quantity { get; set; }
    [Range(typeof(decimal), "0", "999999999.99"), Display(Name = "Unit price")] public decimal UnitPrice { get; set; }
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}