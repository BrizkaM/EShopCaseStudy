//------------------------------------------------------------------------------------------
// File: InMemoryStockUpdateQueue.cs
//------------------------------------------------------------------------------------------
using EShopProject.MessageQueue.Interfaces;
using System.Collections.Concurrent;

namespace EShopProject.MessageQueue;

/// <summary>
/// In-memory implementation of stock update queue using thread-safe ConcurrentQueue
/// </summary>
public class InMemoryStockUpdateQueue : IStockUpdateQueue
{
    /// <summary>
    /// The underlying thread-safe queue that stores stock update requests
    /// </summary>
    private readonly ConcurrentQueue<StockUpdateRequest> _queue;

    /// <summary>
    /// Initializes a new instance of the <see cref="InMemoryStockUpdateQueue"/> class
    /// </summary>
    public InMemoryStockUpdateQueue()
    {
        _queue = new ConcurrentQueue<StockUpdateRequest>();
    }

    /// <summary>
    /// Enqueues a stock update request to the queue
    /// </summary>
    /// <param name="request">The stock update request to enqueue</param>
    /// <exception cref="ArgumentNullException">Thrown when request is null</exception>
    /// <returns>A completed task representing the asynchronous operation</returns>
    public Task EnqueueAsync(StockUpdateRequest request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        _queue.Enqueue(request);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Attempts to dequeue a stock update request from the queue
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains
    /// the dequeued request, or null if the queue is empty
    /// </returns>
    public Task<StockUpdateRequest?> DequeueAsync()
    {
        _queue.TryDequeue(out var request);
        return Task.FromResult(request);
    }

    /// <summary>
    /// Gets the current number of items in the queue
    /// </summary>
    public int Count => _queue.Count;
}