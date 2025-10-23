//------------------------------------------------------------------------------------------
// File: PagedResultDto.cs
//------------------------------------------------------------------------------------------
namespace EShopProject.Services.ServiceOutputs;

/// <summary>
/// Generic paginated result DTO
/// </summary>
public class PagedResultDto<T>
{
    /// <summary>
    /// Gets or sets the collection of items for the current page
    /// </summary>
    public IEnumerable<T> Items { get; set; } = new List<T>();

    /// <summary>
    /// Gets or sets the current page number (1-based)
    /// </summary>
    public int PageNumber { get; set; }

    /// <summary>
    /// Gets or sets the number of items per page
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Gets or sets the total number of items across all pages
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Gets or sets the total number of available pages
    /// </summary>
    public int TotalPages { get; set; }

    /// <summary>
    /// Gets a value indicating whether a previous page exists (true if current page > 1)
    /// </summary>
    public bool HasPreviousPage => PageNumber > 1;

    /// <summary>
    /// Gets a value indicating whether a next page exists (true if current page < total pages)
    /// </summary>
    public bool HasNextPage => PageNumber < TotalPages;
}