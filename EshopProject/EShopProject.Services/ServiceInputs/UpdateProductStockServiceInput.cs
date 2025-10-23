//------------------------------------------------------------------------------------------
// File: UpdateProductStockServiceInput.cs
//------------------------------------------------------------------------------------------
using System.ComponentModel.DataAnnotations;

namespace EShopProject.Services.ServiceInputs;

/// <summary>
/// Input for updating a product
/// </summary>
public class UpdateProductStockServiceInput
{
    /// <summary>
    /// Represents the quantity of the product in stock to be updated.
    /// </summary>
    /// <remarks>
    /// This field is optional, but if provided, it must be a non-negative integer.
    /// </remarks>
    [Required(ErrorMessage = "Quantity is required")]
    [Range(0, int.MaxValue, ErrorMessage = "Quantity must be non-negative")]
    public int Quantity { get; set; }
}