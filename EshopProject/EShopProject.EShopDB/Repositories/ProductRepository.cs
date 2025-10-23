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

    public ProductRepository(EShopDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<IEnumerable<Product>> GetAllAsync()
    {
        return await _context.Products
            .OrderByDescending(p => p.Idate)
            .ToListAsync();
    }

    public async Task<Product?> GetByIdAsync(int id)
    {
        return await _context.Products.FindAsync(id);
    }

    public async Task<Product> AddAsync(Product product)
    {
        var entry = await _context.Products.AddAsync(product);
        return entry.Entity;
    }

    public Task UpdateAsync(Product product)
    {
        _context.Products.Update(product);
        return Task.CompletedTask;
    }

    public async Task DeleteAsync(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product != null)
        {
            _context.Products.Remove(product);
        }
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Products.AnyAsync(p => p.Id == id);
    }

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