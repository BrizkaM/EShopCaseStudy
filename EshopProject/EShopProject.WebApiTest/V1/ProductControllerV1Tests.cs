using EShopProject.Core.Entities;
using EShopProject.Services;
using EShopProject.Services.Interfaces;
using EShopProject.Services.ServiceInputs;
using EShopProject.Services.ServiceOutputs;
using EShopProject.WebApi.Controllers.V1;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace EShopProject.WebApiTest.V1;

/// <summary>
/// Unit tests for ProductsController V1
/// </summary>
[TestClass]
public class ProductsControllerV1Tests
{
    private Mock<IProductService> _mockProductService = null!;
    private Mock<ILogger<ProductsController>> _mockLogger = null!;
    private ProductsController _controller = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        _mockProductService = new Mock<IProductService>();
        _mockLogger = new Mock<ILogger<ProductsController>>();
        _controller = new ProductsController(_mockProductService.Object, _mockLogger.Object);
    }

    [TestMethod]
    public async Task GetAllProducts_WithProducts_ReturnsOkResult()
    {
        // Arrange
        var products = GetTestProducts();
        _mockProductService.Setup(s => s.GetAllProductsAsync())
            .ReturnsAsync(products);

        // Act
        var result = await _controller.GetAllProducts();

        // Assert
        var okResult = result.Result as OkObjectResult;
        Assert.IsNotNull(okResult);
        var response = okResult.Value as ApiResponse<IEnumerable<ProductDto>>;
        Assert.IsNotNull(response);
        Assert.IsTrue(response.Success);
        Assert.AreEqual(3, response.Data!.Count());
    }

    [TestMethod]
    public async Task GetProductById_ExistingId_ReturnsOkResult()
    {
        // Arrange
        var product = GetTestProducts().First();
        _mockProductService.Setup(s => s.GetProductByIdAsync(1))
            .ReturnsAsync(product);

        // Act
        var result = await _controller.GetProductById(1);

        // Assert
        var okResult = result.Result as OkObjectResult;
        Assert.IsNotNull(okResult);
        var response = okResult.Value as ApiResponse<ProductDto>;
        Assert.IsNotNull(response);
        Assert.IsTrue(response.Success);
        Assert.AreEqual("Test Product 1", response.Data!.Name);
    }

    [TestMethod]
    public async Task GetProductById_NonExistingId_ReturnsNotFound()
    {
        // Arrange
        _mockProductService.Setup(s => s.GetProductByIdAsync(999))
            .ReturnsAsync((Product?)null);

        // Act
        var result = await _controller.GetProductById(999);

        // Assert
        var notFoundResult = result.Result as NotFoundObjectResult;
        Assert.IsNotNull(notFoundResult);
        var response = notFoundResult.Value as ApiResponse<ProductDto>;
        Assert.IsNotNull(response);
        Assert.IsFalse(response.Success);
    }

    [TestMethod]
    public async Task CreateProduct_ValidInput_ReturnsCreatedResult()
    {
        // Arrange
        var input = new CreateProductServiceInput
        {
            Name = "New Product",
            ImageUrl = "https://example.com/image.jpg"
        };

        var createdProduct = new Product
        {
            Id = 1,
            Name = input.Name,
            ImageUrl = input.ImageUrl,
            Quantity = 0,
            Idate = DateTime.UtcNow
        };

        _mockProductService.Setup(s => s.CreateProductAsync(input.Name, input.ImageUrl))
            .ReturnsAsync(createdProduct);

        // Act
        var result = await _controller.CreateProduct(input);

        // Assert
        var createdResult = result.Result as CreatedAtActionResult;
        Assert.IsNotNull(createdResult);
        var response = createdResult.Value as ApiResponse<ProductDto>;
        Assert.IsNotNull(response);
        Assert.IsTrue(response.Success);
        Assert.AreEqual("New Product", response.Data!.Name);
    }

    [TestMethod]
    public async Task UpdateProductStock_ValidInput_ReturnsOkResult()
    {
        // Arrange
        var input = new UpdateProductStockServiceInput { Quantity = 50 };
        _mockProductService.Setup(s => s.UpdateProductStockAsync(1, 50))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.UpdateProductStock(1, input);

        // Assert
        var okResult = result.Result as OkObjectResult;
        Assert.IsNotNull(okResult);
        var response = okResult.Value as ApiResponse<object>;
        Assert.IsNotNull(response);
        Assert.IsTrue(response.Success);
    }

    [TestMethod]
    public async Task UpdateProductStock_NonExistingProduct_ReturnsNotFound()
    {
        // Arrange
        var input = new UpdateProductStockServiceInput { Quantity = 50 };
        _mockProductService.Setup(s => s.UpdateProductStockAsync(999, 50))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.UpdateProductStock(999, input);

        // Assert
        var notFoundResult = result.Result as NotFoundObjectResult;
        Assert.IsNotNull(notFoundResult);
        var response = notFoundResult.Value as ApiResponse<object>;
        Assert.IsNotNull(response);
        Assert.IsFalse(response.Success);
    }

    [TestMethod]
    public async Task UpdateProductStock_NegativeQuantity_ThrowsArgumentException()
    {
        // Arrange
        var input = new UpdateProductStockServiceInput { Quantity = -10 };
        _mockProductService.Setup(s => s.UpdateProductStockAsync(1, -10))
            .ThrowsAsync(new ArgumentException("Quantity cannot be negative"));

        // Act
        var result = await _controller.UpdateProductStock(1, input);

        // Assert
        var badRequestResult = result.Result as BadRequestObjectResult;
        Assert.IsNotNull(badRequestResult);
        var response = badRequestResult.Value as ApiResponse<object>;
        Assert.IsNotNull(response);
        Assert.IsFalse(response.Success);
    }

    private static List<Product> GetTestProducts()
    {
        return new List<Product>
        {
            new Product
            {
                Id = 1,
                Name = "Test Product 1",
                ImageUrl = "https://example.com/1.jpg",
                Price = 100.00m,
                Description = "Description 1",
                Quantity = 10,
                Idate = DateTime.UtcNow,
                Udate = DateTime.UtcNow
            },
            new Product
            {
                Id = 2,
                Name = "Test Product 2",
                ImageUrl = "https://example.com/2.jpg",
                Price = 200.00m,
                Description = "Description 2",
                Quantity = 20,
                Idate = DateTime.UtcNow,
                Udate = DateTime.UtcNow
            },
            new Product
            {
                Id = 3,
                Name = "Test Product 3",
                ImageUrl = "https://example.com/3.jpg",
                Price = 300.00m,
                Description = "Description 3",
                Quantity = 30,
                Idate = DateTime.UtcNow,
                Udate = DateTime.UtcNow
            }
        };
    }
}