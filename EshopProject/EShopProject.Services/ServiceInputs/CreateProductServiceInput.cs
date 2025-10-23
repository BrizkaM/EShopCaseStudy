//------------------------------------------------------------------------------------------
// File: CreateProductServiceInput.cs
//------------------------------------------------------------------------------------------
using System.ComponentModel.DataAnnotations;

namespace EShopProject.Services.ServiceInputs;

/// <summary>
/// Input for creating a new product
/// Only name and imageUrl are required
/// </summary>
public class CreateProductServiceInput
{
    /// <summary>
    /// Represents the name of the product.
    /// </summary>
    /// <remarks>
    /// This field is required and its length must not exceed 200 characters.
    /// </remarks>
    [Required(ErrorMessage = "Product name is required")]
    [MaxLength(200, ErrorMessage = "Name cannot exceed 200 characters")]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Represents the image URL of the product.
    /// </summary>
    /// <remarks>
    /// This field is required, its length must not exceed 500 characters, and it must be a valid URL.
    /// </remarks>
    [Required(ErrorMessage = "Image URL is required")]
    [MaxLength(500, ErrorMessage = "Image URL cannot exceed 500 characters")]
    [Url(ErrorMessage = "Invalid URL format")]
    public string ImageUrl { get; set; } = string.Empty;

    /// <summary>
    /// Represents the price of the product.
    /// </summary>
    /// <remarks>
    /// This field is optional, but if provided, it must be a non-negative value.
    /// </remarks>
    [Range(0, double.MaxValue, ErrorMessage = "Price must be positive")]
    public decimal? Price { get; set; }

    /// <summary>
    /// Represents the description of the product.
    /// </summary>
    /// <remarks>
    /// This field is optional and its length must not exceed 2000 characters.
    /// </remarks>
    [MaxLength(2000, ErrorMessage = "Description cannot exceed 2000 characters")]
    public string? Description { get; set; }

    /// <summary>
    /// Represents the quantity of the product in stock.
    /// </summary>
    /// <remarks>
    /// This field is optional, but if provided, it must be a non-negative integer.
    /// </remarks>
    [Range(0, int.MaxValue, ErrorMessage = "Quantity must be non-negative")]
    public int? Quantity { get; set; }
}