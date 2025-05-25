using OCB.Mediator.Helper.ErrorHandler;

namespace OCB.Mediator.Helper.Results;

/// <summary>
/// <see cref="Result"/> expansion
/// </summary>
/// <typeparam name="TValue"></typeparam>
public class Result<TValue> : Result
{
    /// <summary>
    /// Protected <see cref="Result{TValue}"/> constructor
    /// </summary>
    /// <param name="value"></param>
    /// <param name="isSuccess"></param>
    /// <param name="error"></param>
    internal Result(ValidationError error)
        : base(error)
    {
    }
}