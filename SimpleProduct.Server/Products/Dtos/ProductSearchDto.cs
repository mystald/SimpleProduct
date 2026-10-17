using System.ComponentModel.DataAnnotations;

namespace SimpleProduct.Server.Products.Dtos;

public class ProductSearchDto
{
    [StringLength(200, ErrorMessage = "Search term cannot exceed 200 characters.")]
    public string? Name { get; set; }

    [Range(0, (double)decimal.MaxValue, ErrorMessage = "MinPrice must be non-negative.")]
    public decimal? MinPrice { get; set; }

    [Range(0, (double)decimal.MaxValue, ErrorMessage = "MaxPrice must be non-negative.")]
    public decimal? MaxPrice { get; set; }
}
