namespace EShopProject.MessageQueue.Interfaces;

/// <summary>
/// Interface for handling stock updates
/// This should be implemented by the consumer (WebApi) to define business logic
/// </summary>
public interface IStockUpdateHandler
{
    /// <summary>
    /// Handle stock update for a product
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <param name="quantity">New quantity</param>
    /// <returns>True if successful, false otherwise</returns>
    Task<bool> HandleStockUpdateAsync(int productId, int quantity);
}
