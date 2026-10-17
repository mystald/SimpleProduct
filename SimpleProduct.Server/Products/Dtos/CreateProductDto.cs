using System.ComponentModel.DataAnnotations;

namespace SimpleProduct.Server.Products.Dtos;

public class CreateProductDto
{
    [Required(ErrorMessage = "Product name is required.")]
    [StringLength(200, ErrorMessage = "Product name cannot exceed 200 characters.")]
    public string Name { get; set; } = string.Empty;

    [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters.")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Price is required.")]
    [Range(0.01, (double)decimal.MaxValue, ErrorMessage = "Price must be greater than 0.")]
    public decimal Price { get; set; }
}
