//------------------------------------------------------------------------------------------
// File: QueueServiceCollectionExtensions.cs
//------------------------------------------------------------------------------------------
using EShopProject.MessageQueue.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EShopProject.MessageQueue.Extensions;

/// <summary>
/// Extension methods for registering queue services
/// </summary>
public static class QueueServiceCollectionExtensions
{
    /// <summary>
    /// Add In-Memory Stock Update Queue services
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="processingInterval">Queue processing interval (default: 2 seconds)</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddInMemoryStockUpdateQueue(
        this IServiceCollection services,
        TimeSpan? processingInterval = null)
    {
        // Register queue as singleton (shared across all requests)
        services.AddSingleton<IStockUpdateQueue, InMemoryStockUpdateQueue>();

        // Register background processor
        services.AddHostedService(provider =>
        {
            var queue = provider.GetRequiredService<IStockUpdateQueue>();
            var logger = provider.GetRequiredService<ILogger<StockUpdateQueueProcessor>>();
            return new StockUpdateQueueProcessor(provider, queue, logger, processingInterval);
        });

        return services;
    }
}
