namespace OCB.Mediator.Helper.ResultPattern;

/// <summary>
/// Represents the status of a request operation.
/// </summary>
/// <remarks>This enumeration is used to indicate the outcome of a request, such as whether it was successful,
/// encountered an error, or was unauthorized. The values can be used to determine the appropriate action or response
/// based on the result of the operation.</remarks>
public enum RequestResultStatus
{
    /// <summary>
    /// Represents the completion status of an operation.
    /// </summary>
    Done = 0,

    /// <summary>
    /// Represents an HTTP status code indicating that the requested resource could not be found.
    /// </summary>
    /// <remarks>This status code is typically used to indicate that the server could not locate the requested
    /// resource. It is commonly associated with the HTTP 404 Not Found response.</remarks>
    NotFound,

    /// <summary>
    /// Represents an exception that occurs when a user is unauthorized to perform a specific action.
    /// </summary>
    /// <remarks>This exception can be used to indicate that an operation failed due to insufficient
    /// permissions or authentication. It is typically thrown when a user attempts to access a resource or perform an
    /// action without proper authorization.</remarks>
    Unauth,

    /// <summary>
    /// Represents an error that occurred during the execution of an operation.
    /// </summary>
    /// <remarks>This class can be used to encapsulate details about an error, such as its message, type, or
    /// other relevant information.</remarks>
    Error
}