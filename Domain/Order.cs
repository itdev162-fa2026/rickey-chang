using System.ComponentModel.DataAnnotations;

namespace Domain;

public enum OrderStatus
{
    Pending,
    Completed,
    Failed
}

public class Order
{
    public int Id { get; set; }

    [Required]
    [EmailAddress]
    public string CustomerEmail { get; set; } = string.Empty;

    [Range(0.01, double.MaxValue)]
    public decimal TotalAmount { get; set; }

    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    public DateTime CreatedDate { get; set; }

    public DateTime? CompletedDate { get; set; }

    // Stripe payment fields
    public string? StripeSessionId { get; set; }
    public string? StripePaymentIntentId { get; set; }

    // Navigation property
    public List<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
