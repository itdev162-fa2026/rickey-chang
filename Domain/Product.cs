using System.ComponentModel.DataAnnotations;
using Domain.Validation;

namespace Domain;

[ValidSalePrice]
public class Product
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Product name is required")]
    [StringLength(100, ErrorMessage = "Product name cannot exceed 100 characters")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Product description is required")]
    [StringLength(1000, ErrorMessage = "Product description cannot exceed 1000 characters")]
    public string Description { get; set; } = string.Empty;

    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
    public decimal Price { get; set; }

    public bool IsOnSale { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Sale price must be non-negative")]
    public decimal? SalePrice { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Stock must be non-negative")]
    public int CurrentStock { get; set; }

    [Url(ErrorMessage = "Invalid image URL format")]
    public string ImageUrl { get; set; } = string.Empty;

    public DateTime CreatedDate { get; set; }
    public DateTime LastUpdatedDate { get; set; }
}
