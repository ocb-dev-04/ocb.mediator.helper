namespace CQRS.MediatR.Helper.ErrorHandler;

/// <summary>
/// Model to create validation errors objects
/// </summary>
/// <param name="PropertyName"></param>
/// <param name="ErrorMessage"></param>
/// <returns></returns>
public sealed record ValidationError(string PropertyName, string ErrorMessage)
{
    public static ValidationError None = new(string.Empty, string.Empty);
    public static ValidationError NullValue = new("nullValue", "Null value was provided");
}