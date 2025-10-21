using System.ComponentModel.DataAnnotations;

namespace EShopProject.Services.ServiceInputs;

/// <summary>
/// Input for updating a product
/// </summary>
public class UpdateProductStockServiceInput
{
    [Required(ErrorMessage = "Quantity is required")]
    [Range(0, int.MaxValue, ErrorMessage = "Quantity must be non-negative")]
    public int Quantity { get; set; }
}