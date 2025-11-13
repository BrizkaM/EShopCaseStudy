//------------------------------------------------------------------------------------------
// File: IProductRepository.cs
//------------------------------------------------------------------------------------------
using EShopProject.Core.Entities;

namespace EShopProject.Core.Interfaces;

/// <summary>
/// Repository interface for Product entity operations
/// </summary>
public interface IProductRepository
{
    Task<IEnumerable<Product>> GetAllAsync();
    Task<Product?> GetByIdAsync(int id);
    Task<Product> AddAsync(Product product);
    Task UpdateAsync(Product product);
    // For pagination (v2)
    Task<(IEnumerable<Product> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize);
}