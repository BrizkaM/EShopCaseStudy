//------------------------------------------------------------------------------------------
// File: ProductsController.cs
//------------------------------------------------------------------------------------------
using EShopProject.Core.Entities;
using EShopProject.Services;
using EShopProject.Services.Interfaces;
using EShopProject.Services.ServiceInputs;
using EShopProject.Services.ServiceOutputs;
using Microsoft.AspNetCore.Mvc;

namespace EShopProject.WebApi.Controllers.V1;

/// <summary>
/// Products API controller
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/products")]
[Produces("application/json")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(IProductService productService, ILogger<ProductsController> logger)
    {
        _productService = productService;
        _logger = logger;
    }

    /// <summary>
    /// Get all products
    /// </summary>
    /// <returns>List of all products</returns>
    /// <response code="200">Returns the list of products</response>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ProductDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<ProductDto>>>> GetAllProducts()
    {
        _logger.LogInformation("GET /api/v1/products");
        var products = await _productService.GetAllProductsAsync();
        var productDtos = products.Select(MapToDto);
        return Ok(ApiResponse<IEnumerable<ProductDto>>.SuccessResponse(productDtos));
    }

    /// <summary>
    /// Get a single product by ID
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <returns>Product details</returns>
    /// <response code="200">Returns the product</response>
    /// <response code="404">Product not found</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ProductDto>>> GetProductById(int id)
    {
        // this is version v1 endpoint
        _logger.LogInformation("GET /api/v1/products/{Id} - Retrieving product", id);

        var product = await _productService.GetProductByIdAsync(id);

        if (product == null)
        {
            _logger.LogWarning("Product with ID {Id} not found", id);
            return NotFound(ApiResponse<ProductDto>.ErrorResponse($"Product with ID {id} not found"));
        }

        return Ok(ApiResponse<ProductDto>.SuccessResponse(
            MapToDto(product),
            "Product retrieved successfully"));
    }

    /// <summary>
    /// Create a new product
    /// </summary>
    /// <param name="createDto">Product creation data (name and imageUrl required)</param>
    /// <returns>Created product</returns>
    /// <response code="201">Product created successfully</response>
    /// <response code="400">Invalid input data</response>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<ProductDto>>> CreateProduct(
        [FromBody] CreateProductServiceInput createDto)
    {
        _logger.LogInformation("POST /api/v1/products");

        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<ProductDto>.ErrorResponse("Invalid input", errors));
        }

        try
        {
            var product = await _productService.CreateProductAsync(createDto.Name, createDto.ImageUrl);
            return CreatedAtAction(nameof(GetProductById), new { id = product.Id },
                ApiResponse<ProductDto>.SuccessResponse(MapToDto(product)));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<ProductDto>.ErrorResponse(ex.Message));
        }
    }

    /// <summary>
    /// Update product stock quantity
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <param name="updateDto">Stock update data</param>
    /// <returns>Success status</returns>
    /// <response code="200">Stock updated successfully</response>
    /// <response code="400">Invalid input data</response>
    /// <response code="404">Product not found</response>
    [HttpPatch("{id}/stock")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<object>>> UpdateProductStock(
        int id,
        [FromBody] UpdateProductStockServiceInput updateDto)
    {
        _logger.LogInformation("PATCH /api/v1/products/{Id}/stock", id);

        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<object>.ErrorResponse("Invalid input", errors));
        }

        try
        {
            var success = await _productService.UpdateProductStockAsync(id, updateDto.Quantity);
            if (!success)
                return NotFound(ApiResponse<object>.ErrorResponse($"Product with ID {id} not found"));

            return Ok(ApiResponse<object>.SuccessResponse(new { productId = id, quantity = updateDto.Quantity }));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
        }
    }

    // IDate, UDate properties skipped as they are DB info only
    private static ProductDto MapToDto(Product product)
    {
        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            ImageUrl = product.ImageUrl,
            Price = product.Price,
            Description = product.Description,
            Quantity = product.Quantity,
        };
    }
}