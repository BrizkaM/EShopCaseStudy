//------------------------------------------------------------------------------------------
// File: UnitOfWork.cs
//------------------------------------------------------------------------------------------
using EShopProject.Core.Interfaces;
using EShopProject.EShopDB.Data;

using Microsoft.EntityFrameworkCore.Storage;

namespace EShopProject.EShopDB.Repositories;

/// <summary>
/// Unit of Work implementation for managing database transactions
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly EShopDbContext _context;
    private IDbContextTransaction? _transaction;
    private IProductRepository? _productRepository;

    /// <summary>
    /// Initializes a new instance of the UnitOfWork class
    /// </summary>
    /// <param name="context">The database context</param>
    /// <exception cref="ArgumentNullException">Thrown when context is null</exception>
    public UnitOfWork(EShopDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <summary>
    /// Gets the product repository instance
    /// </summary>
    public IProductRepository Products
    {
        get
        {
            return _productRepository ??= new ProductRepository(_context);
        }
    }

    /// <summary>
    /// Saves all pending changes to the database
    /// </summary>
    /// <returns>The number of affected records in the database</returns>
    public async Task<int> CompleteAsync()
    {
        return await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Begins a new database transaction
    /// </summary>
    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    /// <summary>
    /// Commits the current transaction and saves all changes permanently
    /// </summary>
    public async Task CommitTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    /// <summary>
    /// Rolls back the current transaction and discards all changes
    /// </summary>
    public async Task RollbackTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    /// <summary>
    /// Disposes the current transaction and database context
    /// </summary>
    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}