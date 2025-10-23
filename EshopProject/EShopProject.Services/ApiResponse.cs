//------------------------------------------------------------------------------------------
// File: ApiResponse.cs
//------------------------------------------------------------------------------------------
namespace EShopProject.Services;

/// <summary>
/// Standard API response wrapper
/// </summary>
public class ApiResponse<T>
{
    /// <summary>
    /// Gets or sets a value indicating whether the API operation was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets the data payload returned by the API operation
    /// </summary>
    public T? Data { get; set; }

    /// <summary>
    /// Gets or sets a human-readable message describing the result of the operation
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// Gets or sets a list of error messages when the operation fails
    /// </summary>
    public List<string>? Errors { get; set; }

    /// <summary>
    /// Creates a successful API response with the specified data and optional message
    /// </summary>
    /// <param name="data">The data to include in the response</param>
    /// <param name="message">Optional message describing the successful operation</param>
    /// <returns>A new ApiResponse instance indicating success</returns>
    public static ApiResponse<T> SuccessResponse(T data, string? message = null)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Data = data,
            Message = message
        };
    }

    /// <summary>
    /// Creates an error API response with the specified message and optional error details
    /// </summary>
    /// <param name="message">The error message describing what went wrong</param>
    /// <param name="errors">Optional list of detailed error messages</param>
    /// <returns>A new ApiResponse instance indicating failure</returns>
    public static ApiResponse<T> ErrorResponse(string message, List<string>? errors = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            Errors = errors
        };
    }
}