using EShopProject.Business.Entities;
using EShopProject.Services;
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
    /// <summary>
    /// Get all products
    /// </summary>
    /// <returns>List of all products</returns>
    /// <response code="200">Returns the list of products</response>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ProductDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IEnumerable<ProductDto>>>> GetAllProducts()
    {
        throw new NotImplementedException();
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
        throw new NotImplementedException();
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
        throw new NotImplementedException();
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
        throw new NotImplementedException();
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