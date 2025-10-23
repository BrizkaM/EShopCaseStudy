namespace EShopProject.MessageQueue.Interfaces;

/// <summary>
/// Interface for asynchronous stock update queue
/// </summary>
public interface IStockUpdateQueue
{
    /// <summary>
    /// Enqueue a stock update request
    /// </summary>
    Task EnqueueAsync(StockUpdateRequest request);

    /// <summary>
    /// Dequeue a stock update request
    /// </summary>
    Task<StockUpdateRequest?> DequeueAsync();

    /// <summary>
    /// Get current queue size
    /// </summary>
    int Count { get; }
}
