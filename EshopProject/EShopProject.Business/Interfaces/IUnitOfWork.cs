namespace EShopProject.Core.Interfaces;

/// <summary>
/// Unit of Work pattern for managing database transactions
/// </summary>
public interface IUnitOfWork : IDisposable
{
    IProductRepository Products { get; }
    Task<int> CompleteAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}