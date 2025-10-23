using EShopProject.MessageQueue.Interfaces;
using System.Collections.Concurrent;

namespace EShopProject.MessageQueue;

/// <summary>
/// In-memory implementation of stock update queue
/// Thread-safe using ConcurrentQueue
/// </summary>
public class InMemoryStockUpdateQueue : IStockUpdateQueue
{
    private readonly ConcurrentQueue<StockUpdateRequest> _queue;

    public InMemoryStockUpdateQueue()
    {
        _queue = new ConcurrentQueue<StockUpdateRequest>();
    }

    public Task EnqueueAsync(StockUpdateRequest request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        _queue.Enqueue(request);
        return Task.CompletedTask;
    }

    public Task<StockUpdateRequest?> DequeueAsync()
    {
        _queue.TryDequeue(out var request);
        return Task.FromResult(request);
    }

    public int Count => _queue.Count;
}