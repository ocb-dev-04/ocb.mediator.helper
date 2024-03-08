namespace CQRS.MediatR.Helper.ErrorHandler;

/// <summary>
/// Base class to use Result design pattern
/// </summary>
public class ValidationResult
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public ValidationError Errors { get; }

    /// <summary>
    /// Protected <see cref="Result"/> constructor
    /// </summary>
    /// <param name="isSuccess"></param>
    /// <param name="error"></param>
    protected ValidationResult(bool isSuccess, ValidationError error)
    {
        IsSuccess = isSuccess;
        Errors = error;
    }
}