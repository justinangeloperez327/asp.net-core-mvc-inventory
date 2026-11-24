using System.ComponentModel.DataAnnotations;
namespace InventoryMvc.Models;
public class StockMovement
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
    public int Delta { get; set; }
    public int BalanceAfter { get; set; }
    [Required, StringLength(200)] public string Reason { get; set; } = "";
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
public class MovementInput
{
    [Range(1,int.MaxValue)] public int ProductId { get; set; }
    [Range(1,int.MaxValue)] public int Quantity { get; set; }
    public bool Outbound { get; set; }
    [Required, StringLength(200)] public string Reason { get; set; } = "";
}