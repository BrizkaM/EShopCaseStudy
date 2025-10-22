using EShopProject.Entities.Entities;

namespace EShopProject.Services.Interfaces;

/// <summary>
/// Service interface for product business logic
/// </summary>
public interface IProductService
{
    Task<IEnumerable<Product>> GetAllProductsAsync();
    Task<Product?> GetProductByIdAsync(int id);
    Task<Product> CreateProductAsync(string name, string imageUrl);
    Task<bool> UpdateProductStockAsync(int id, int quantity);
}