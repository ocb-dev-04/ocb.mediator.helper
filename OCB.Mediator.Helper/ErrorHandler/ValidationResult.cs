﻿using OCB.Mediator.Helper.Abstractions.Validations;
using OCB.Mediator.Helper.Results;

namespace OCB.Mediator.Helper.ErrorHandler;

/// <summary>
/// Base class to use Result design pattern
/// </summary>
public class ValidationResult : Result, IValidationResult
{
    /// <summary>
    /// Protected <see cref="Result"/> constructor
    /// </summary>
    /// <param name="isSuccess"></param>
    /// <param name="error"></param>
    private ValidationResult(ValidationError[] errors)
        : base(IValidationResult.ValidationErrors)
        => Errors = errors;

    public ValidationError[] Errors {get;}

    public static ValidationResult WithErrors(ValidationError[] errors)
        => new(errors);
}