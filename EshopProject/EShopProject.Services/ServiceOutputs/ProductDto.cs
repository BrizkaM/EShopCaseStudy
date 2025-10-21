namespace EShopProject.Services.ServiceOutputs;

/// <summary>
/// Data Transfer Object for Product
/// </summary>
public class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public decimal? Price { get; set; }
    public string? Description { get; set; }
    public int Quantity { get; set; }
}
