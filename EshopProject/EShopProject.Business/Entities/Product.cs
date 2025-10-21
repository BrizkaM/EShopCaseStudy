using System.ComponentModel.DataAnnotations;

namespace EShopProject.Business.Entities;

/// <summary>
/// Product entity representing an e-shop product
/// </summary>
public class Product
{
    public Product()
    {
        Idate = DateTime.UtcNow;
        Udate = DateTime.UtcNow;
    }

    /// <summary>
    /// Unique identifier for the product
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Name of the product (required)
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// URL to the main product image (required)
    /// </summary>
    [Required]
    [MaxLength(500)]
    public string ImageUrl { get; set; } = string.Empty;

    /// <summary>
    /// Price of the product (optional)
    /// </summary>
    public decimal? Price { get; set; }

    /// <summary>
    /// Description of the product (optional)
    /// </summary>
    [MaxLength(2000)]
    public string? Description { get; set; }

    /// <summary>
    /// Quantity of product in stock (default: 0)
    /// </summary>
    public int Quantity { get; set; } = 0;

    /// <summary>
    /// Date when the product was created
    /// </summary>
    public DateTime Idate { get; set; }

    /// <summary>
    /// Date when the product was last updated
    /// </summary>
    public DateTime Udate { get; set; }
}