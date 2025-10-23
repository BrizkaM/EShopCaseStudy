//------------------------------------------------------------------------------------------
// File: ProductDto.cs
//------------------------------------------------------------------------------------------
namespace EShopProject.Services.ServiceOutputs;

/// <summary>
/// Data Transfer Object for Product
/// </summary>
public class ProductDto
{
    /// <summary>
    /// Gets or sets the unique identifier for the product.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the name of the product.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the URL to the product's image.
    /// </summary>
    public string ImageUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the price of the product. Null if price is not set.
    /// </summary>
    public decimal? Price { get; set; }

    /// <summary>
    /// Gets or sets the detailed description of the product. Null if description is not available.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the available quantity of the product in stock.
    /// </summary>
    public int Quantity { get; set; }
}
