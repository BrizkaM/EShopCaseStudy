//------------------------------------------------------------------------------------------
// File: StockUpdateQueueTests.cs
//------------------------------------------------------------------------------------------
using EShopProject.MessageQueue;

namespace EShopProject.MessageQueueTest;

/// <summary>
/// Unit tests for InMemoryStockUpdateQueue
/// </summary>
[TestClass]
public class StockUpdateQueueTests
{
    /// <summary>
    /// Tests that EnqueueAsync successfully adds a request to the queue and increments count
    /// </summary>
    [TestMethod]
    public async Task StockUpdateQueue_EnqueueAsync_AddsRequestToQueue()
    {
        // Arrange
        var queue = new InMemoryStockUpdateQueue();
        var request = new StockUpdateRequest
        {
            ProductId = 1,
            Quantity = 50
        };

        // Act
        await queue.EnqueueAsync(request);

        // Assert
        Assert.AreEqual(1, queue.Count);
    }

    /// <summary>
    /// Tests that DequeueAsync returns the first request in FIFO order and maintains correct queue state
    /// </summary>
    [TestMethod]
    public async Task StockUpdateQueue_DequeueAsync_ReturnsFirstRequest()
    {
        // Arrange
        var queue = new InMemoryStockUpdateQueue();
        var request1 = new StockUpdateRequest { ProductId = 1, Quantity = 10 };
        var request2 = new StockUpdateRequest { ProductId = 2, Quantity = 20 };

        await queue.EnqueueAsync(request1);
        await queue.EnqueueAsync(request2);

        // Act
        var result = await queue.DequeueAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.ProductId);
        Assert.AreEqual(10, result.Quantity);
        Assert.AreEqual(1, queue.Count);
    }

    /// <summary>
    /// Tests that DequeueAsync returns null when attempting to dequeue from an empty queue
    /// </summary>
    [TestMethod]
    public async Task StockUpdateQueue_DequeueAsync_EmptyQueue_ReturnsNull()
    {
        // Arrange
        var queue = new InMemoryStockUpdateQueue();

        // Act
        var result = await queue.DequeueAsync();

        // Assert
        Assert.IsNull(result);
    }

    /// <summary>
    /// Verifies that the queue maintains First-In-First-Out (FIFO) order when processing multiple requests
    /// </summary>
    [TestMethod]
    public async Task StockUpdateQueue_Queue_FIFO_Order()
    {
        // Arrange
        var queue = new InMemoryStockUpdateQueue();
        var requests = new[]
        {
            new StockUpdateRequest { ProductId = 1, Quantity = 10 },
            new StockUpdateRequest { ProductId = 2, Quantity = 20 },
            new StockUpdateRequest { ProductId = 3, Quantity = 30 }
        };

        foreach (var request in requests)
        {
            await queue.EnqueueAsync(request);
        }

        // Act & Assert
        for (int i = 0; i < 3; i++)
        {
            var result = await queue.DequeueAsync();
            Assert.IsNotNull(result);
            Assert.AreEqual(i + 1, result.ProductId);
            Assert.AreEqual((i + 1) * 10, result.Quantity);
        }

        Assert.AreEqual(0, queue.Count);
    }

    /// <summary>
    /// Tests that EnqueueAsync throws ArgumentNullException when attempting to enqueue a null request
    /// </summary>
    [TestMethod]
    public async Task StockUpdateQueue_EnqueueAsync_NullRequest_ThrowsArgumentNullException()
    {
        // Arrange
        var queue = new InMemoryStockUpdateQueue();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            queue.EnqueueAsync(null!));
    }

    /// <summary>
    /// Verifies that the Count property accurately reflects the current size of the queue after various operations
    /// </summary>
    [TestMethod]
    public async Task StockUpdateQueue_Count_ReflectsQueueSize()
    {
        // Arrange
        var queue = new InMemoryStockUpdateQueue();

        // Act & Assert
        Assert.AreEqual(0, queue.Count);

        await queue.EnqueueAsync(new StockUpdateRequest { ProductId = 1, Quantity = 10 });
        Assert.AreEqual(1, queue.Count);

        await queue.EnqueueAsync(new StockUpdateRequest { ProductId = 2, Quantity = 20 });
        Assert.AreEqual(2, queue.Count);

        await queue.DequeueAsync();
        Assert.AreEqual(1, queue.Count);

        await queue.DequeueAsync();
        Assert.AreEqual(0, queue.Count);
    }

    /// <summary>
    /// Tests that StockUpdateRequest correctly sets the RequestedAt timestamp upon creation
    /// </summary>
    [TestMethod]
    public void StockUpdateQueue_StockUpdateRequest_SetsRequestedAt()
    {
        // Arrange
        var beforeCreation = DateTime.UtcNow;

        // Act
        var request = new StockUpdateRequest
        {
            ProductId = 1,
            Quantity = 50
        };

        var afterCreation = DateTime.UtcNow;

        // Assert
        Assert.IsTrue(request.RequestedAt >= beforeCreation);
        Assert.IsTrue(request.RequestedAt <= afterCreation);
    }

    /// <summary>
    /// Verifies thread safety of the queue when multiple threads perform concurrent enqueue operations
    /// </summary>
    [TestMethod]
    public async Task StockUpdateQueue_Queue_ThreadSafety_MultipleEnqueues()
    {
        // Arrange
        var queue = new InMemoryStockUpdateQueue();
        var tasks = new List<Task>();

        // Act - Enqueue from multiple threads
        for (int i = 0; i < 100; i++)
        {
            int productId = i;
            tasks.Add(Task.Run(async () =>
            {
                await queue.EnqueueAsync(new StockUpdateRequest
                {
                    ProductId = productId,
                    Quantity = productId * 10
                });
            }));
        }

        await Task.WhenAll(tasks);

        // Assert
        Assert.AreEqual(100, queue.Count);
    }

    /// <summary>
    /// Verifies thread safety of the queue when multiple threads perform concurrent dequeue operations
    /// </summary>
    [TestMethod]
    public async Task StockUpdateQueue_Queue_ThreadSafety_MultipleDequeues()
    {
        // Arrange
        var queue = new InMemoryStockUpdateQueue();

        // Add 100 items
        for (int i = 0; i < 100; i++)
        {
            await queue.EnqueueAsync(new StockUpdateRequest
            {
                ProductId = i,
                Quantity = i * 10
            });
        }

        // Act - Dequeue from multiple threads
        var tasks = new List<Task<StockUpdateRequest?>>();
        for (int i = 0; i < 100; i++)
        {
            tasks.Add(Task.Run(async () => await queue.DequeueAsync()));
        }

        var results = await Task.WhenAll(tasks);

        // Assert
        Assert.AreEqual(0, queue.Count);
        Assert.AreEqual(100, results.Count(r => r != null));
    }

    /// <summary>
    /// Tests concurrent enqueue and dequeue operations to ensure thread safety and data integrity
    /// </summary>
    [TestMethod]
    public async Task StockUpdateQueue_Queue_ConcurrentEnqueueDequeue_WorksCorrectly()
    {
        // Arrange
        var queue = new InMemoryStockUpdateQueue();
        var enqueueCount = 0;
        var dequeueCount = 0;

        // Act - Concurrent enqueue and dequeue
        var enqueueTasks = new List<Task>();
        var dequeueTasks = new List<Task>();

        for (int i = 0; i < 50; i++)
        {
            int productId = i;
            enqueueTasks.Add(Task.Run(async () =>
            {
                await queue.EnqueueAsync(new StockUpdateRequest
                {
                    ProductId = productId,
                    Quantity = productId * 10
                });
                Interlocked.Increment(ref enqueueCount);
            }));

            dequeueTasks.Add(Task.Run(async () =>
            {
                var result = await queue.DequeueAsync();
                if (result != null)
                {
                    Interlocked.Increment(ref dequeueCount);
                }
            }));
        }

        await Task.WhenAll(enqueueTasks.Concat(dequeueTasks));

        // Assert
        Assert.AreEqual(50, enqueueCount);
        Assert.IsLessThanOrEqualTo(50, dequeueCount); // Some dequeues might happen before enqueues
        Assert.IsGreaterThanOrEqualTo(0, queue.Count);
        Assert.AreEqual(50 - dequeueCount, queue.Count);
    }

    /// <summary>
    /// Verifies that multiple enqueue and dequeue operations maintain data integrity and FIFO order
    /// </summary>
    [TestMethod]
    public async Task StockUpdateQueue_Queue_MultipleEnqueuesAndDequeues_MaintainsIntegrity()
    {
        // Arrange
        var queue = new InMemoryStockUpdateQueue();
        var totalItems = 1000;

        // Act - Enqueue items
        for (int i = 0; i < totalItems; i++)
        {
            await queue.EnqueueAsync(new StockUpdateRequest
            {
                ProductId = i,
                Quantity = i
            });
        }

        // Assert - Count is correct
        Assert.AreEqual(totalItems, queue.Count);

        // Act - Dequeue half
        var dequeuedItems = new List<StockUpdateRequest?>();
        for (int i = 0; i < totalItems / 2; i++)
        {
            dequeuedItems.Add(await queue.DequeueAsync());
        }

        // Assert
        Assert.AreEqual(totalItems / 2, queue.Count);
        Assert.AreEqual(totalItems / 2, dequeuedItems.Count(r => r != null));

        // Verify FIFO order
        for (int i = 0; i < dequeuedItems.Count; i++)
        {
            Assert.IsNotNull(dequeuedItems[i]);
            Assert.AreEqual(i, dequeuedItems[i]!.ProductId);
        }
    }

    /// <summary>
    /// Tests that the queue properly empties and maintains correct state after multiple enqueue/dequeue cycles
    /// </summary>
    [TestMethod]
    public async Task StockUpdateQueue_Queue_EmptyAfterMultipleOperations()
    {
        // Arrange
        var queue = new InMemoryStockUpdateQueue();

        // Act - Multiple enqueue/dequeue cycles
        for (int cycle = 0; cycle < 5; cycle++)
        {
            // Enqueue 10 items
            for (int i = 0; i < 10; i++)
            {
                await queue.EnqueueAsync(new StockUpdateRequest
                {
                    ProductId = i,
                    Quantity = i * 10
                });
            }

            Assert.AreEqual(10, queue.Count);

            // Dequeue all items
            for (int i = 0; i < 10; i++)
            {
                var result = await queue.DequeueAsync();
                Assert.IsNotNull(result);
            }

            // Assert queue is empty after each cycle
            Assert.AreEqual(0, queue.Count);
        }
    }

    /// <summary>
    /// Verifies that StockUpdateRequest properties are set and maintained correctly
    /// </summary>
    [TestMethod]
    public void StockUpdateQueue_StockUpdateRequest_Properties_SetCorrectly()
    {
        // Arrange & Act
        var request = new StockUpdateRequest
        {
            ProductId = 42,
            Quantity = 100
        };

        // Assert
        Assert.AreEqual(42, request.ProductId);
        Assert.AreEqual(100, request.Quantity);
        Assert.IsTrue(request.RequestedAt <= DateTime.UtcNow);
        Assert.IsTrue(request.RequestedAt >= DateTime.UtcNow.AddSeconds(-1));
    }

    /// <summary>
    /// Tests the queue's ability to handle a large number of items while maintaining correct behavior
    /// </summary>
    [TestMethod]
    public async Task StockUpdateQueue_Queue_LargeNumberOfItems_HandlesCorrectly()
    {
        // Arrange
        var queue = new InMemoryStockUpdateQueue();
        var itemCount = 10000;

        // Act - Enqueue large number of items
        for (int i = 0; i < itemCount; i++)
        {
            await queue.EnqueueAsync(new StockUpdateRequest
            {
                ProductId = i,
                Quantity = i
            });
        }

        // Assert
        Assert.AreEqual(itemCount, queue.Count);

        // Act - Dequeue all
        var count = 0;
        while (queue.Count > 0)
        {
            var result = await queue.DequeueAsync();
            if (result != null) count++;
        }

        // Assert
        Assert.AreEqual(itemCount, count);
        Assert.AreEqual(0, queue.Count);
    }

    /// <summary>
    /// Verifies that multiple dequeue calls on an empty queue consistently return null
    /// </summary>
    [TestMethod]
    public async Task StockUpdateQueue_DequeueAsync_MultipleCallsOnEmptyQueue_ReturnsNull()
    {
        // Arrange
        var queue = new InMemoryStockUpdateQueue();

        // Act & Assert
        for (int i = 0; i < 10; i++)
        {
            var result = await queue.DequeueAsync();
            Assert.IsNull(result);
            Assert.AreEqual(0, queue.Count);
        }
    }

    /// <summary>
    /// Tests alternating enqueue and dequeue operations to verify correct queue behavior and state management
    /// </summary>
    [TestMethod]
    public async Task StockUpdateQueue_Queue_AlternatingEnqueueDequeue_WorksCorrectly()
    {
        // Arrange
        var queue = new InMemoryStockUpdateQueue();

        // Act & Assert - Alternate operations
        for (int i = 0; i < 100; i++)
        {
            // Enqueue
            await queue.EnqueueAsync(new StockUpdateRequest
            {
                ProductId = i,
                Quantity = i * 10
            });
            Assert.AreEqual(1, queue.Count);

            // Dequeue immediately
            var result = await queue.DequeueAsync();
            Assert.IsNotNull(result);
            Assert.AreEqual(i, result.ProductId);
            Assert.AreEqual(0, queue.Count);
        }
    }
}