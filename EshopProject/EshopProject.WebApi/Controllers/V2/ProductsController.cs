using EShopProject.Core.Entities;
using EShopProject.MessageQueue;
using EShopProject.MessageQueue.Interfaces;
using EShopProject.Services;
using EShopProject.Services.Interfaces;
using EShopProject.Services.ServiceInputs;
using EShopProject.Services.ServiceOutputs;
using Microsoft.AspNetCore.Mvc;

namespace EShopProject.WebApi.Controllers.V2;

/// <summary>
/// API v2 - Products management with pagination support (default page size 10)
/// All list endpoints now support pagination
/// Stock updates are processed asynchronously via queue
/// </summary>
[ApiController]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/products")]
[Produces("application/json")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly IStockUpdateQueue _stockUpdateQueue;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(
        IProductService productService,
        IStockUpdateQueue stockUpdateQueue,
        ILogger<ProductsController> logger)
    {
        _productService = productService;
        _stockUpdateQueue = stockUpdateQueue;
        _logger = logger;
    }

    /// <summary>
    /// Get paginated products (V2 - with pagination support)
    /// </summary>
    /// <param name="pageNumber">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 10, max: 100)</param>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResultDto<ProductDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResultDto<ProductDto>>>> GetProducts(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        _logger.LogInformation("GET /api/v2/products?pageNumber={PageNumber}&pageSize={PageSize}", pageNumber, pageSize);

        var (items, totalCount, totalPages) = await _productService.GetPagedProductsAsync(pageNumber, pageSize);
        var productDtos = items.Select(MapToDto);

        var pagedResult = new PagedResultDto<ProductDto>
        {
            Items = productDtos,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages
        };

        return Ok(ApiResponse<PagedResultDto<ProductDto>>.SuccessResponse(pagedResult, "Products retrieved successfully"));
    }

    /// <summary>
    /// Get product by ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<ProductDto>>> GetProductById(int id)
    {
        _logger.LogInformation("GET /api/v2/products/{Id}", id);
        var product = await _productService.GetProductByIdAsync(id);

        if (product == null)
            return NotFound(ApiResponse<ProductDto>.ErrorResponse($"Product with ID {id} not found"));

        return Ok(ApiResponse<ProductDto>.SuccessResponse(MapToDto(product), "Product retrieved successfully"));
    }

    /// <summary>
    /// Create new product
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<ProductDto>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<ProductDto>>> CreateProduct([FromBody] CreateProductServiceInput createDto)
    {
        _logger.LogInformation("POST /api/v2/products");

        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<ProductDto>.ErrorResponse("Invalid input", errors));
        }

        try
        {
            var product = await _productService.CreateProductAsync(createDto.Name, createDto.ImageUrl);
            return CreatedAtAction(nameof(GetProductById), new { id = product.Id },
                ApiResponse<ProductDto>.SuccessResponse(MapToDto(product), "Product created successfully"));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ApiResponse<ProductDto>.ErrorResponse(ex.Message));
        }
    }

    /// <summary>
    /// Update product stock (V2 - asynchronous processing via queue)
    /// The request is queued and processed by a background service
    /// </summary>
    [HttpPatch("{id}/stock")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResponse<object>>> UpdateProductStock(int id, [FromBody] UpdateProductStockServiceInput updateDto)
    {
        _logger.LogInformation("PATCH /api/v2/products/{Id}/stock (async queue)", id);

        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<object>.ErrorResponse("Invalid input", errors));
        }

        try
        {
            var request = new StockUpdateRequest
            {
                ProductId = id,
                Quantity = updateDto.Quantity
            };

            await _stockUpdateQueue.EnqueueAsync(request);

            _logger.LogInformation("Stock update queued - ProductId: {ProductId}, Queue size: {QueueSize}", id, _stockUpdateQueue.Count);

            return Accepted(ApiResponse<object>.SuccessResponse(
                new
                {
                    productId = id,
                    quantity = updateDto.Quantity,
                    status = "queued",
                    queuePosition = _stockUpdateQueue.Count,
                    message = "Stock update will be processed asynchronously within a few seconds"
                },
                "Stock update request accepted and queued for processing"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error queueing stock update");
            return BadRequest(ApiResponse<object>.ErrorResponse("Failed to queue stock update"));
        }
    }

    private static ProductDto MapToDto(Product product) => new()
    {
        Id = product.Id,
        Name = product.Name,
        ImageUrl = product.ImageUrl,
        Price = product.Price,
        Description = product.Description,
        Quantity = product.Quantity
    };
}