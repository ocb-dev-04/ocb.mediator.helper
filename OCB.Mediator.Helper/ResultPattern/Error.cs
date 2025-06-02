using Microsoft.AspNetCore.Http;

namespace OCB.Mediator.Helper.ResultPattern;

/// <summary>
/// Error model
/// </summary>
/// <param name="StatusCode"></param>
/// <param name="Translation"></param>
/// <param name="Description"></param>
/// <returns></returns>
public record Error(int StatusCode, string Translation, string Description)
{
    public static Error None = new(0, string.Empty, string.Empty);
    public static Error NullValue = new(500, "nullValue", "Null value was provided");

    /// <summary>
    /// Method to create a <see cref="Error"/> instance as not modified
    /// </summary>
    /// <param name="translation"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    public static Error NotModified(string? translation = default, string? message = default)
        => new(StatusCodes.Status304NotModified, translation ?? string.Empty, message ?? string.Empty);

    /// <summary>
    /// Method to create a <see cref="Error"/> instance as bad request
    /// </summary>
    /// <param name="translation"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    public static Error BadRequest(string? translation = default, string? message = default)
        => new(StatusCodes.Status400BadRequest, translation ?? string.Empty, message ?? string.Empty);

    /// <summary>
    /// Method to create a <see cref="Error"/> instance as not found
    /// </summary>
    /// <param name="translation"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    public static Error NotFound(string? translation = default, string? message = default)
        => new(StatusCodes.Status404NotFound, translation ?? string.Empty, message ?? string.Empty);

    /// <summary>
    /// Method to create a <see cref="Error"/> instance as too many request
    /// </summary>
    /// <param name="translation"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    public static Error TooManyRequest(string? translation = default, string? message = default)
        => new(StatusCodes.Status429TooManyRequests, translation ?? string.Empty, message ?? string.Empty);

    /// <summary>
    /// Method to create a <see cref="Error"/> instance as unauthorized
    /// </summary>
    /// <returns></returns>
    public static Error Unauthorized()
        => new(StatusCodes.Status401Unauthorized, string.Empty, string.Empty);
}