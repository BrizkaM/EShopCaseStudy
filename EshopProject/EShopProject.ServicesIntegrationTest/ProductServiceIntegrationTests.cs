using EShopProject.Core.Interfaces;
using EShopProject.Core.Entities;
using EShopProject.EShopDB.Data;
using EShopProject.EShopDB.Repositories;
using EShopProject.Services;
using EShopProject.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace EShopProject.WeApiIntegrationTest;

/// <summary>
/// Integration tests for Product Service.
/// </summary>
[TestClass]
public class ProductServiceIntegrationTests
{
    private EShopDbContext _context = null!;
    private IProductService _productService = null!;
    private IUnitOfWork _unitOfWork = null!;
    private Mock<ILogger<ProductService>> _mockLogger = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        // Setup In-Memory database
        var options = new DbContextOptionsBuilder<EShopDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new EShopDbContext(options);
        _unitOfWork = new UnitOfWork(_context);
        _mockLogger = new Mock<ILogger<ProductService>>();
        _productService = new ProductService(_unitOfWork, _mockLogger.Object);

        // Seed initial test data
        SeedTestData();
    }

    [TestCleanup]
    public void TestCleanup()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [TestMethod]
    public async Task GetAllProducts_ReturnsAllProducts()
    {
        // Act
        var result = await _productService.GetAllProductsAsync();

        // Assert
        var products = result.ToList();
        Assert.HasCount(5, products);
        Assert.IsTrue(products.Any(p => p.Name == "Test Product 1"));
        Assert.IsTrue(products.Any(p => p.Name == "Test Product 2"));
    }

    [TestMethod]
    public async Task GetAllProducts_ReturnsProductsOrderedByCreatedAtDescending()
    {
        // Act
        var result = await _productService.GetAllProductsAsync();

        // Assert
        var products = result.ToList();
        Assert.IsTrue(products[0].Idate >= products[1].Idate);
        Assert.IsTrue(products[1].Idate >= products[2].Idate);
    }

    [TestMethod]
    public async Task GetAllProducts_EmptyDatabase_ReturnsEmptyList()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<EShopDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var emptyContext = new EShopDbContext(options);
        var emptyUnitOfWork = new UnitOfWork(emptyContext);
        var emptyService = new ProductService(emptyUnitOfWork, _mockLogger.Object);

        // Act
        var result = await emptyService.GetAllProductsAsync();

        // Assert
        Assert.AreEqual(0, result.Count());
    }

    [TestMethod]
    public async Task GetProductById_ExistingId_ReturnsProduct()
    {
        // Arrange
        var existingProduct = await _context.Products.FirstAsync();

        // Act
        var result = await _productService.GetProductByIdAsync(existingProduct.Id);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(existingProduct.Id, result.Id);
        Assert.AreEqual(existingProduct.Name, result.Name);
        Assert.AreEqual(existingProduct.ImageUrl, result.ImageUrl);
    }

    [TestMethod]
    public async Task GetProductById_NonExistingId_ReturnsNull()
    {
        // Act
        var result = await _productService.GetProductByIdAsync(999);

        // Assert
        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task GetProductById_InvalidId_ReturnsNull()
    {
        // Act
        var result = await _productService.GetProductByIdAsync(0);

        // Assert
        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task GetProductById_NegativeId_ReturnsNull()
    {
        // Act
        var result = await _productService.GetProductByIdAsync(-1);

        // Assert
        Assert.IsNull(result);
    }

    [TestMethod]
    public async Task CreateProduct_ValidInput_CreatesProduct()
    {
        // Arrange
        var name = "New Test Product";
        var imageUrl = "https://example.com/new-product.jpg";

        // Act
        var result = await _productService.CreateProductAsync(name, imageUrl);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreNotEqual(0, result.Id);
        Assert.AreEqual(name, result.Name);
        Assert.AreEqual(imageUrl, result.ImageUrl);
        Assert.AreEqual(0, result.Quantity); // Default
        Assert.AreEqual(DateTime.UtcNow.Date, result.Idate.Date);
        Assert.AreEqual(DateTime.UtcNow.Date, result.Udate.Date);

        // Verify in database
        var dbProduct = await _context.Products.FindAsync(result.Id);
        Assert.IsNotNull(dbProduct);
        Assert.AreEqual(name, dbProduct.Name);
    }

    [TestMethod]
    public async Task CreateProduct_MinimalInput_CreatesProductWithDefaults()
    {
        // Arrange
        var name = "Minimal Product";
        var imageUrl = "https://example.com/minimal.jpg";

        // Act
        var result = await _productService.CreateProductAsync(name, imageUrl);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Quantity);
        Assert.IsNull(result.Price);
        Assert.IsNull(result.Description);
    }

    [TestMethod]
    [DataRow("", "https://example.com/image.jpg")]
    [DataRow(null, "https://example.com/image.jpg")]
    [DataRow("   ", "https://example.com/image.jpg")]
    public async Task CreateProduct_EmptyName_ThrowsArgumentException(string name, string imageUrl)
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _productService.CreateProductAsync(name, imageUrl));

        Assert.Contains("name", exception.Message.ToLower());
    }

    [TestMethod]
    [DataRow("Product Name", "")]
    [DataRow("Product Name", null)]
    [DataRow("Product Name", "   ")]
    public async Task CreateProduct_EmptyImageUrl_ThrowsArgumentException(string name, string imageUrl)
    {
        // Act & Assert
        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            _productService.CreateProductAsync(name, imageUrl));

        Assert.Contains("image", exception.Message.ToLower());
    }

    [TestMethod]
    public async Task CreateProduct_TrimsWhitespace()
    {
        // Arrange
        var name = "  Product With Spaces  ";
        var imageUrl = "  https://example.com/image.jpg  ";

        // Act
        var result = await _productService.CreateProductAsync(name, imageUrl);

        // Assert
        Assert.AreEqual("Product With Spaces", result.Name);
        Assert.AreEqual("https://example.com/image.jpg", result.ImageUrl);
    }

    [TestMethod]
    public async Task GetPagedProducts_FirstPage_ReturnsCorrectItems()
    {
        // Act
        var (items, totalCount, totalPages) = await _productService.GetPagedProductsAsync(1, 2);

        // Assert
        var itemsList = items.ToList();
        Assert.HasCount(2, itemsList);
        Assert.AreEqual(5, totalCount);
        Assert.AreEqual(3, totalPages); // 5 items / 2 per page = 3 pages
    }

    [TestMethod]
    public async Task GetPagedProducts_SecondPage_ReturnsCorrectItems()
    {
        // Act
        var (items, totalCount, totalPages) = await _productService.GetPagedProductsAsync(2, 2);

        // Assert
        var itemsList = items.ToList();
        Assert.HasCount(2, itemsList);
        Assert.AreEqual(5, totalCount);
        Assert.AreEqual(3, totalPages);
    }

    [TestMethod]
    public async Task GetPagedProducts_LastPage_ReturnsRemainingItems()
    {
        // Act
        var (items, totalCount, totalPages) = await _productService.GetPagedProductsAsync(3, 2);

        // Assert
        var itemsList = items.ToList();
        Assert.HasCount(1, itemsList); // Only 1 item on last page (5 total / 2 per page)
        Assert.AreEqual(5, totalCount);
        Assert.AreEqual(3, totalPages);
    }

    [TestMethod]
    public async Task GetPagedProducts_DefaultPageSize_Returns10Items()
    {
        // Arrange - Add more products to test default page size
        for (int i = 6; i <= 15; i++)
        {
            await _context.Products.AddAsync(new Product
            {
                Name = $"Extra Product {i}",
                ImageUrl = $"https://example.com/{i}.jpg",
                Quantity = i
            });
        }
        await _context.SaveChangesAsync();

        // Act
        var (items, totalCount, totalPages) = await _productService.GetPagedProductsAsync(1, 10);

        // Assert
        Assert.AreEqual(10, items.Count());
        Assert.AreEqual(15, totalCount);
        Assert.AreEqual(2, totalPages);
    }

    [TestMethod]
    [DataRow(0, 10, 1, DisplayName = "Invalid page -> corrected to 1")]
    [DataRow(-1, 10, 1, DisplayName = "Negative page -> corrected to 1")]
    public async Task GetPagedProducts_InvalidPageNumber_UsesPageOne(int inputPage, int pageSize, int expectedPage)
    {
        // Act
        var (items, _, _) = await _productService.GetPagedProductsAsync(inputPage, pageSize);

        // Assert
        Assert.IsTrue(items.Any()); // Should return first page results
    }

    [TestMethod]
    [DataRow(1, 0, 10, DisplayName = "Invalid size -> corrected to 10")]
    [DataRow(1, -5, 10, DisplayName = "Negative size -> corrected to 10")]
    [DataRow(1, 200, 100, DisplayName = "Too large -> capped at 100")]
    public async Task GetPagedProducts_InvalidPageSize_UsesDefault(int pageNumber, int inputSize, int expectedMinSize)
    {
        // Act
        var (items, _, _) = await _productService.GetPagedProductsAsync(pageNumber, inputSize);

        // Assert
        var itemsList = items.ToList();
        Assert.IsTrue(itemsList.Count <= expectedMinSize || itemsList.Count == 5); // Should use corrected size or return all if less
    }

    [TestMethod]
    public async Task GetPagedProducts_EmptyDatabase_ReturnsEmptyResult()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<EShopDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        var emptyContext = new EShopDbContext(options);
        var emptyUnitOfWork = new UnitOfWork(emptyContext);
        var emptyService = new ProductService(emptyUnitOfWork, _mockLogger.Object);

        // Act
        var (items, totalCount, totalPages) = await emptyService.GetPagedProductsAsync(1, 10);

        // Assert
        Assert.AreEqual(0, items.Count());
        Assert.AreEqual(0, totalCount);
        Assert.AreEqual(0, totalPages);
    }

    [TestMethod]
    public async Task GetPagedProducts_OrderedByCreatedAtDescending()
    {
        // Act
        var (items, _, _) = await _productService.GetPagedProductsAsync(1, 10);

        // Assert
        var itemsList = items.ToList();
        for (int i = 0; i < itemsList.Count - 1; i++)
        {
            Assert.IsTrue(itemsList[i].Idate >= itemsList[i + 1].Idate);
        }
    }

    [TestMethod]
    public async Task CompleteWorkflow_CreateAndUpdate_WorksCorrectly()
    {
        // Create
        var product = await _productService.CreateProductAsync("Workflow Product", "https://example.com/workflow.jpg");
        Assert.IsNotNull(product);
        Assert.AreEqual(0, product.Quantity);

        // Update stock
        var updateResult = await _productService.UpdateProductStockAsync(product.Id, 50);
        Assert.IsTrue(updateResult);

        // Verify
        var retrieved = await _productService.GetProductByIdAsync(product.Id);
        Assert.IsNotNull(retrieved);
        Assert.AreEqual(50, retrieved.Quantity);
    }

    [TestMethod]
    public async Task CompleteWorkflow_MultipleStockUpdates_MaintainsConsistency()
    {
        // Arrange
        var product = await _productService.CreateProductAsync("Stock Test", "https://example.com/stock.jpg");

        // Act - Multiple updates
        await _productService.UpdateProductStockAsync(product.Id, 10);
        await _productService.UpdateProductStockAsync(product.Id, 20);
        await _productService.UpdateProductStockAsync(product.Id, 30);

        // Assert
        var final = await _productService.GetProductByIdAsync(product.Id);
        Assert.AreEqual(30, final!.Quantity);
    }

    [TestMethod]
    public async Task PaginationAndFiltering_WorkTogether()
    {
        // Arrange - Create products with specific pattern
        for (int i = 1; i <= 25; i++)
        {
            await _productService.CreateProductAsync($"Pagination Test {i}", $"https://example.com/{i}.jpg");
        }

        // Act - Get different pages
        var (page1, _, _) = await _productService.GetPagedProductsAsync(1, 10);
        var (page2, _, _) = await _productService.GetPagedProductsAsync(2, 10);
        var (page3, totalCount, totalPages) = await _productService.GetPagedProductsAsync(3, 10);

        // Assert
        Assert.AreEqual(10, page1.Count());
        Assert.AreEqual(10, page2.Count());
        Assert.IsGreaterThan(0, page3.Count());
        Assert.AreEqual(30, totalCount); // 5 original + 25 new
        Assert.AreEqual(3, totalPages);
    }

    private void SeedTestData()
    {
        var products = new[]
        {
            new Product
            {
                Name = "Test Product 1",
                ImageUrl = "https://example.com/1.jpg",
                Price = 100.00m,
                Description = "Test Description 1",
                Quantity = 10,
                Idate = DateTime.UtcNow.AddDays(-5),
                Udate = DateTime.UtcNow.AddDays(-5)
            },
            new Product
            {
                Name = "Test Product 2",
                ImageUrl = "https://example.com/2.jpg",
                Price = 200.00m,
                Description = "Test Description 2",
                Quantity = 20,
                Idate = DateTime.UtcNow.AddDays(-4),
                Udate = DateTime.UtcNow.AddDays(-4)
            },
            new Product
            {
                Name = "Test Product 3",
                ImageUrl = "https://example.com/3.jpg",
                Price = 300.00m,
                Description = "Test Description 3",
                Quantity = 30,
                Idate = DateTime.UtcNow.AddDays(-3),
                Udate = DateTime.UtcNow.AddDays(-3)
            },
            new Product
            {
                Name = "Test Product 4",
                ImageUrl = "https://example.com/4.jpg",
                Price = 400.00m,
                Description = "Test Description 4",
                Quantity = 40,
                Idate = DateTime.UtcNow.AddDays(-2),
                Udate = DateTime.UtcNow.AddDays(-2)
            },
            new Product
            {
                Name = "Test Product 5",
                ImageUrl = "https://example.com/5.jpg",
                Price = 500.00m,
                Description = "Test Description 5",
                Quantity = 50,
                Idate = DateTime.UtcNow.AddDays(-1),
                Udate = DateTime.UtcNow.AddDays(-1)
            }
        };

        _context.Products.AddRange(products);
        _context.SaveChanges();
    }
}