using OCB.Mediator.Helper.ErrorHandler;

namespace OCB.Mediator.Helper.ValidationResults;

/// <summary>
/// <see cref="ValidationResult"/> expansion
/// </summary>
/// <typeparam name="TValue"></typeparam>
public class ValidationResult<TValue> : ValidationResult
{
    /// <summary>
    /// Protected <see cref="ValidationResult{TValue}"/> constructor
    /// </summary>
    /// <param name="value"></param>
    /// <param name="isSuccess"></param>
    /// <param name="error"></param>
    internal ValidationResult(ValidationError error)
        : base(error)
    {
    }
}