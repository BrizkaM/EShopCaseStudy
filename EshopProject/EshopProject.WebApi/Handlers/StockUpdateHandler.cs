using EShopProject.MessageQueue.Interfaces;
using EShopProject.Services.Interfaces;

namespace EShopApi.WebApi.Handlers;

/// <summary>
/// Handler for processing stock updates from the queue
/// Implements the business logic for stock updates
/// This is the bridge between Queue infrastructure and Application layer
/// </summary>
public class StockUpdateHandler : IStockUpdateHandler
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<StockUpdateHandler> _logger;

    public StockUpdateHandler(
        IServiceScopeFactory serviceScopeFactory,
        ILogger<StockUpdateHandler> logger)
    {
        _serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Handle stock update request from queue
    /// Creates a new scope to resolve scoped services (DbContext, UnitOfWork, etc.)
    /// </summary>
    public async Task<bool> HandleStockUpdateAsync(int productId, int quantity)
    {
        _logger.LogInformation("Handling stock update for ProductId: {ProductId}, Quantity: {Quantity}",
            productId, quantity);

        try
        {
            // Create a new scope to get scoped services (like DbContext)
            // This is necessary because the background service is a singleton
            using var scope = _serviceScopeFactory.CreateScope();
            var productService = scope.ServiceProvider.GetRequiredService<IProductService>();

            // Execute the stock update through the service layer
            var result = await productService.UpdateProductStockAsync(productId, quantity);

            if (result)
            {
                _logger.LogInformation("Stock update completed successfully for ProductId: {ProductId}", productId);
            }
            else
            {
                _logger.LogWarning("Stock update failed for ProductId: {ProductId} - Product not found", productId);
            }

            return result;
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid stock update request for ProductId: {ProductId}", productId);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling stock update for ProductId: {ProductId}", productId);
            throw; // Re-throw to let the queue processor handle it
        }
    }
}