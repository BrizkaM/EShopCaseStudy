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
        _logger.LogInformation("Retrieving all products");
        return await _unitOfWork.Products.GetAllAsync();
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
        _logger.LogInformation("Creating new product: {ProductName}", name);

        // Business validation
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Product name cannot be empty", nameof(name));

        if (string.IsNullOrWhiteSpace(imageUrl))
            throw new ArgumentException("Image URL cannot be empty", nameof(imageUrl));

        // Create product entity
        var product = new Product
        {
            Name = name.Trim(),
            ImageUrl = imageUrl.Trim(),
            Quantity = 0, // Default stock
            Idate = DateTime.UtcNow,
            Udate = DateTime.UtcNow
        };

        // Add to repository
        var createdProduct = await _unitOfWork.Products.AddAsync(product);
        await _unitOfWork.CompleteAsync();

        _logger.LogInformation("Product created successfully with ID: {ProductId}", createdProduct.Id);
        return createdProduct;
    }

    public async Task<bool> UpdateProductStockAsync(int id, int quantity)
    {
        _logger.LogInformation("Updating stock for product ID: {ProductId} to quantity: {Quantity}", id, quantity);

        // Business validation
        if (id <= 0)
        {
            _logger.LogWarning("Invalid product ID: {ProductId}", id);
            return false;
        }

        if (quantity < 0)
        {
            _logger.LogWarning("Invalid quantity: {Quantity}", quantity);
            throw new ArgumentException("Quantity cannot be negative", nameof(quantity));
        }

        // Get product
        var product = await _unitOfWork.Products.GetByIdAsync(id);
        if (product == null)
        {
            _logger.LogWarning("Product not found: {ProductId}", id);
            return false;
        }

        // Update stock (business logic could include inventory checks, notifications, etc.)
        product.Quantity = quantity;
        product.Udate = DateTime.UtcNow;

        await _unitOfWork.Products.UpdateAsync(product);
        await _unitOfWork.CompleteAsync();

        _logger.LogInformation("Stock updated successfully for product ID: {ProductId}", id);
        return true;
    }
}