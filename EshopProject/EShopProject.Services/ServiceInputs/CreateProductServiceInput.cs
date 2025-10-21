using System.ComponentModel.DataAnnotations;

namespace EShopProject.Services.ServiceInputs;

/// <summary>
/// Input for creating a new product
/// Only name and imageUrl are required
/// </summary>
public class CreateProductServiceInput
{
    [Required(ErrorMessage = "Product name is required")]
    [MaxLength(200, ErrorMessage = "Name cannot exceed 200 characters")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Image URL is required")]
    [MaxLength(500, ErrorMessage = "Image URL cannot exceed 500 characters")]
    [Url(ErrorMessage = "Invalid URL format")]
    public string ImageUrl { get; set; } = string.Empty;

    [Range(0, double.MaxValue, ErrorMessage = "Price must be positive")]
    public decimal? Price { get; set; }

    [MaxLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
    public string? Description { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Quantity must be non-negative")]
    public int? Quantity { get; set; }
}