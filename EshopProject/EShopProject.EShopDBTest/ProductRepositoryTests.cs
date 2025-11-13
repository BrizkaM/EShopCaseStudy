//------------------------------------------------------------------------------------------
// File: ProductRepositoryTests.cs
//------------------------------------------------------------------------------------------
using EShopProject.Core.Entities;
using EShopProject.EShopDB.Data;
using EShopProject.EShopDB.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EShopProject.EShopDBTest;

/// <summary>
/// Integration tests for ProductRepository using In-Memory database
/// Tests the complete data access layer with Entity Framework Core
/// </summary>
[TestClass]
public class ProductRepositoryTests
{
    private EShopDbContext _context = null!;
    private ProductRepository _repository = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        var options = new DbContextOptionsBuilder<EShopDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new EShopDbContext(options);
        _repository = new ProductRepository(_context);
    }

    [TestCleanup]
    public void TestCleanup()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    /// <summary>
    /// Test: GetAllAsync returns all products ordered by CreatedAt descending
    /// Verifies that repository retrieves all products and applies correct sorting
    /// </summary>
    [TestMethod]
    public async Task ProductRepository_GetAllAsync_ReturnsAllProducts_OrderedByCreatedAtDescending()
    {
        // Arrange
        var products = new[]
        {
            new Product { Name = "Product 1", ImageUrl = "url1", Idate = DateTime.UtcNow.AddDays(-2), Udate = DateTime.UtcNow.AddDays(-2) },
            new Product { Name = "Product 2", ImageUrl = "url2", Idate = DateTime.UtcNow.AddDays(-1), Udate = DateTime.UtcNow.AddDays(-1) },
            new Product { Name = "Product 3", ImageUrl = "url3", Idate = DateTime.UtcNow, Udate = DateTime.UtcNow }
        };

        await _context.Products.AddRangeAsync(products);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        var productList = result.ToList();
        Assert.HasCount(3, productList);
        Assert.AreEqual("Product 3", productList[0].Name); // Most recent first
        Assert.IsTrue(productList[0].Idate >= productList[1].Idate);
        Assert.IsTrue(productList[1].Idate >= productList[2].Idate);
    }

    /// <summary>
    /// Test: GetByIdAsync with existing ID returns the correct product
    /// Verifies that repository can retrieve a single product by its ID
    /// </summary>
    [TestMethod]
    public async Task ProductRepository_GetByIdAsync_ExistingId_ReturnsProduct()
    {
        // Arrange
        var product = new Product { Name = "Test Product", ImageUrl = "test-url", Quantity = 10 };
        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(product.Id);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("Test Product", result.Name);
        Assert.AreEqual("test-url", result.ImageUrl);
        Assert.AreEqual(10, result.Quantity);
    }

    /// <summary>
    /// Test: GetByIdAsync with non-existing ID returns null
    /// Verifies that repository handles missing products gracefully
    /// </summary>
    [TestMethod]
    public async Task ProductRepository_GetByIdAsync_NonExistingId_ReturnsNull()
    {
        // Act
        var result = await _repository.GetByIdAsync(999);

        // Assert
        Assert.IsNull(result);
    }

    /// <summary>
    /// Test: AddAsync adds a new product to the database
    /// Verifies that repository can create new products and generate IDs
    /// </summary>
    [TestMethod]
    public async Task ProductRepository_AddAsync_AddsProductToDatabase()
    {
        // Arrange
        var product = new Product
        {
            Name = "New Product",
            ImageUrl = "https://example.com/new.jpg",
            Price = 199.99m,
            Quantity = 5,
            Description = "Test description"
        };

        // Act
        var result = await _repository.AddAsync(product);
        await _context.SaveChangesAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreNotEqual(0, result.Id);

        var savedProduct = await _context.Products.FindAsync(result.Id);
        Assert.IsNotNull(savedProduct);
        Assert.AreEqual("New Product", savedProduct.Name);
        Assert.AreEqual(199.99m, savedProduct.Price);
    }

    /// <summary>
    /// Test: UpdateAsync updates an existing product in the database
    /// Verifies that repository can modify product properties
    /// </summary>
    [TestMethod]
    public async Task ProductRepository_UpdateAsync_UpdatesExistingProduct()
    {
        // Arrange
        var product = new Product
        {
            Name = "Original",
            ImageUrl = "url",
            Quantity = 10,
            Price = 100m
        };
        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();

        // Act
        product.Name = "Updated";
        product.Quantity = 20;
        product.Price = 200m;
        await _repository.UpdateAsync(product);
        await _context.SaveChangesAsync();

        // Assert
        var updatedProduct = await _context.Products.FindAsync(product.Id);
        Assert.IsNotNull(updatedProduct);
        Assert.AreEqual("Updated", updatedProduct.Name);
        Assert.AreEqual(20, updatedProduct.Quantity);
        Assert.AreEqual(200m, updatedProduct.Price);
    }

    /// <summary>
    /// Test: GetPagedAsync returns correct page of products
    /// Verifies pagination logic with correct item count and total
    /// </summary>
    [TestMethod]
    public async Task ProductRepository_GetPagedAsync_ReturnsCorrectPage()
    {
        // Arrange
        for (int i = 1; i <= 25; i++)
        {
            await _context.Products.AddAsync(new Product
            {
                Name = $"Product {i}",
                ImageUrl = $"url{i}",
                Idate = DateTime.UtcNow.AddMinutes(-i),
                Udate = DateTime.UtcNow.AddMinutes(-i)
            });
        }
        await _context.SaveChangesAsync();

        // Act
        var (items, totalCount) = await _repository.GetPagedAsync(2, 10);

        // Assert
        var itemsList = items.ToList();
        Assert.HasCount(10, itemsList);
        Assert.AreEqual(25, totalCount);
    }

    /// <summary>
    /// Test: GetPagedAsync returns remaining items on last page
    /// Verifies pagination handles partial pages correctly
    /// </summary>
    [TestMethod]
    public async Task ProductRepository_GetPagedAsync_LastPage_ReturnsRemainingItems()
    {
        // Arrange
        for (int i = 1; i <= 25; i++)
        {
            await _context.Products.AddAsync(new Product
            {
                Name = $"Product {i}",
                ImageUrl = $"url{i}",
                Idate = DateTime.UtcNow.AddMinutes(-i),
                Udate = DateTime.UtcNow.AddMinutes(-i)
            });
        }
        await _context.SaveChangesAsync();

        // Act
        var (items, totalCount) = await _repository.GetPagedAsync(3, 10);

        // Assert
        var itemsList = items.ToList();
        Assert.HasCount(5, itemsList); // 25 total, page 3 of size 10 = 5 items
        Assert.AreEqual(25, totalCount);
    }

    /// <summary>
    /// Test: GetPagedAsync returns empty page when page number exceeds total pages
    /// Verifies pagination handles out-of-range page numbers
    /// </summary>
    [TestMethod]
    public async Task ProductRepository_GetPagedAsync_PageBeyondTotal_ReturnsEmptyList()
    {
        // Arrange
        for (int i = 1; i <= 5; i++)
        {
            await _context.Products.AddAsync(new Product
            {
                Name = $"Product {i}",
                ImageUrl = $"url{i}"
            });
        }
        await _context.SaveChangesAsync();

        // Act
        var (items, totalCount) = await _repository.GetPagedAsync(10, 10);

        // Assert
        Assert.AreEqual(0, items.Count());
        Assert.AreEqual(5, totalCount);
    }

    /// <summary>
    /// Test: GetPagedAsync orders results by CreatedAt descending
    /// Verifies that pagination maintains correct sort order
    /// </summary>
    [TestMethod]
    public async Task ProductRepository_GetPagedAsync_OrdersByCreatedAtDescending()
    {
        // Arrange
        var products = new[]
        {
            new Product { Name = "Oldest", ImageUrl = "url1", Idate = DateTime.UtcNow.AddDays(-3), Udate = DateTime.UtcNow.AddDays(-3) },
            new Product { Name = "Middle", ImageUrl = "url2", Idate = DateTime.UtcNow.AddDays(-2), Udate = DateTime.UtcNow.AddDays(-2) },
            new Product { Name = "Newest", ImageUrl = "url3", Idate = DateTime.UtcNow.AddDays(-1), Udate = DateTime.UtcNow.AddDays(-1) }
        };

        await _context.Products.AddRangeAsync(products);
        await _context.SaveChangesAsync();

        // Act
        var (items, _) = await _repository.GetPagedAsync(1, 10);

        // Assert
        var itemsList = items.ToList();
        Assert.AreEqual("Newest", itemsList[0].Name);
        Assert.AreEqual("Middle", itemsList[1].Name);
        Assert.AreEqual("Oldest", itemsList[2].Name);
    }

    /// <summary>
    /// Test: AddAsync preserves all product properties
    /// Verifies that all fields are correctly saved to database
    /// </summary>
    [TestMethod]
    public async Task ProductRepository_AddAsync_PreservesAllProperties()
    {
        // Arrange
        var product = new Product
        {
            Name = "Complete Product",
            ImageUrl = "https://example.com/image.jpg",
            Price = 299.99m,
            Description = "Full description",
            Quantity = 42,
            Idate = DateTime.UtcNow,
            Udate = DateTime.UtcNow
        };

        // Act
        var result = await _repository.AddAsync(product);
        await _context.SaveChangesAsync();

        // Assert
        var savedProduct = await _context.Products.FindAsync(result.Id);
        Assert.IsNotNull(savedProduct);
        Assert.AreEqual("Complete Product", savedProduct.Name);
        Assert.AreEqual("https://example.com/image.jpg", savedProduct.ImageUrl);
        Assert.AreEqual(299.99m, savedProduct.Price);
        Assert.AreEqual("Full description", savedProduct.Description);
        Assert.AreEqual(42, savedProduct.Quantity);
    }

    /// <summary>
    /// Test: GetPagedAsync with page size 1 returns single item
    /// Verifies pagination works with smallest page size
    /// </summary>
    [TestMethod]
    public async Task ProductRepository_GetPagedAsync_PageSizeOne_ReturnsSingleItem()
    {
        // Arrange
        for (int i = 1; i <= 5; i++)
        {
            await _context.Products.AddAsync(new Product
            {
                Name = $"Product {i}",
                ImageUrl = $"url{i}"
            });
        }
        await _context.SaveChangesAsync();

        // Act
        var (items, totalCount) = await _repository.GetPagedAsync(2, 1);

        // Assert
        Assert.AreEqual(1, items.Count());
        Assert.AreEqual(5, totalCount);
    }

    /// <summary>
    /// Test: GetPagedAsync with empty database returns empty result
    /// Verifies pagination handles empty database correctly
    /// </summary>
    [TestMethod]
    public async Task ProductRepository_GetPagedAsync_EmptyDatabase_ReturnsEmptyResult()
    {
        // Act
        var (items, totalCount) = await _repository.GetPagedAsync(1, 10);

        // Assert
        Assert.AreEqual(0, items.Count());
        Assert.AreEqual(0, totalCount);
    }
}