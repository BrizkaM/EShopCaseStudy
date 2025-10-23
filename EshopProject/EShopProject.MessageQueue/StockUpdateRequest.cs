//------------------------------------------------------------------------------------------
// File: StockUpdateRequest.cs
//------------------------------------------------------------------------------------------
namespace EShopProject.MessageQueue;

/// <summary>
/// Stock update request model
/// </summary>
public class StockUpdateRequest
{
    /// <summary>
    /// Initializes a new instance of the <see cref="StockUpdateRequest"/> class with current UTC timestamp.
    /// </summary>
    public StockUpdateRequest()
    {
        RequestedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Gets or sets the unique identifier of the product to update.
    /// </summary>
    public int ProductId { get; set; }

    /// <summary>
    /// Gets or sets the quantity to adjust the stock level by. 
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Gets or sets the UTC timestamp when this stock update request was created.
    /// </summary>
    public DateTime RequestedAt { get; set; }
}