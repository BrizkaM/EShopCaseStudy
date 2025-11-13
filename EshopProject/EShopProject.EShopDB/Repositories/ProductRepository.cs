//------------------------------------------------------------------------------------------
// File: ProductRepository.cs
//------------------------------------------------------------------------------------------
using EShopProject.Core.Entities;
using EShopProject.Core.Interfaces;
using EShopProject.EShopDB.Data;
using Microsoft.EntityFrameworkCore;

namespace EShopProject.EShopDB.Repositories;

/// <summary>
/// Repository implementation for Product entity
/// </summary>
public class ProductRepository : IProductRepository
{
    private readonly EShopDbContext _context;

    /// <summary>
    /// Initializes a new instance of the ProductRepository class
    /// </summary>
    /// <param name="context">The database context</param>
    /// <exception cref="ArgumentNullException">Thrown when context is null</exception>
    public ProductRepository(EShopDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <summary>
    /// Retrieves all products ordered by insertion date descending
    /// </summary>
    /// <returns>A collection of all products</returns>
    public async Task<IEnumerable<Product>> GetAllAsync()
    {
        return await _context.Products
            .OrderByDescending(p => p.Idate)
            .ToListAsync();
    }

    /// <summary>
    /// Retrieves a product by its identifier
    /// </summary>
    /// <param name="id">The product identifier</param>
    /// <returns>The product if found; otherwise, null</returns>
    public async Task<Product?> GetByIdAsync(int id)
    {
        return await _context.Products.FindAsync(id);
    }

    /// <summary>
    /// Adds a new product to the database
    /// </summary>
    /// <param name="product">The product to add</param>
    /// <returns>The added product with updated information</returns>
    public async Task<Product> AddAsync(Product product)
    {
        var entry = await _context.Products.AddAsync(product);
        return entry.Entity;
    }

    /// <summary>
    /// Updates an existing product in the database
    /// </summary>
    /// <param name="product">The product with updated information</param>
    /// <returns>A task representing the asynchronous operation</returns>
    public Task UpdateAsync(Product product)
    {
        _context.Products.Update(product);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Retrieves a paginated list of products ordered by insertion date descending
    /// </summary>
    /// <param name="pageNumber">The page number to retrieve (1-based)</param>
    /// <param name="pageSize">The number of items per page</param>
    /// <returns>A tuple containing the page of products and the total count of all products</returns>
    public async Task<(IEnumerable<Product> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize)
    {
        var totalCount = await _context.Products.CountAsync();

        var items = await _context.Products
            .OrderByDescending(p => p.Idate)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }
}