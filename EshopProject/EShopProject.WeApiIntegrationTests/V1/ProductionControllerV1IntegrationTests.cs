using EShopProject.Entities.Entities;
using EShopProject.Entities.Interfaces;
using EShopProject.EShopDB.Data;
using EShopProject.EShopDB.Repositories;
using EShopProject.Services;
using EShopProject.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace EShopProject.WeApiIntegrationTest.V1;

/// <summary>
/// Integration tests for Product Controller V1.
/// </summary>
[TestClass]
public class ProductControllerV1IntegrationTests
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