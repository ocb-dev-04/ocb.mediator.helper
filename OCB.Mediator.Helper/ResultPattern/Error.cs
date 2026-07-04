namespace OCB.Mediator.Helper.ResultPattern;

/// <summary>
/// Represents an error with an associated HTTP status code, translation key, and description.
/// </summary>
/// <remarks>The <see cref="Error"/> record is used to encapsulate error information in a structured format. It
/// includes a status code, a translation key for localization purposes, and a description of the error. Common
/// predefined errors are available as static instances, such as <see cref="None"/> and <see cref="NullValue"/>.
/// Additionally, factory methods are provided to create specific error instances for common HTTP status
/// codes.</remarks>
/// <param name="StatusCode"></param>
/// <param name="Translation"></param>
/// <param name="Description"></param>
public record Error(int StatusCode, string Translation, string Description)
{
    public static readonly Error None = new(0, string.Empty, string.Empty);
    public static readonly Error NullValue = new(500, "nullValue", "Null value was provided");

    /// <summary>
    /// Creates an <see cref="Error"/> instance representing a "Not Modified" (HTTP 304) status.
    /// </summary>
    /// <param name="translation">An optional string providing a localized or alternative representation of the error. If <paramref
    /// name="translation"/> is <see langword="null"/>, an empty string is used.</param>
    /// <param name="message">An optional string containing additional details about the error. If <paramref name="message"/> is <see
    /// langword="null"/>, an empty string is used.</param>
    /// <returns>An <see cref="Error"/> object with a status code of 304 (Not Modified), and the specified translation and
    /// message values.</returns>
    public static Error NotModified(string? translation = default, string? message = default)
        => new(304, translation ?? string.Empty, message ?? string.Empty);

    /// <summary>
    /// Creates an <see cref="Error"/> instance representing a 400 Bad Request HTTP status code.
    /// </summary>
    /// <param name="translation">An optional string representing a translation key or identifier for the error message. If not provided, defaults
    /// to an empty string.</param>
    /// <param name="message">An optional string containing the error message to be associated with the response. If not provided, defaults to
    /// an empty string.</param>
    /// <returns>An <see cref="Error"/> object initialized with a 400 Bad Request status code, the specified translation key, and
    /// the specified error message.</returns>
    public static Error BadRequest(string? translation = default, string? message = default)
        => new(400, translation ?? string.Empty, message ?? string.Empty);

    /// <summary>
    /// Creates an <see cref="Error"/> instance representing a "Not Found" (404) HTTP status code.
    /// </summary>
    /// <param name="translation">An optional string providing a localized translation or description of the error. If <paramref
    /// name="translation"/> is <see langword="null"/>, an empty string is used.</param>
    /// <param name="message">An optional string containing additional details or context about the error. If <paramref name="message"/> is
    /// <see langword="null"/>, an empty string is used.</param>
    /// <returns>An <see cref="Error"/> object initialized with a 404 status code, the specified translation, and message.</returns>
    public static Error NotFound(string? translation = default, string? message = default)
        => new(404, translation ?? string.Empty, message ?? string.Empty);

    /// <summary>
    /// Creates an error representing a "Too Many Requests" (HTTP 429) response.
    /// </summary>
    /// <param name="translation">An optional translation string that provides additional context or localized information. If not specified, an
    /// empty string is used.</param>
    /// <param name="message">An optional message describing the error in more detail. If not specified, an empty string is used.</param>
    /// <returns>An <see cref="Error"/> instance with a status code of 429 (Too Many Requests), containing the provided
    /// translation and message.</returns>
    public static Error TooManyRequest(string? translation = default, string? message = default)
        => new(429, translation ?? string.Empty, message ?? string.Empty);

    /// <summary>
    /// Creates an <see cref="Error"/> instance representing an unauthorized access error.
    /// </summary>
    /// <returns>An <see cref="Error"/> object with a status code of 401 (Unauthorized) and empty
    /// message and details.</returns>
    public static Error Unauthorized()
        => new(401, string.Empty, string.Empty);

    /// <summary>
    /// Creates an error representing an internal server error (HTTP status code 500).
    /// </summary>
    /// <param name="translation">An optional localized message describing the error. If null, an empty string is used.</param>
    /// <param name="message">An optional detailed message providing additional information about the error. If null, an empty string is used.</param>
    /// <returns>An <see cref="Error"/> instance with a status code of 500 (Internal Server Error).</returns>
    public static Error InternalServerError(string? translation = default, string? message = default)
        => new(500, translation ?? string.Empty, message ?? string.Empty);

    /// <summary>
    /// Creates an error representing an internal server exception with the specified translation key and message.
    /// </summary>
    /// <param name="translation">The translation key that identifies the localized error message to display to the user. Cannot be null or empty.</param>
    /// <param name="message">The detailed error message describing the exception. Cannot be null or empty.</param>
    /// <returns>An Error object with a status code of 500 (Internal Server Error), containing the specified translation key and
    /// message.</returns>
    public static Error Exception(string translation, string message)
        => new(500, translation, message);
}