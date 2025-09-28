using System.ComponentModel.DataAnnotations;

namespace Domain.Validation;

public class ValidSalePriceAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var product = (Product)validationContext.ObjectInstance;

        if (product.IsOnSale)
        {
            if (product.SalePrice == null)
            {
                return new ValidationResult("Sale price must be provided when product is on sale");
            }

            if (product.SalePrice >= product.Price)
            {
                return new ValidationResult("Sale price must be less than regular price");
            }
        }

        return ValidationResult.Success;
    }
}
