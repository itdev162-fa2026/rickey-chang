using System.ComponentModel.DataAnnotations;

namespace Domain;

public class OrderItem
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public int ProductId { get; set; }

    [Required]
    [StringLength(100)]
    public string ProductName { get; set; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }

    [Range(0.01, double.MaxValue)]
    public decimal PriceAtPurchase { get; set; }

    // Navigation property back to Order
    public Order Order { get; set; } = null!;
}
