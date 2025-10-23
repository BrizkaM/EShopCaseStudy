//------------------------------------------------------------------------------------------
// File: ProductsControllerV2Tests.cs
//------------------------------------------------------------------------------------------
using EShopProject.Core.Entities;
using EShopProject.MessageQueue;
using EShopProject.MessageQueue.Interfaces;
using EShopProject.Services;
using EShopProject.Services.Interfaces;
using EShopProject.Services.ServiceInputs;
using EShopProject.Services.ServiceOutputs;
using EShopProject.WebApi.Controllers.V2;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace EShopProject.WebApiTest.V2;

/// <summary>
/// Unit tests for ProductsController V2 (with pagination and async queue)
/// </summary>
[TestClass]
public class ProductsControllerV2Tests
{
    private Mock<IProductService> _mockProductService = null!;
    private Mock<IStockUpdateQueue> _mockQueue = null!;
    private Mock<ILogger<ProductsController>> _mockLogger = null!;
    private ProductsController _controller = null!;

    [TestInitialize]
    public void TestInitialize()
    {
        _mockProductService = new Mock<IProductService>();
        _mockQueue = new Mock<IStockUpdateQueue>();
        _mockLogger = new Mock<ILogger<ProductsController>>();
        _controller = new ProductsController(
            _mockProductService.Object,
            _mockQueue.Object,
            _mockLogger.Object);
    }

    /// <summary>
    /// Tests GetProducts endpoint with pagination parameters, verifying correct page size and content
    /// </summary>
    [TestMethod]
    public async Task ProductsController_GetProducts_WithPagination_ReturnsOkResult()
    {
        // Arrange
        var products = GetTestProducts();
        _mockProductService.Setup(s => s.GetPagedProductsAsync(1, 10))
            .ReturnsAsync((products, 25, 3));

        // Act
        var result = await _controller.GetProducts(1, 10);

        // Assert
        var okResult = result.Result as OkObjectResult;
        Assert.IsNotNull(okResult);
        var response = okResult.Value as ApiResponse<PagedResultDto<ProductDto>>;
        Assert.IsNotNull(response);
        Assert.IsTrue(response.Success);
        Assert.AreEqual(3, response.Data!.Items.Count());
        Assert.AreEqual(25, response.Data.TotalCount);
        Assert.AreEqual(3, response.Data.TotalPages);
    }

    /// <summary>
    /// Tests GetProducts endpoint with default pagination values when no parameters are provided
    /// </summary>
    [TestMethod]
    public async Task ProductsController_GetProducts_DefaultPagination_UsesDefaultValues()
    {
        // Arrange
        var products = GetTestProducts();
        _mockProductService.Setup(s => s.GetPagedProductsAsync(1, 10))
            .ReturnsAsync((products, 3, 1));

        // Act
        var result = await _controller.GetProducts(); // No parameters = defaults

        // Assert
        var okResult = result.Result as OkObjectResult;
        Assert.IsNotNull(okResult);
        var response = okResult.Value as ApiResponse<PagedResultDto<ProductDto>>;
        Assert.IsNotNull(response);
        Assert.IsTrue(response.Success);
        Assert.AreEqual(1, response.Data!.PageNumber);
        Assert.AreEqual(10, response.Data.PageSize);
    }

    /// <summary>
    /// Tests GetProducts endpoint with custom page size parameter, verifying correct pagination settings
    /// </summary>
    [TestMethod]
    public async Task ProductsController_GetProducts_CustomPageSize_ReturnsCorrectPageSize()
    {
        // Arrange
        var products = GetTestProducts();
        _mockProductService.Setup(s => s.GetPagedProductsAsync(2, 5))
            .ReturnsAsync((products, 15, 3));

        // Act
        var result = await _controller.GetProducts(2, 5);

        // Assert
        var okResult = result.Result as OkObjectResult;
        Assert.IsNotNull(okResult);
        var response = okResult.Value as ApiResponse<PagedResultDto<ProductDto>>;
        Assert.IsNotNull(response);
        Assert.AreEqual(2, response.Data!.PageNumber);
        Assert.AreEqual(5, response.Data.PageSize);
    }

    /// <summary>
    /// Tests GetProductById endpoint with an existing product ID, expecting successful retrieval
    /// </summary>
    [TestMethod]
    public async Task ProductsController_GetProductById_ExistingId_ReturnsOkResult()
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

    /// <summary>
    /// Tests GetProductById endpoint with a non-existing ID, expecting NotFound response
    /// </summary>
    [TestMethod]
    public async Task ProductsController_GetProductById_NonExistingId_ReturnsNotFound()
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

    /// <summary>
    /// Tests CreateProduct endpoint with valid input data, expecting successful creation
    /// </summary>
    [TestMethod]
    public async Task ProductsController_CreateProduct_ValidInput_ReturnsCreatedResult()
    {
        // Arrange
        var input = new CreateProductServiceInput
        {
            Name = "New Product V2",
            ImageUrl = "https://example.com/v2.jpg"
        };

        var createdProduct = new Product
        {
            Id = 10,
            Name = input.Name,
            ImageUrl = input.ImageUrl,
            Quantity = 0,
            Idate = DateTime.UtcNow,
            Udate = DateTime.UtcNow
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
        Assert.AreEqual("New Product V2", response.Data!.Name);
    }

    /// <summary>
    /// Tests UpdateProductStock endpoint with valid input, verifying queue processing
    /// </summary>
    [TestMethod]
    public async Task ProductsController_UpdateProductStock_ValidInput_ReturnsAcceptedResult()
    {
        // Arrange
        var input = new UpdateProductStockServiceInput { Quantity = 50 };
        _mockQueue.Setup(q => q.EnqueueAsync(It.IsAny<StockUpdateRequest>()))
            .Returns(Task.CompletedTask);
        _mockQueue.Setup(q => q.Count).Returns(1);

        // Act
        var result = await _controller.UpdateProductStock(1, input);

        // Assert
        var acceptedResult = result.Result as AcceptedResult;
        Assert.IsNotNull(acceptedResult);
        var response = acceptedResult.Value as ApiResponse<object>;
        Assert.IsNotNull(response);
        Assert.IsTrue(response.Success);
        Assert.Contains("queued", response.Message!.ToLower());

        // Verify queue was called
        _mockQueue.Verify(q => q.EnqueueAsync(It.Is<StockUpdateRequest>(
            r => r.ProductId == 1 && r.Quantity == 50)), Times.Once);
    }

    /// <summary>
    /// Tests UpdateProductStock endpoint, verifying correct request data is enqueued
    /// </summary>
    [TestMethod]
    public async Task ProductsController_UpdateProductStock_EnqueuesCorrectRequest()
    {
        // Arrange
        var input = new UpdateProductStockServiceInput { Quantity = 75 };
        StockUpdateRequest? capturedRequest = null;

        _mockQueue.Setup(q => q.EnqueueAsync(It.IsAny<StockUpdateRequest>()))
            .Callback<StockUpdateRequest>(r => capturedRequest = r)
            .Returns(Task.CompletedTask);

        // Act
        await _controller.UpdateProductStock(5, input);

        // Assert
        Assert.IsNotNull(capturedRequest);
        Assert.AreEqual(5, capturedRequest.ProductId);
        Assert.AreEqual(75, capturedRequest.Quantity);
        Assert.IsLessThan(2, (DateTime.UtcNow - capturedRequest.RequestedAt).TotalSeconds);
    }

    /// <summary>
    /// Tests UpdateProductStock endpoint, verifying queue position is returned in response
    /// </summary>
    [TestMethod]
    public async Task ProductsController_UpdateProductStock_ReturnsQueuePosition()
    {
        // Arrange
        var input = new UpdateProductStockServiceInput { Quantity = 100 };
        _mockQueue.Setup(q => q.EnqueueAsync(It.IsAny<StockUpdateRequest>()))
            .Returns(Task.CompletedTask);
        _mockQueue.Setup(q => q.Count).Returns(5);

        // Act
        var result = await _controller.UpdateProductStock(1, input);

        // Assert
        var acceptedResult = result.Result as AcceptedResult;
        Assert.IsNotNull(acceptedResult);
        var response = acceptedResult.Value as ApiResponse<object>;
        Assert.IsNotNull(response);

        // Verify queue position is in response
        var data = System.Text.Json.JsonSerializer.Serialize(response.Data);
        Assert.Contains("queuePosition", data);
    }

    /// <summary>
    /// Tests UpdateProductStock endpoint with queue exception, expecting BadRequest response
    /// </summary>
    [TestMethod]
    public async Task ProductsController_UpdateProductStock_QueueException_ReturnsBadRequest()
    {
        // Arrange
        var input = new UpdateProductStockServiceInput { Quantity = 50 };
        _mockQueue.Setup(q => q.EnqueueAsync(It.IsAny<StockUpdateRequest>()))
            .ThrowsAsync(new InvalidOperationException("Queue is full"));

        // Act
        var result = await _controller.UpdateProductStock(1, input);

        // Assert
        var badRequestResult = result.Result as BadRequestObjectResult;
        Assert.IsNotNull(badRequestResult);
        var response = badRequestResult.Value as ApiResponse<object>;
        Assert.IsNotNull(response);
        Assert.IsFalse(response.Success);
    }

    /// <summary>
    /// Tests UpdateProductStock endpoint with invalid model state, expecting BadRequest response
    /// </summary>
    [TestMethod]
    public async Task ProductsController_UpdateProductStock_InvalidModelState_ReturnsBadRequest()
    {
        // Arrange
        var input = new UpdateProductStockServiceInput { Quantity = 50 };
        _controller.ModelState.AddModelError("Quantity", "Invalid quantity");

        // Act
        var result = await _controller.UpdateProductStock(1, input);

        // Assert
        var badRequestResult = result.Result as BadRequestObjectResult;
        Assert.IsNotNull(badRequestResult);
    }

    /// <summary>
    /// Tests GetProducts endpoint pagination metadata accuracy
    /// </summary>
    [TestMethod]
    public async Task ProductsController_GetProducts_PaginationMetadata_IsCorrect()
    {
        // Arrange
        var products = GetTestProducts();
        _mockProductService.Setup(s => s.GetPagedProductsAsync(2, 10))
            .ReturnsAsync((products, 25, 3));

        // Act
        var result = await _controller.GetProducts(2, 10);

        // Assert
        var okResult = result.Result as OkObjectResult;
        Assert.IsNotNull(okResult);
        var response = okResult.Value as ApiResponse<PagedResultDto<ProductDto>>;
        Assert.IsNotNull(response);

        Assert.AreEqual(2, response.Data!.PageNumber);
        Assert.AreEqual(10, response.Data.PageSize);
        Assert.AreEqual(25, response.Data.TotalCount);
        Assert.AreEqual(3, response.Data.TotalPages);
        Assert.IsTrue(response.Data.HasPreviousPage);
        Assert.IsTrue(response.Data.HasNextPage);
    }

    /// <summary>
    /// Tests GetProducts endpoint first page navigation properties
    /// </summary>
    [TestMethod]
    public async Task ProductsController_GetProducts_FirstPage_HasNoPreviousPage()
    {
        // Arrange
        var products = GetTestProducts();
        _mockProductService.Setup(s => s.GetPagedProductsAsync(1, 10))
            .ReturnsAsync((products, 25, 3));

        // Act
        var result = await _controller.GetProducts(1, 10);

        // Assert
        var okResult = result.Result as OkObjectResult;
        var response = (okResult!.Value as ApiResponse<PagedResultDto<ProductDto>>)!;

        Assert.IsFalse(response.Data!.HasPreviousPage);
        Assert.IsTrue(response.Data.HasNextPage);
    }

    /// <summary>
    /// Tests GetProducts endpoint last page navigation properties
    /// </summary>
    [TestMethod]
    public async Task ProductsController_GetProducts_LastPage_HasNoNextPage()
    {
        // Arrange
        var products = GetTestProducts();
        _mockProductService.Setup(s => s.GetPagedProductsAsync(3, 10))
            .ReturnsAsync((products, 25, 3));

        // Act
        var result = await _controller.GetProducts(3, 10);

        // Assert
        var okResult = result.Result as OkObjectResult;
        var response = (okResult!.Value as ApiResponse<PagedResultDto<ProductDto>>)!;

        Assert.IsTrue(response.Data!.HasPreviousPage);
        Assert.IsFalse(response.Data.HasNextPage);
    }

    /// <summary>
    /// Tests CreateProduct endpoint with invalid model state, expecting BadRequest response
    /// </summary>
    [TestMethod]
    public async Task ProductsController_CreateProduct_InvalidModelState_ReturnsBadRequest()
    {
        // Arrange
        var input = new CreateProductServiceInput
        {
            Name = "Test",
            ImageUrl = "invalid-url"
        };
        _controller.ModelState.AddModelError("ImageUrl", "Invalid URL format");

        // Act
        var result = await _controller.CreateProduct(input);

        // Assert
        var badRequestResult = result.Result as BadRequestObjectResult;
        Assert.IsNotNull(badRequestResult);
        var response = badRequestResult.Value as ApiResponse<ProductDto>;
        Assert.IsNotNull(response);
        Assert.IsFalse(response.Success);
        Assert.IsNotNull(response.Errors);
        Assert.IsNotEmpty(response.Errors);
    }

    /// <summary>
    /// Tests CreateProduct endpoint handling service exceptions, expecting BadRequest response
    /// </summary>
    [TestMethod]
    public async Task ProductsController_CreateProduct_ServiceThrowsException_ReturnsBadRequest()
    {
        // Arrange
        var input = new CreateProductServiceInput
        {
            Name = "Test Product",
            ImageUrl = "https://example.com/test.jpg"
        };
        _mockProductService.Setup(s => s.CreateProductAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new ArgumentException("Product name already exists"));

        // Act
        var result = await _controller.CreateProduct(input);

        // Assert
        var badRequestResult = result.Result as BadRequestObjectResult;
        Assert.IsNotNull(badRequestResult);
        var response = badRequestResult.Value as ApiResponse<ProductDto>;
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
                Quantity = 30,
                Idate = DateTime.UtcNow,
                Udate = DateTime.UtcNow
            }
        };
    }
}