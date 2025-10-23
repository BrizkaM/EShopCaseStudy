namespace EShopProject.MessageQueue;

/// <summary>
/// Stock update request model
/// </summary>
public class StockUpdateRequest
{
    public StockUpdateRequest()
    {
        RequestedAt = DateTime.UtcNow;
    }

    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public DateTime RequestedAt { get; set; }

}