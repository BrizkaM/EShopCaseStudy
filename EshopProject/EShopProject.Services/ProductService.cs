using EShopProject.Entities.Entities;
using EShopProject.Entities.Interfaces;
using EShopProject.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace EShopProject.Services;

/// <summary>
/// Service for product business logic
/// </summary>
public class ProductService : IProductService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ProductService> _logger;

    public ProductService(IUnitOfWork unitOfWork, ILogger<ProductService> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IEnumerable<Product>> GetAllProductsAsync()
    {
        throw new NotImplementedException();
    }

    public async Task<Product?> GetProductByIdAsync(int id)
    {
        _logger.LogInformation("Retrieving product with ID: {ProductId}", id);

        if (id <= 0)
        {
            _logger.LogWarning("Invalid product ID: {ProductId}", id);
            return null;
        }

        return await _unitOfWork.Products.GetByIdAsync(id);
    }

    public async Task<Product> CreateProductAsync(string name, string imageUrl)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> UpdateProductStockAsync(int id, int quantity)
    {
        throw new NotImplementedException();
    }
}