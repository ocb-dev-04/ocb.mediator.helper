namespace OCB.Mediator.Helper.ErrorHandler;

/// <summary>
/// Represents an error that occurs during validation, including the property name and the associated error message.
/// </summary>
/// <remarks>This type is used to encapsulate validation errors in a structured format, making it easier to
/// identify and handle issues related to specific properties during validation processes.</remarks>
/// <param name="PropertyName"></param>
/// <param name="ErrorMessage"></param>
public sealed record ValidationError(string PropertyName, string ErrorMessage)
{
    /// <summary>
    /// Represents a validation error with no associated message or field.
    /// </summary>
    /// <remarks>This static instance can be used to indicate the absence of validation errors. It contains an
    /// empty error message and an empty field name.</remarks>
    public static ValidationError None = new(string.Empty, string.Empty);
    
    /// <summary>
    /// Represents a validation error indicating that a null value was provided.
    /// </summary>
    /// <remarks>This error can be used to signal that a required value was not supplied,  typically in
    /// scenarios where null values are not allowed.</remarks>
    public static ValidationError NullValue = new("nullValue", "Null value was provided");
}