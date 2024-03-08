using System.Diagnostics.CodeAnalysis;

namespace CQRS.MediatR.Helper.ErrorHandler;

/// <summary>
/// <see cref="ValidationResult"/> expansion
/// </summary>
/// <typeparam name="TValue"></typeparam>
public sealed class ValidationResult<TValue> : ValidationResult
{
    private readonly TValue? _value;

    /// <summary>
    /// Protected <see cref="ValidationResult{TValue}"/> constructor
    /// </summary>
    /// <param name="value"></param>
    /// <param name="isSuccess"></param>
    /// <param name="error"></param>
    public ValidationResult(TValue? value, bool isSuccess, ValidationError error)
        : base(isSuccess, error)
    {
        _value = value;
    }

    [NotNull]
    public TValue Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("The value of a failure result can not be accessed.");

    public static implicit operator ValidationResult<TValue>(TValue? value)
        => new(value, true, ValidationError.None);
}