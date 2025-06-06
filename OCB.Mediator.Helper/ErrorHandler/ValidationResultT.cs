﻿using OCB.Mediator.Helper.Results;
using OCB.Mediator.Helper.Abstractions.Validations;

namespace OCB.Mediator.Helper.ErrorHandler;

/// <summary>
/// <see cref="ValidationResult"/> expansion
/// </summary>
/// <typeparam name="TValue"></typeparam>
public sealed class ValidationResult<TValue> : Result<TValue>, IValidationResult
{
    /// <summary>
    /// Protected <see cref="ValidationResult{TValue}"/> constructor
    /// </summary>
    /// <param name="error"></param>
    public ValidationResult(ValidationError[] errors)
        : base(IValidationResult.ValidationErrors)
        => Errors = errors;

    public ValidationError[] Errors {get;}

    public static ValidationResult<TValue> WithErrors(ValidationError[] errors)
        => new(errors);
}