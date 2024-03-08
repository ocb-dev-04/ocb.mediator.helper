using System.Diagnostics.CodeAnalysis;

namespace CQRS.MediatR.Helper.ErrorHandler;

/// <summary>
/// <see cref="Result"/> expansion
/// </summary>
/// <typeparam name="TValue"></typeparam>
public sealed class Result<TValue> : Result
{
    private readonly TValue? _value;

    /// <summary>
    /// Protected <see cref="Result{TValue}"/> constructor
    /// </summary>
    /// <param name="value"></param>
    /// <param name="isSuccess"></param>
    /// <param name="error"></param>
    public Result(TValue? value, bool isSuccess, ValidationError error)
        : base(isSuccess, error)
    {
        _value = value;
    }

    [NotNull]
    public TValue Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("The value of a failure result can not be accessed.");

    public static implicit operator Result<TValue>(TValue? value) => Create(value);
}