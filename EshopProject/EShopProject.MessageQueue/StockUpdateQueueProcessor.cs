//------------------------------------------------------------------------------------------
// File: StockUpdateQueueProcessor.cs
//------------------------------------------------------------------------------------------
using EShopProject.MessageQueue.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EShopProject.MessageQueue;

/// <summary>
/// Background service that processes stock updates from a queue.
/// Runs continuously at configurable intervals (default 2 seconds) to handle pending stock update requests.
/// </summary>
/// <remarks>
/// This service implements <see cref="BackgroundService"/> to run as a long-running service.
/// It processes stock updates in a fault-tolerant way, logging errors without crashing the service.
/// </remarks>
public class StockUpdateQueueProcessor : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IStockUpdateQueue _queue;
    private readonly ILogger<StockUpdateQueueProcessor> _logger;
    private readonly TimeSpan _processingInterval;

    /// <summary>
    /// Initializes a new instance of the <see cref="StockUpdateQueueProcessor"/> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider for dependency injection.</param>
    /// <param name="queue">The queue interface for processing stock updates.</param>
    /// <param name="logger">The logger instance for this processor.</param>
    /// <param name="processingInterval">Optional interval between processing cycles. Defaults to 2 seconds if not specified.</param>
    /// <exception cref="ArgumentNullException">Thrown when any required dependency is null.</exception>
    public StockUpdateQueueProcessor(
        IServiceProvider serviceProvider,
        IStockUpdateQueue queue,
        ILogger<StockUpdateQueueProcessor> logger,
        TimeSpan? processingInterval = null)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _queue = queue ?? throw new ArgumentNullException(nameof(queue));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _processingInterval = processingInterval ?? TimeSpan.FromSeconds(2);
    }

    /// <summary>
    /// Executes the background processing loop for stock updates.
    /// </summary>
    /// <param name="stoppingToken">Cancellation token used to signal when the service should stop.</param>
    /// <returns>A task representing the background processing operation.</returns>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Stock Update Queue Processor started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessQueueAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing stock update queue");
            }

            await Task.Delay(_processingInterval, stoppingToken);
        }

        _logger.LogInformation("Stock Update Queue Processor stopped");
    }

    /// <summary>
    /// Processes all pending items in the stock update queue.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token to stop processing.</param>
    /// <returns>A task representing the queue processing operation.</returns>
    /// <remarks>
    /// Creates a new service scope for each processing cycle and attempts to handle each queued item.
    /// Failed updates are logged but don't prevent processing of subsequent items.
    /// </remarks>
    private async Task ProcessQueueAsync(CancellationToken cancellationToken)
    {
        if (_queue.Count == 0)
            return;

        _logger.LogInformation("Processing {Count} items in queue", _queue.Count);

        using var scope = _serviceProvider.CreateScope();

        // Get the stock update handler from DI
        var handler = scope.ServiceProvider.GetService<IStockUpdateHandler>();
        if (handler == null)
        {
            _logger.LogWarning("No IStockUpdateHandler registered. Queue items will not be processed.");
            return;
        }

        while (_queue.Count > 0 && !cancellationToken.IsCancellationRequested)
        {
            var request = await _queue.DequeueAsync();
            if (request == null)
                break;

            try
            {
                _logger.LogInformation(
                    "Processing stock update - ProductId: {ProductId}, Quantity: {Quantity}",
                    request.ProductId, request.Quantity);

                var success = await handler.HandleStockUpdateAsync(request.ProductId, request.Quantity);

                if (success)
                {
                    _logger.LogInformation(
                        "Successfully updated stock for ProductId: {ProductId}",
                        request.ProductId);
                }
                else
                {
                    _logger.LogWarning(
                        "Failed to update stock for ProductId: {ProductId} - Product not found",
                        request.ProductId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error updating stock for ProductId: {ProductId}",
                    request.ProductId);
            }
        }
    }
}
