using EShopProject.Business.Entities;
using EShopProject.Services.Interfaces;

namespace EShopProject.Services;

/// <summary>
/// Service for product business logic
/// </summary>
public class ProductService : IProductService
{
    public async Task<IEnumerable<Product>> GetAllProductsAsync()
    {
        throw new NotImplementedException();
    }

    public async Task<Product?> GetProductByIdAsync(int id)
    {
        throw new NotImplementedException();
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