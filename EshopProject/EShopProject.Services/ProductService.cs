//------------------------------------------------------------------------------------------
// File: ProductsControllerV1Tests.cs
//------------------------------------------------------------------------------------------
using EShopProject.Core.Entities;
using EShopProject.Core.Interfaces;
using EShopProject.Services.Interfaces;
using Microsoft.Extensions.Logging;

/// <summary>
/// Service for product business logic
/// </summary>
public class ProductService : IProductService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ProductService> _logger;

    /// <summary>
    /// Initializes a new instance of the ProductService class.
    /// </summary>
    /// <param name="unitOfWork">The unit of work instance for handling database transactions.</param>
    /// <param name="logger">The logger instance for logging service operations.</param>
    /// <exception cref="ArgumentNullException">Thrown when unitOfWork or logger is null.</exception>
    public ProductService(IUnitOfWork unitOfWork, ILogger<ProductService> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Retrieves all products from the database asynchronously.
    /// </summary>
    /// <returns>A collection of all products.</returns>
    public async Task<IEnumerable<Product>> GetAllProductsAsync()
    {
        _logger.LogInformation("Retrieving all products");
        return await _unitOfWork.Products.GetAllAsync();
    }

    /// <summary>
    /// Retrieves a product by its unique identifier asynchronously.
    /// </summary>
    /// <param name="id">The unique identifier of the product.</param>
    /// <returns>The product if found; otherwise, null.</returns>
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

    /// <summary>
    /// Creates a new product asynchronously.
    /// </summary>
    /// <param name="name">The name of the product.</param>
    /// <param name="imageUrl">The URL of the product's image.</param>
    /// <returns>The created product.</returns>
    /// <exception cref="ArgumentException">Thrown when name or imageUrl is empty or whitespace.</exception>
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

    /// <summary>
    /// Updates the stock quantity of a product asynchronously.
    /// </summary>
    /// <param name="id">The unique identifier of the product.</param>
    /// <param name="quantity">The new quantity to set.</param>
    /// <returns>True if the update was successful; otherwise, false.</returns>
    /// <exception cref="ArgumentException">Thrown when quantity is negative.</exception>
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

    /// <summary>
    /// Retrieves a paginated list of products asynchronously.
    /// </summary>
    /// <param name="pageNumber">The page number to retrieve (1-based).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <returns>A tuple containing the items, total count, and total pages.</returns>
    public async Task<(IEnumerable<Product> Items, int TotalCount, int TotalPages)> GetPagedProductsAsync(
    int pageNumber, int pageSize)
    {
        _logger.LogInformation("Retrieving paginated products - Page: {PageNumber}, Size: {PageSize}",
            pageNumber, pageSize);

        // Business validation
        if (pageNumber < 1) pageNumber = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100; // Max page size

        var (items, totalCount) = await _unitOfWork.Products.GetPagedAsync(pageNumber, pageSize);
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        return (items, totalCount, totalPages);
    }
}